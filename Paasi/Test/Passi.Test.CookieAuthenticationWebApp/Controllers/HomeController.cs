using Bogus.DataSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Exceptions;
using Passi.Test.CookieAuthenticationWebApp.Extensions;
using Passi.Test.CookieAuthenticationWebApp.Models;
using System.Security.Claims;
using System.Web;
using System;

namespace Passi.Test.CookieAuthenticationWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPassiService passiService;
        private readonly IPassiSecureService passiSecure;
        private readonly ICLogService clogService;
        private readonly IConfiguration configuration;

        public HomeController(IPassiService passiService,
            IPassiSecureService passiSecure,
            ICLogService clogService,
            IConfiguration configuration)
        {
            this.passiService = passiService;
            this.passiSecure = passiSecure;
            this.clogService = clogService;
            this.configuration = configuration;
        }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            return View(new IndexModel(configuration.GetValue<int>("ServiceId"), null));
        }

        [Authorize]
        [HttpGet("service")]
        public IActionResult Service(int serviceId, int? srcPortal = null)
        {
            return View("Index", new IndexModel(serviceId, srcPortal));
        }

        [Authorize]
        [HttpPost("/me")]
        public async Task<IActionResult> Me(int serviceId, int? srcPortal = null)
        {
            var me = await passiService.MeAsync();
            return View("Index", new IndexModel(serviceId, srcPortal) { Response = me.Serialize() });
        }

        [Authorize]
        [HttpPost("/me/claimsPrincipal")]
        public IActionResult ClaimsPrincipal(int serviceId, int? srcPortal = null)
        {
            return View("Index", new IndexModel(serviceId, srcPortal)
            {
                Response = User.Claims.Select(x => new
                {
                    Key = x.Type,
                    Value = x.Value
                }).ToList().Serialize()
            });
        }

        [Authorize]
        [HttpPost("/me/profiles")]
        public async Task<IActionResult> Profiles(int serviceId, int? srcPortal = null)
        {
            var data = await passiService.ProfileAsync();
            return View("Index", new IndexModel(serviceId, srcPortal) { Response = data.Serialize() });
        }

        [Authorize]
        [HttpGet("/me/switchProfile")]
        public IActionResult SwitchProfile(string serviceId)
        {
            serviceId = !string.IsNullOrWhiteSpace(serviceId) ? serviceId : "0";

            var uri = passiService.SwitchProfileUrl();
            uri = AddToQueryString(uri, "idservizio", serviceId);
            uri = AddToQueryString(uri, "uri", ServiceUrl(serviceId));
            string result = uri.ToString();
            return Redirect(result);
        }

        [Authorize]
        [HttpPost("/me/services")]
        public async Task<IActionResult> Services(int serviceId, int? srcPortal = null)
        {
            var data = await passiService.AuthorizedServicesAsync();
            return View("Index", new IndexModel(serviceId, srcPortal) { Response = data.Serialize() });
        }

        [Authorize]
        [HttpPost("/me/isAuthorized")]
        public async Task<IActionResult> IsAuthorized(int serviceId, int? srcPortal = null)
        {
            var data = await passiService.IsAuthorizedAsync(serviceId);
            return View("Index", new IndexModel(serviceId, srcPortal) { Response = data.Serialize() });
        }

        [Authorize]
        [HttpPost("/me/card")]
        public async Task<IActionResult> Contacts(int serviceId, int? srcPortal = null, string fiscalCode = "")
        {
            var data = await passiService.UserContactsAsync(fiscalCode);
            return View("Index", new IndexModel(serviceId, srcPortal) { Response = data.Serialize() });
        }

        [Authorize]
        [HttpPost("/me/delegation")]
        public async Task<IActionResult> HasPatronage(string fiscalCode, int serviceId, int? srcPortal = null)
        {
            var data = await passiService.HasPatronageDelegationAsync(fiscalCode);
            return View("Index", new IndexModel(serviceId, srcPortal) { Response = data.Serialize() });
        }

        [Authorize]
        [HttpPost("/me/writelog")]
        public async Task<IActionResult> WriteLogAsync(int eventId, string paramz, int serviceId, int? srcPortal = null)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var piece in paramz.Split(';'))
            {
                var kvp = piece.Split('=');
                string key = kvp.FirstOrDefault() ?? string.Empty;
                string value = kvp.LastOrDefault() ?? string.Empty;
                dictionary[key] = value;
            }

            bool data = true;

            try
            {
                await clogService.LogAsync(eventId, 100, dictionary);
            }
            catch (PassiException)
            {
                data = false;
            }

            return View("Index", new IndexModel(serviceId, srcPortal)
            {
                Response = new
                {
                    Salvato = data,
                }.Serialize()
            });
        }

        [Authorize]
        [HttpPost("/me/links")]
        public IActionResult Links(int serviceId, int? srcPortal = null)
        {
            return View("Index", new IndexModel(serviceId, srcPortal)
            {
                Response = new
                {
                    SwitchProfileUrl = passiService.SwitchProfileUrl(),
                }.Serialize()
            });
        }

        [Authorize]
        [HttpGet("/me/logout")]
        public IActionResult Logout()
        {
            return Redirect(passiService.LogoutUrl().ToString());
        }

        [Authorize]
        [HttpPost("/me/token")]
        public async Task<IActionResult> Token(int serviceId, int? srcPortal = null)
        {
            var sessionToken = await passiSecure.SessionTokenAsync();
            return View("Index", new IndexModel(serviceId, srcPortal)
            {
                Response = new
                {
                    SessionToken = sessionToken,
                }.Serialize()
            });
        }

        [Authorize(Roles = "3")] // Cittadino
        [HttpPost("/me/cittadino")]
        public async Task<IActionResult> RuoloUtente(int serviceId, int? srcPortal = null)
        {
            return View("Index", new IndexModel(serviceId, srcPortal)
            {
                Response = new
                {
                    text = "Sei un cittadino"
                }.Serialize()
            });
        }

        private Uri ServiceUrl(string serviceId)
        {
            string loginUrl = $"https://{Request?.Host.Host.TrimEnd('/')}/Service?serviceId={serviceId}";

            return new Uri(loginUrl);
        }

        private static Uri AddToQueryString<T>(Uri uri, string key, T value)
        {
            var currentQs = uri.Query;
            var pieces = currentQs.Split("&");

            var dictionary = new Dictionary<string, string>();
            foreach (var p in pieces)
            {
                var couple = p.Split('=');
                if (couple.Length == 2)
                {
                    var myKey = couple.FirstOrDefault();
                    var myValue = couple.LastOrDefault();
                    if (!string.IsNullOrWhiteSpace(myValue) && !string.IsNullOrWhiteSpace(myKey) && !dictionary.ContainsKey(myKey))
                    {
                        dictionary.Add(myKey, myValue);
                    }
                }
            }

            var _value = value?.ToString();
            if (!string.IsNullOrWhiteSpace(_value) && !dictionary.ContainsKey(key))
            {
                if (value is Uri)
                {
                    dictionary.Add(key, HttpUtility.UrlEncode(_value));
                }
                else if (value is ErrorCodes errorCode)
                {
                    dictionary.Add(key, ((int)errorCode).ToString());
                }
                else
                {
                    dictionary.Add(key, _value);
                }
            }

            var newQs = string.Join("&", dictionary.Select(x => $"{x.Key}={x.Value}")).Trim('?').Trim('/');
            return new Uri($"{Schema.Https}://{uri.Host}{uri.AbsolutePath}?{newQs}".Trim('?'));
        }


    }


}