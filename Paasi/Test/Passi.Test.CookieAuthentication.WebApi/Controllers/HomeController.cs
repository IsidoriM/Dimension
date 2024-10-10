using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Passi.Core.Application.Services;

namespace Passi.Test.CookieAuthentication.WebApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPassiService passiService;

        public HomeController(IPassiService passiService)
        {
            this.passiService = passiService;
        }

        [Authorize]
        [HttpGet("/api/me")]
        public async Task<IActionResult> Me()
        {
            var me = await passiService.MeAsync();
            return Json(me);
        }
    }
}
