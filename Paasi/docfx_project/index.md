# P.A.S.S.I. (Portale Accesso Sicuro Servizi INPS)

PASSI si occupa, per le applicazioni Web Internet, di centralizzare l’autenticazione e l’autorizzazione dell’utente che accede ai servizi dell’Istituto.
Per accedere alle applicazioni INPS esposte su Internet l’utente può utilizzare un PIN INPS (rilasciato in sede oppure on line) oppure una CNS (carta nazionale dei servizi) oppure una credenziale SPID (Sistema Pubblico di Identità Digitale) oppure la CIE (Carta d’Identità Elettronica) e deve essere autorizzato al servizio richiesto.

Ad ogni utente è assegnato uno o più profili, ad ogni profilo (es. Cittadino, Comune X, ASL Y, Patronato Z) sono associati i servizi a cui è autorizzato. Il tipo di profilo assegnato ad un utente è identificato dal tipo utente (es. Cittadino, Comune, ASL, Patronato).

PASSI offre un’interfaccia applicativa per la gestione del Single Sign-on accessibile sia dalle applicazioni Java che .NET attraverso librerie condivise.
PASSI gestisce due fasi distinte:

<ul>
<li><b>Autenticazione</b>: attraverso la funzione di login centralizzata viene richiesto all’utente di inserire le credenziali di accesso. In questa fase l’utente viene riconosciuto e le informazioni di sessione ad esso legate vengono messe a disposizione dell’applicazione.</li>
<li><b>Autorizzazione</b>: viene verificata la possibilità di accesso da parte dell’utente al servizio richiesto. Nel caso questa fase non venga superata, verrà effettuato un reindirizzamento automatico ad una pagina che nega l’accesso.</li>
</ul>

Oltre alle funzionalità di autenticazione e autorizzazione, che rappresentano il core della piattaforma PASSI, essa mette a disposizione dei metodi per il passaggio sicuro di parametri, più alcune utility descritte in seguito.

## Autenticazione e autorizzazione

Se l’utente non è autenticato, PASSI effettua autonomamente un reindirizzamento alla maschera di login che richiede l’immissione delle credenziali di accesso (PIN/SPID/CNS/CIE).

Una volta autenticato, se l’utente è abilitato al servizio richiesto per più profili, PASSI presenta una maschera per la scelta del profilo su cui ci si vuole attestare. Se per il servizio richiesto il profilo autorizzato è unico, questa selezione viene effettuata in maniera trasparente, senza interazione con l’utente, salvo l’inserimento del codice CAPTCHA laddove previsto.

Nel momento in cui PASSI restituisce il controllo all’applicazione, l’utenza loggata sarà disponibile nell’oggetto <i>ClaimsPrincipal</i>. Questo contiene le informazioni sull’utente, sulla sessione e sul profilo selezionato.

Di seguito sono elencate le proprietà (<i>Claims</i>) restituite:

<ul>
<li><b>NameIdentifier</b> <i>(http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier)</i>: valorizzato con la proprietà <i>UserId</i> dell’oggetto <i>SessionInfo</i>. Ciò corrisponde per quasi la totalità dei casi al codice fiscale dell’utente;</li>
<li><b>AuthenticationType</b>: rappresenta la tipologia di autenticazione con la quale l'utente ha acceduto (es. Spid, CNS, ecc.);</li>
<li><b>EnteDescription</b>: denominazione dell’ente per cui l’utente sta operando;</li>
<li><b>EnteCode</b>: codice dell’ente per cui l’utente sta operando;</li>
<li><b>EnteFiscalCode</b>: codice fiscale dell’ente per cui l’utente sta operando;</li>
<li><b>OfficeCode</b>: definisce la struttura territoriale dell’ente per cui l’utente sta operando (se previsto, es. patronati). Tale valore è estratto dalla proprietà <i>OfficeCode</i> dell’oggetto <i>SessionInfo</i>;</li>
<li><b>ProfileTypeId</b>: è il tipo di profilo con cui l’utente accede al servizio. (es. 1 per patronato, 14 per CAF, 3 per cittadino). Tale valore è estratto dalla proprietà <i>ProfileId</i> dell’oggetto <i>SessionInfo</i> (e non la proprietà <i>UserClass</i> che è in dismissione);</li>

</ul>

Per profili di tipo Patronato, CAF, etc. l’ufficio di "Direzione Generale", tipicamente uno per Ente, ha codice ufficio con prefisso <i>DIGEN</i>, es. DIGEN, DIGEN0, etc. (proprietà <i>OfficeCode</i> dell’oggetto <i>SessionInfo</i>).
