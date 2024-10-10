using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Passi.Core.Application.Options;
using Passi.Core.Application.Repositories;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Exceptions;
using Passi.Core.Extensions;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Web;

namespace Passi.Core.Services
{

    /// <summary>
    /// Rimuovere il public (ma capire perchè non funziona il visible to)
    /// </summary>
    internal class AuthenticationService : IPassiAuthenticationService
    {
        private readonly IHttpContextAccessor accessor;
        private readonly IInfoRepository<ProfileInfo> profileInfoRepository;
        private readonly IInfoRepository<SessionInfo> sessionInfoRepository;
        private readonly IInfoRepository<ConventionInfo> conventionInfoRepository;
        private readonly IInfoRepository<ContactCenterInfo> contactCenterRepository;
        private readonly IDataCypherService dataCypher;
        private readonly IUserRepository userRepository;
        private readonly ILevelsRepository levelsRepository;
        private readonly ILogger logger;
        private readonly ConfigurationOptions configurationOptions;
        private readonly UrlOptions urlOptions;

        public AuthenticationService(IHttpContextAccessor accessor,
            IInfoRepository<ProfileInfo> profileInfoRepository,
            IInfoRepository<SessionInfo> sessionInfoRepository,
            IInfoRepository<ConventionInfo> conventionInfoRepository,
            IInfoRepository<ContactCenterInfo> contactCenterRepository,
            IDataCypherService dataCypher,
            IUserRepository userRepository,
            ILevelsRepository levelsRepository,
            ILogger<AuthenticationService> logger,
            IOptions<UrlOptions> urlOptions,
            IOptions<ConfigurationOptions> configurationOptions)
        {
            this.accessor = accessor;
            this.profileInfoRepository = profileInfoRepository;
            this.sessionInfoRepository = sessionInfoRepository;
            this.conventionInfoRepository = conventionInfoRepository;
            this.contactCenterRepository = contactCenterRepository;
            this.dataCypher = dataCypher;
            this.userRepository = userRepository;
            this.levelsRepository = levelsRepository;
            this.logger = logger;
            this.configurationOptions = configurationOptions.Value;
            this.urlOptions = urlOptions.Value;

        }

        #region Verifiche
        /// <summary>
        /// Verifica che l'utente loggato sia lo stesso utente del profilo,
        /// andando a verificare che il codice fiscale in entrambi i cookie sia lo stesso
        /// Se i codici fiscali sono diversi, l'utente viene fatto sloggare
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <param name="profileInfo"></param>
        /// <exception cref="PassiUnauthorizedException"></exception>
        private void CheckSameUser(SessionInfo sessionInfo, ProfileInfo profileInfo)
        {
            if (profileInfo.HasProfile)
            {
                var profileUserId = profileInfo.Id;
                logger.LogInformation($"CheckAuthentication - Cookie Profilo presente");

                // se il codice fiscale del cookie di sessione è diverso dal codice fiscale del
                // cookie di profilo vado a logout
                if (sessionInfo.UserId != profileInfo.FiscalCode)
                {
                    logger.LogInformation("CheckAuthentication - CF cookie profilo diverso da sessione: {profileUserId}", profileUserId);
                    throw new PassiUnauthorizedException(urlOptions.Logout, ErrorCodes.Three, Reason.SessionUserIdDifferentProfileUserId, true);
                }
            }
        }

        /// <summary>
        /// Verifica che la data dell'ultimo aggiornamento del cookie di sessione non sia troppo vecchia
        /// Se è stato superato il timeout, l'utente viene sloggato
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <exception cref="PassiUnauthorizedException"></exception>
        private void CheckSessionTimeout(SessionInfo sessionInfo)
        {
            var userId = sessionInfo.UserId;
            var now = DateTime.UtcNow;
            if (now - sessionInfo.LastUpdated > sessionInfo.SessionTimeout)
            {
                logger.LogInformation("CheckAuthentication - Sessione scaduta, {userId}", userId);
                throw new PassiUnauthorizedException(urlOptions.Logout, ErrorCodes.One, Reason.SessionExpiredForTimeout, true);
            }
        }

        /// <summary>
        /// Dopo un certo tempo dal login, l'utente va comunque sloggato.
        /// Verifichiamo il tempo intercorso tra la data di login e il timeout oltre il quale l'utente va fatto sloggare
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <exception cref="PassiUnauthorizedException"></exception>
        private void CheckLoginTimeout(SessionInfo sessionInfo)
        {
            var userId = sessionInfo.UserId;
            var now = DateTime.UtcNow;
            if (now - sessionInfo.LoggedIn > sessionInfo.SessionMaximumTime)
            {
                logger.LogInformation("CheckAuthentication - Sessione generale scaduta - {userId}", userId);
                throw new PassiUnauthorizedException(urlOptions.Logout, ErrorCodes.Two, Reason.SessionExpiredForMaximumTime, true);
            }
        }

        /// <summary>
        /// E' possibile che per l'accesso ad un servizio possa essere richiesto uno specifico profilo 
        /// (In pratica gestisce l'autorizzazione)
        /// Verifichiamo se
        /// 1) L'utente è loggato col profilo desiderato
        /// 2) Se non lo è se ha un profilo valido
        /// 3) Se lo ha vediamo se ha permessi sufficienti
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="sessionInfo"></param>
        /// <param name="profileInfo"></param>
        /// <returns></returns>
        private async Task CheckProfileAsync(int serviceId, SessionInfo sessionInfo, ProfileInfo profileInfo, Uri returnUrl)
        {
            // Esploriamo prima le casistiche nelle quali non serve il srcPortal (id profilo richiesto)
            var requiredUserTypeId = RequiredUserTypeId();
            var isForcedDifferentProfileRequired = profileInfo.HasProfile
                && requiredUserTypeId != null && profileInfo.ProfileTypeId != requiredUserTypeId;

            // Caso 1: Non ho bisogno di avere un profilo per loggarmi
            if (serviceId == SpecialServices.NoProfileNeeded)
                return;

            // Caso 2: Ho un profilo e il servizio non ne richiede uno specifico 
            if (serviceId == SpecialServices.EveryProfileIsOK
                && profileInfo.HasProfile && !isForcedDifferentProfileRequired)
            {
                return;
            }

            // Caso 3: Ho il servizio richiesto tra quelli specificati nel cookie di profilo e non è richiesto 
            // un differente profilo specifico 
            if (profileInfo.HasProfile
                && profileInfo.Services.Any(x => x.Id == serviceId)
                && !isForcedDifferentProfileRequired)
            {
                return;
            }

            // Caso 4: Il servizio non è tra quelli autorizzati per questo profilo oppure per il profilo forzato.
            // La lista dei servizi viene valorizzata dentro InfoRepository attraverso il cookie di profilo.
            // Se nel cookie non ci sta il servizio richiesto, richiedo al DB l'intera lista dei servizi per l'utente, non filtrata per ente.
            // Ma anche dal db è possibile che non ci sia il servizio richiesto. In questo caso si richiede la login.

            ICollection<Service> dbServices = await userRepository.AuthorizedServicesAsync(
                    sessionInfo.FiscalCode,
                    string.Empty,
                    sessionInfo.AuthenticationType,
                    true);

            bool hasOtherAvailableProfiles = dbServices.Any() && dbServices.Any(x => x.Id == serviceId);

            if (serviceId > SpecialServices.EveryProfileIsOK && !hasOtherAvailableProfiles)
            {
                var url = urlOptions.ErrorPage;
                url = url
                    .AddToQueryString(Keys.ErrorMessage, ErrorCodes.Four);

                throw new PassiUnauthorizedException(url, Reason.ServiceUnavailable);
            }

            // Se non ho un profilo
            // o il servizio ne richiede uno specifico diverso dal profilo loggato,
            // devo far capire all'utente che deve switchare profilo

            // Prendo i profili disponibili dal DB
            var userProfiles = await UserProfilesAsync(serviceId, sessionInfo);

            // Per accedere devo avere almeno un profilo disponibile!
            // Se non ce l'ho, allora faccio rifare l'autenticazione
            if (!userProfiles.Any())
            {
                var url = urlOptions.ErrorPage;
                url = url
                    .AddToQueryString(Keys.ErrorMessage, ErrorCodes.Four);

                throw new PassiUnauthorizedException(url, Reason.ProfileNotFound);
            }
            // (1) Se non è richiesto forzatamente un profilo specifico e il profilo attuale 
            // è diverso da quelli disponibili per questo id Servizio,
            // chiedo di cambiare profilo
            var noSpecificProfileRequired = profileInfo.HasProfile && requiredUserTypeId == null;

            if (noSpecificProfileRequired && !userProfiles.Any(a => a.InstitutionCode == profileInfo.InstitutionCode))
            {
                var service = profileInfo.Services.FirstOrDefault(x => x.Id == serviceId) ?? dbServices.First(x => x.Id == serviceId);
                // vs: tolto il check service != null perché il service in questo caso ci sarà
                // sempre, almeno nei dbServices: infatti, se non c'è vuol dire che andrà a false
                // la hasOtherAvailableProfiles, ma in quel caso, se il serviceId è normale allora
                // si andrà in eccezione sopra, mentre se il serviceId è tra gli speciali
                // allora si va in OK da prima
                bool isAuthorized = await levelsRepository.CompareAuthorizationAsync(sessionInfo.AuthenticationType.ShortDescribe(), service.RequiredAuthenticationType);
                if (isAuthorized)
                {
                    throw SwitchProfile(sessionInfo.SessionId, serviceId, sessionInfo.AuthenticationType, Reason.ProfileNotFound, returnUrl);
                }
                else
                {
                    // Switch profile+level
                    throw SwitchLevelThenProfile(sessionInfo.SessionId, serviceId, service.RequiredAuthenticationType, Reason.LevelSwitchNeeded, returnUrl);
                }
            }

            // (2) oppure se richiedo uno specifico profilo e l'utente ha questo profilo,
            // ma non è quello con il quale si è loggato
            // chiedo di cambiare profilo
            if (isForcedDifferentProfileRequired)
            {
                if (userProfiles.Any(x => x.ProfileTypeId == requiredUserTypeId))
                {
                    throw SwitchProfile (sessionInfo.SessionId, serviceId, sessionInfo.AuthenticationType, Reason.ProfileNotFound, returnUrl);
                }
                else
                {
                    // questo potrebbe succedere se srcPortal è forzato con un valore di tipo utente
                    // non presente tra quelli disponibili per l'utente loggato
                    var url = urlOptions.ErrorPage;
                    url = url.AddToQueryString(Keys.ErrorMessage, ErrorCodes.Four);

                    throw new PassiUnauthorizedException(url, Reason.ProfileNotFound);
                }

            }

            // Indipendentemente dal se richiedo uno specifico profilo o meno
            // se l'utente non ha un profilo
            // ma ne ha alcuni disponibili per questo id Servizio
            // chiedo di cambiare profilo
            // vs: tolto il check su userProfiles.Any() visto che se fosse false sarebbe uscito prima
            if (!profileInfo.HasProfile)
            {
                throw SwitchProfile(sessionInfo.SessionId, serviceId, sessionInfo.AuthenticationType, Reason.ProfileNotFound, returnUrl);
            }

            // Se tutti i controlli preliminari non hanno dato esito, vado su logout
            var urlLogout = urlOptions.ErrorPage;
            urlLogout = urlLogout
                    .AddToQueryString(Keys.ErrorMessage, ErrorCodes.Four);

            throw new PassiUnauthorizedException(urlLogout, Reason.ProfileNotFound, true);
        }

        /// <summary>
        /// Se l'utente ha un profilo, e il servizio richiede uno specifico profilo,
        /// andiamo a verificare che l'utente abbia il servizio tra i suoi servizi abilitati
        /// e che il suo tipo di autenticazione sia sufficiente per visualizzare il servizio
        /// Se il servizio non è tra quelli abilitati o se il profilo non è sufficiente, viene dato errore
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="sessionInfo"></param>
        /// <param name="profileInfo"></param>
        /// <returns></returns>
        private async Task CheckLevelAsync(int serviceId, SessionInfo sessionInfo, ProfileInfo profileInfo, Uri returnUrl)
        {
            var sessionId = sessionInfo.SessionId;

            var requiredProfile = RequiredUserTypeId();

            /// Supponiamo che l'utente sia profilato e che il profilo sia quello richiesto
            if (serviceId > 0 && profileInfo.HasProfile && (requiredProfile == null || requiredProfile.Value == profileInfo.ProfileTypeId))
            {
                var myService = profileInfo.Services.FirstOrDefault(x => x.Id == serviceId) ?? throw new PassiUnauthorizedException(urlOptions.Logout, ErrorCodes.Three, Reason.ServiceUnavailable, true);

                /// Verifichiamo se l'autenticazione dell'utente è sufficientemente robusta
                var isProfileAuthorized = await levelsRepository.CompareAuthorizationAsync(sessionInfo.AuthenticationType.ShortDescribe(), myService.RequiredAuthenticationType);
                if (!isProfileAuthorized)
                {
                    throw SwitchLevel(sessionId, myService.RequiredAuthenticationType, Reason.LevelSwitchNeeded, returnUrl);
                }
            }
        }

        /// <summary>
        /// Verifica della validità della sessione
        /// La sessione è ancora valida se l'ultimo aggiornamento non risale a troppo tempo fa
        /// Se però il flag di controllo è minore di 3 non diamo errore per consentire i controlli in dev
        /// </summary>
        /// <param name="sessionInfo"></param>
        /// <returns></returns>
        /// <exception cref="PassiUnauthorizedException"></exception>
        private async Task CheckActiveSessionAsync(SessionInfo sessionInfo)
        {
            var now = DateTime.UtcNow;
            var sessionManagementFlag = 0;
            if (int.TryParse(configurationOptions.SessionManagementFlag, out int value))
            {
                sessionManagementFlag = value;
            }

            //Controllo che la sessione tipicamente usata per controllo/sviluppo sia ancora attiva
            if (sessionManagementFlag > 0 && (now - sessionInfo.LastSessionUpdate > sessionInfo.SessionMaximumCachingTime))
            {
                // viene effettuato il check della sessione unica ed aggiornato il cookie di sessione
                bool isValidSession = await userRepository.HasSessionAsync(sessionInfo.UserId,
                    sessionInfo.SessionId,
                    sessionManagementFlag);

                sessionInfo.LastSessionUpdate = now;

                if (!isValidSession)
                {
                    logger.LogInformation("CheckAuthentication - Sessione sostituita - {sessionId}", sessionInfo.SessionId);
                    switch (sessionManagementFlag)
                    {
                        //manca il case: 1 la scrittura del log sulla sessione unica
                        case 3:
                            throw new PassiUnauthorizedException(urlOptions.Logout, ErrorCodes.Seven, Reason.SessionExpiredForMaximumTime, true);
                        default:
                            break;
                    }
                    sessionInfo.HasSessionFlag = true;
                }
            }
        }

        private void CheckTimeSlots(int serviceId, SessionInfo sessionInfo, ProfileInfo profileInfo)
        {
            // gestione fascia oraria

            if (profileInfo.HasProfile && serviceId != SpecialServices.NoProfileNeeded && !profileInfo.IsTimeSlotValid)
            {
                var secureString = dataCypher.Secure(new NameValueCollection() {
                    { "descrizioneEnte", sessionInfo.InstitutionDescription },
                    { "inizioFasciaOraria", profileInfo.Opening.ToLocalTime().ToString(Keys.HourFormat) },
                    { "fineFasciaOraria", profileInfo.Closing.ToLocalTime().ToString(Keys.HourFormat) }
                });

                var url = urlOptions.ErrorPage
                    .AddToQueryString(Keys.ErrorMessage, ErrorCodes.EightyOne)
                    .AddToQueryString(Keys.Secure, HttpUtility.UrlEncode(secureString));

                throw new PassiUnauthorizedException(url, Reason.TimeSlotClosed);
            }
        }

        private async Task CheckDelegationAsync(SessionInfo sessionInfo, ProfileInfo profileInfo)
        {
            var profileMustBeRenewed = profileInfo.HasProfile && !profileInfo.IsStillValid;
            if (profileMustBeRenewed
                && !string.IsNullOrEmpty(sessionInfo.DelegateUserId)
                && sessionInfo.DelegateUserId != sessionInfo.UserId)
            {
                var hasDelegation = await userRepository.HasDelegationAsync(sessionInfo.UserId, sessionInfo.DelegateUserId);
                if (!hasDelegation)
                {
                    throw new PassiUnauthorizedException(urlOptions.Logout, ErrorCodes.Three, Reason.DelegationNotFound, true);
                }
            }
        }

        private async Task CheckConventions(int serviceId, SessionInfo sessionInfo, ProfileInfo profileInfo, ConventionInfo conventionInfo)
        {
            /// Operazioni preliminari: valorizziamo le convenzioni se non ci sono
            /// Le convenzioni devono essere controllate solamente se "serviceId" != 0 (EveryProfileOk)

            if (serviceId != SpecialServices.EveryProfileIsOK && profileInfo.HasProfile && profileInfo.Services.Any(a => a.HasConvention && a.Id == serviceId))
            {
                conventionInfo.LastUpdate = profileInfo.LastUpdate;
                conventionInfo.Timeout = profileInfo.Timeout;

                // Se le convenzioni devono essere ricaricate perché è scaduto il tempo valido in cache (o perché non sono state
                // correttamente caricate, le vado a prendere dal DB.
                if (conventionInfo.HasConvention &&
                    (!conventionInfo.Conventions.Any() || !conventionInfo.HasValidCacheSlot))
                {
                    var conventions = await userRepository.ConventionsAsync(sessionInfo.UserId,
                        sessionInfo.InstitutionCode);
                    conventionInfo.UserId = sessionInfo.UserId;
                    conventionInfo.UserTypeId = profileInfo.ProfileTypeId;
                    conventionInfo.WorkOfficeCode = profileInfo.OfficeCode;
                    foreach (var convention in conventions.ToArray())
                    {
                        conventionInfo.Conventions.Add(convention);
                    }
                }

                if (conventionInfo.Conventions.Any())
                {
                    var matchingConvention = conventionInfo.Conventions.FirstOrDefault(x => x.ServiceId == serviceId);

                    // Convenzione assente
                    if (matchingConvention == null)
                    {
                        var url = urlOptions.ErrorPage
                            .AddToQueryString(Keys.ErrorMessage, ErrorCodes.EightyFour);

                        throw new PassiUnauthorizedException(url, Reason.ConventionNotFound);
                    }
                    // Convenzione scaduta
                    else if (!matchingConvention.IsAvailable)
                    {
                        var url = urlOptions.ErrorPage
                            .AddToQueryString(Keys.ErrorMessage, ErrorCodes.EightyThree);

                        throw new PassiUnauthorizedException(url, Reason.ConventionExpired);
                    }
                }
            }
        }

        private async Task UpdateConventionsAsync(int serviceId, ConventionInfo conventionInfo, ProfileInfo profileInfo)
        {
            if (serviceId != SpecialServices.EveryProfileIsOK && conventionInfo.HasConvention && !conventionInfo.HasValidCacheSlot && profileInfo.Services.Any(a => a.HasConvention && a.Id == serviceId))
            {
                // Imposto lo slot di validità della cache per le convenzioni con lo stesso valore
                // di quello per il profilo. In questo modo mantengo la sincronizzazione.

                conventionInfo.LastUpdate = profileInfo.LastUpdate;
                conventionInfo.Timeout = profileInfo.Timeout;
                await conventionInfoRepository.UpdateAsync(conventionInfo);
            }
        }

        #endregion

        /// <summary>
        /// Metodo centralizzato che contiene tutta la logica per autenticare l'utente e verificare se ha i permessi corretti per accedere ad uno specifico ServiceId.
        /// </summary>
        /// <param name="serviceId">Id del servizio.</param>
        /// <returns></returns>
        /// <exception cref="PassiUnauthorizedException"></exception>
        public async Task<SessionInfo> IsAuthorizedAsync(int serviceId, string returnUrl = "")
        {
            SessionInfo? sessionInfo;
            Uri returnUri = ReturnUrl(accessor);
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUri = new Uri(returnUrl);
            }

            try
            {
                sessionInfo = await sessionInfoRepository.RetrieveAsync();
                var profileInfo = await profileInfoRepository.RetrieveAsync();
                var conventionInfo = await conventionInfoRepository.RetrieveAsync();

                CheckSameUser(sessionInfo, profileInfo);
                CheckSessionTimeout(sessionInfo);
                CheckLoginTimeout(sessionInfo);
                await CheckActiveSessionAsync(sessionInfo);
                await CheckProfileAsync(serviceId, sessionInfo, profileInfo, returnUri);
                await CheckLevelAsync(serviceId, sessionInfo, profileInfo, returnUri);
                CheckTimeSlots(serviceId, sessionInfo, profileInfo);
                await CheckDelegationAsync(sessionInfo, profileInfo);
                await CheckConventions(serviceId, sessionInfo, profileInfo, conventionInfo);

                // I controlli sono finiti, l'utente è autenticato e autorizzato
                sessionInfo = await sessionInfoRepository.UpdateAsync(sessionInfo);
                await profileInfoRepository.UpdateAsync(profileInfo);
                await UpdateConventionsAsync(serviceId, conventionInfo, profileInfo);

                try
                {
                    //Devo verificare che non ci sia un Operatore CC che stia lavorando per un utente,
                    //in tal caso devo prendere le generalità dell'utente e non quelle dell'Operatore
                    var contactCenterInfo = await contactCenterRepository.RetrieveAsync();
                    sessionInfo.CheckContactCenterInfo(contactCenterInfo);
                }
                catch (NotFoundException)
                {
                    // Do nothing
                }

                return sessionInfo!;
            }

            catch (NotFoundException ex)
            {
                logger.LogInformation("CheckAuthentication - CookieNotFoundException Errore: - {message}", ex.Message);
                throw new PassiUnauthorizedException(LoginSpid(returnUri), Reason.InvalidSessionCookie);
            }
            catch (ParameterException ex)
            {
                logger.LogInformation("CheckAuthentication - ParameterException Errore: {message}", ex.Message);
                throw new PassiUnauthorizedException(LoginSpid(returnUri), Reason.InvalidParameter);
            }
            catch (PassiException)
            {
                throw;
            }
            catch (InvalidDataException ex)
            {
                logger.LogInformation("CheckAuthentication - InvalidDataException Errore: {message}", ex.Message);
                throw new PassiUnauthorizedException(LoginSpid(returnUri), Reason.InvalidSessionCookie);
            }
            catch (Exception ex)
            {
                logger.LogInformation("CheckAuthentication - Errore: {message}", ex.Message);
                string errorMessage = dataCypher.Crypt(ex.Message.Replace("\n", "").Replace("\r", ""), Crypto.OUT);

                var url = LoginSpid(returnUri);
                url = url
                    .AddToQueryString(Keys.ErrorMessage, ErrorCodes.Three)
                    .AddToQueryString(Keys.Spec, errorMessage);

                throw new PassiUnauthorizedException(url, Reason.Unknown);
            }
        }

        public Uri ReturnUrl(IHttpContextAccessor contextAccessor)
        {
            if (!string.IsNullOrWhiteSpace(configurationOptions.RedirectUrl))
            {
                return new Uri(configurationOptions.RedirectUrl);
            }

            if(contextAccessor.HttpContext == null)
            {
                throw new NotFoundException("Contesto HTTP non valido");
            }

            var request = contextAccessor.HttpContext?.Request;

            if (request != null)
            {
                /// Per le applicazioni standard, la returnUrl del servizio è esattamente la url che viene chiamata dall'utente
                /// Per le applicazioni API, la returnUrl è la url iniziale del servizio. Viene mantenuta nel SessionToken
                string path = $"{request!.PathBase.ToString().ToLower()}/{request!.Path.ToString().ToLower()}".Trim('/');

                var uribuilder = new UriBuilder()
                {
                    Scheme = Schema.Https,
                    Host = request!.Host.Host,
                    Path = path,
                    Query = request!.QueryString.ToString(),
                };

                return uribuilder.Uri;
            }

            throw new ParameterException("Contesto HTTP non trovato");
        }

        #region private
        private PassiException SwitchLevel(string sessionId, char requiredAuthenticationType, Reason reason, Uri returnUrl)
        {
            logger.LogInformation("CheckAuthentication - Necessario Change Level - {requiredAuthenticationType} - {sessionId}", requiredAuthenticationType, sessionId);
            return requiredAuthenticationType switch
            {
                _ when requiredAuthenticationType == CommonAuthenticationTypes.CNS.ShortDescribe() 
                    => new PassiUnauthorizedException(LinkCNS(returnUrl), reason),
                _ when requiredAuthenticationType == CommonAuthenticationTypes.OTP.ShortDescribe() 
                    => new PassiUnauthorizedException(LinkOTP(returnUrl), reason),
                _ when requiredAuthenticationType == CommonAuthenticationTypes.PIN.ShortDescribe() 
                    => new PassiUnauthorizedException(urlOptions.PassiWebI, reason),
                _ => new PassiUnauthorizedException(urlOptions.PassiWebI, reason),
            };
        }
        private PassiException SwitchLevelThenProfile(string sessionId, int serviceId, char requiredAuthenticationType, Reason reason, Uri returnUrl)
        {
            logger.LogInformation("CheckAuthentication - Necessario Switch Profile {requiredAuthenticationType} - {sessionId}", requiredAuthenticationType, sessionId);

            var innerUrl = urlOptions.SwitchProfile
                .AddToQueryString(Keys.ServiceId, serviceId)
                .AddToQueryString(Keys.Ente, Keys.Ente)
                .AddToQueryString(Keys.Uri, returnUrl);
            innerUrl = AddRequiredProfile(innerUrl);

            Uri redirectUrl = innerUrl;

            if (requiredAuthenticationType == CommonAuthenticationTypes.CNS.ShortDescribe())
            {
                innerUrl = innerUrl
                        .AddToQueryString(Keys.ErrorMessage, ErrorCodes.Eight);
                redirectUrl = urlOptions.PassiWebCns
                    .AddToQueryString(Keys.Uri, innerUrl)
                    .AddToQueryString(Keys.ServiceId, serviceId);
            }
            else if (requiredAuthenticationType == CommonAuthenticationTypes.OTP.ShortDescribe())
            {
                redirectUrl = urlOptions.PassiWebOtp
                        .AddToQueryString(Keys.Uri, innerUrl)
                        .AddToQueryString(Keys.ServiceId, serviceId);
            }

            return new PassiUnauthorizedException(redirectUrl, reason);
        }
        private PassiException SwitchProfile(string sessionId, int serviceId, string authenticationType, Reason reason, Uri returnUrl)
        {
            logger.LogInformation("CheckAuthentication - Necessario Switch Profile {authenticationType} - {sessionId}", authenticationType, sessionId);
            var redirectUrl = urlOptions.SwitchProfile
                .AddToQueryString(Keys.ServiceId, serviceId)
                .AddToQueryString(Keys.Uri, returnUrl);

            redirectUrl = AddRequiredProfile(redirectUrl);

            return new PassiUnauthorizedException(redirectUrl, reason);
        }
        private Uri LoginSpid(Uri returnUrl)
        {
            var url = urlOptions.PassiWeb
                .AddToQueryString(Keys.Uri, returnUrl)
                .AddToQueryString(Keys.Ente, Keys.Ente);
            return AddRequiredProfile(url);
        }
        private Uri LinkCNS(Uri returnUrl)
        {
            var url = urlOptions.PassiWebCns
                .AddToQueryString(Keys.Uri, returnUrl)
                .AddToQueryString(Keys.ErrorMessage, ErrorCodes.Eight)
                .AddToQueryString(Keys.Ente, Keys.Ente);
            return AddRequiredProfile(url);
        }
        private Uri LinkOTP(Uri returnUrl)
        {
            var url = urlOptions.PassiWebOtp
                .AddToQueryString(Keys.Uri, returnUrl)
                .AddToQueryString(Keys.Ente, Keys.Ente);
            return AddRequiredProfile(url);
        }
        private int? RequiredUserTypeId()
        {
            HttpRequest? _request = accessor.HttpContext?.Request;
            var srcPortal = _request?.GetString(Keys.RequiredUserTypeId);
            if (!string.IsNullOrWhiteSpace(srcPortal))
            {
                if (int.TryParse(srcPortal, out int srcPortalId))
                {
                    return srcPortalId;
                }
                throw new PassiUnauthorizedException(urlOptions.Logout, ErrorCodes.Three, Reason.InvalidParameter, true);
            }
            return null;
        }
        private Uri AddRequiredProfile(Uri url)
        {
            int? requiredProfile = RequiredUserTypeId();
            if (requiredProfile != null)
            {
                url.AddToQueryString(Keys.RequiredUserTypeId, requiredProfile);
            }
            return url;
        }
        #endregion

        #region User Profiles
        private ICollection<Profile> _profiles = new HashSet<Profile>();
        private async Task<ICollection<Profile>> UserProfilesAsync(
            int serviceId,
            SessionInfo sessionInfo)
        {
            if (!_profiles.Any())
            {
                _profiles = await userRepository.ProfilesAsync(sessionInfo.UserId,
                    serviceId,
                    null,
                    sessionInfo.AuthenticationType,
                    true);
            }
            return _profiles;
        }

        #endregion

    }
}
