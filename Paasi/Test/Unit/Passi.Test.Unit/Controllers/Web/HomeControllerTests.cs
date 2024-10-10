using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Services;
using Passi.Test.CookieAuthenticationWebApp.Controllers;
using Passi.Test.CookieAuthenticationWebApp.Models;
using System.Security.Claims;
using System.Text;

namespace Passi.Test.Unit.Controllers.Web
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_whenCalled_thenReturnsView()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);
            var configuration = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes("{ \"ServiceId\": 0 }"))).Build();

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            //    var model = Assert.IsAssignableFrom<IEnumerable<StormSessionViewModel>>(
            //viewResult.ViewData.Model);
            //    Assert.Equal(2, model.Count());
            Assert.NotNull(viewResult);

        }

        [Fact]
        public void Service_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = controller.Service(0, 0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.True(string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public async Task Me_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.MeAsync()).ReturnsAsync(new User()
            {
                Name = "Test",
            });

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.Me(0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public void ClaimsPrincipal_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier, "Test"),
                new("AuthenticationType", "Test"),
                new("InstitutionDescription", "Test"),
                new("InstitutionCode", "Test"),
                new("InstitutionFiscalCode", "Test"),
                new("OfficeCode", "Test"),
                new("UserTypeId", "Test"),
                new("AuthenticationType", "Test"),

            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = controller.ClaimsPrincipal(0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.True(!string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public async Task Profiles_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.ProfileAsync()).ReturnsAsync(new Profile()
            {
                InstitutionCode = "Test",
            });

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.Profiles(0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.True(!string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public async Task SwitchProfile_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.MeAsync()).ReturnsAsync(new User()
            {
                Name = "Test",
                MultipleProfile = true
            });
            passiService.Setup(x => x.SwitchProfileUrl()).Returns(new Uri("https://www.google.com"));

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = Schema.Https;
            httpContext.Request.Host = new HostString("google.com");
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.SwitchProfile("test");

            // Assert
            var viewResult = Assert.IsType<RedirectResult>(result);
            Assert.NotNull(viewResult);

            Assert.Contains("google", viewResult.Url);
        }

        [Fact]
        public async Task Services_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.AuthorizedServicesAsync()).ReturnsAsync(new List<int>()
            {
                1, 2, 3, 4,
            });

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.Services(0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public async Task IsAuthorized_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.IsAuthorizedAsync(0)).ReturnsAsync(true);

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.IsAuthorized(0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public async Task Contacts_whenCalled_thenReturnsJson()
        {
            string cf = "MRARSS80D16H501P";
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.UserContactsAsync(cf)).ReturnsAsync(new UserContacts()
            {
                Email = "aaa",
            });

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.Contacts(0, null, cf);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public async Task HasPatronage_whenCalled_thenReturnsJson()
        {
            string cf = "MRARSS80D16H501P";
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.HasPatronageDelegationAsync(cf)).ReturnsAsync(true);

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.HasPatronage(cf, 0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));

        }

        [Fact]
        public void Links_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.SwitchProfileUrl()).Returns(new Uri("https://www.google.com"));

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = controller.Links(0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));

        }

        [Fact]
        public async Task WriteLog_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);
            clogService.Setup(x => x.LogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>>(), 0, null))
                .Returns(Task.CompletedTask);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.WriteLogAsync(0, "a=a;b=b", 0, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));
        }

        [Fact]
        public void Logout_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            passiService.Setup(x => x.LogoutUrl()).Returns(new Uri("https://www.google.com"));

            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = controller.Logout();

            // Assert
            var jsonResult = Assert.IsType<RedirectResult>(result);
            Assert.NotNull(jsonResult);
            Assert.True(jsonResult.Url.Length > 0);
        }

        [Fact]
        public async Task Token_whenCalled_thenReturnsJson()
        {
            // Arrange
            var passiService = new Mock<IPassiService>(MockBehavior.Strict);
            var passiSecureService = new Mock<IPassiSecureService>(MockBehavior.Strict);
            passiSecureService.Setup(x => x.SessionTokenAsync()).ReturnsAsync("Test");

            var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
            var clogService = new Mock<ICLogService>(MockBehavior.Strict);

            var controller = new HomeController(passiService.Object,
                passiSecureService.Object,
                clogService.Object,
                configuration.Object);

            // Act
            var result = await controller.Token(0);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            var model = Assert.IsAssignableFrom<IndexModel>(viewResult.Model);
            Assert.Equal(0, model?.ServiceId);
            Assert.False(string.IsNullOrWhiteSpace(model?.Response));

        }


    }
}
