using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passi.Test.CookieAuthenticationWebApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Passi.Test.Unit.Extensions
{
    public class ProgramTests
    {

        private string AppSettings()
        {
            string json = string.Empty;
            if (File.Exists("Contents/fakeAppSettings.json"))
            {
                json = File.ReadAllText("Contents/fakeAppSettings.json");
            }
            else
            {
                json = File.ReadAllText("../s/Test/Unit/Passi.Test.Unit/Contents/fakeAppSettings.json");
            }
            return json;
        }

        [Fact]
        public void WebApp_whenHostStarting_thenContinue()
        {
            string json = AppSettings();

            var builder = WebApplication.CreateBuilder();

            builder.Configuration.AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)));

            // Add services to the container.
            builder.Services.AddPassiAuthentication(builder.Configuration);

            var app = builder.Build();

            app.UsePassiAuthentication();

            Assert.True(true);
        }

        //[Fact]
        //public void WebApi_whenHostStarting_thenContinue()
        //{
        //    string json = AppSettings();

        //    var builder = WebApplication.CreateBuilder();

        //    builder.Configuration.AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)));

        //    // Add services to the container.
        //    builder.Services.AddPassiAuthentication(builder.Configuration);

        //    var app = builder.Build();

        //    app.UsePassiAuthentication();

        //    Assert.True(true);
        //}

    }
}
