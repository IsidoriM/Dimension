using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Passi.Authentication.Cookie.Providers;
using Passi.Core.Application.Options;
using Passi.Core.Domain.Const;
using Passi.Core.Domain.Entities;
using Passi.Core.Domain.Entities.Info;
using Passi.Core.Extensions;
using Passi.Core.Services.Extensions;
using Passi.Test.Unit.Fixtures;
using System.Runtime.InteropServices;
using AutoFixture;
using Passi.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;
using Passi.Test.CookieAuthenticationWebApp.Extensions;

namespace Passi.Test.Unit.Core
{
    public class ExtensionsTests : IClassFixture<PassiFixture>
    {
        private readonly PassiFixture fixture;

        public ExtensionsTests(PassiFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void PecVerivicationStatus_Ok()
        {
            foreach (PecVerificationStatuses status in Enum.GetValues<PecVerificationStatuses>().Where(w => w != PecVerificationStatuses.None))
            {
                Assert.Equal(((int)status).ToString(), status.ToVerificationString());
                Assert.True(status == ((int)status).ToString().ToVerificationStatuses());
            }
        }

        [Fact]
        public void AuthenticationType_Ok()
        {
            var x = from d in typeof(CommonAuthenticationTypes).GetFields()
                    select d.GetRawConstantValue();
            foreach (var auth in x)
            {
                Assert.Equal(((string)auth).First(), ((string)auth).ShortDescribe());
            }
        }

        [Theory]
        [InlineData("test.test@test.it")]
        [InlineData("test@test.it")]
        public void StringObfuscateEmail_Ok(string email)
        {
            string result = email.ObfuscateEmail();
            Assert.Contains("*", result);
        }

        [Theory]
        [InlineData("333123456789")]
        [InlineData("123456789")]
        public void StringObfuscatePhone_Ok(string email)
        {
            string result = email.ObfuscatePhoneNumber();
            Assert.Contains("*", result);
        }

        [Theory]
        [InlineAutoData]
        [InlineAutoData("")]
        [InlineAutoData(null)]
        public void RegistryConfigurationProvider_Ok(string keyPath)
        {
            Exception? exception = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                RegistryKeyConfigurationProvider registry = new(keyPath);
                exception = Record.Exception(() => registry.Load());
            }
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData]
        public void ConfigureUrlOptions_Ok(Uri link)
        {
            Dictionary<string, string> configurationValues = new()
            {
                { "LinkChangeContacts", link.ToString() },
                { "LinkChangePin", link.ToString() },
                { "LinkErrorPage", link.ToString() },
                { "LinkLogout", link.ToString() },
                { "LinkSwitchProfile", link.ToString() },
                { "LinkPassiWeb", link.ToString() },
                { "LinkPassiWebC", link.ToString() },
                { "LinkPassiWebI", link.ToString() },
                { "LinkPassiWebO", link.ToString() }
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

            Exception? exception = Record.Exception(() => ConfigureExtensions.ConfigureUrlOptions(new UrlOptions(), configuration));
            Assert.Null(exception);
        }

        [Fact]
        public void ConfigureUrlOptionsEmptyUrls_Ok()
        {
            Dictionary<string, string> configurationValues = new()
            {
                { "LinkChangeContacts", string.Empty },
                { "LinkChangePin", string.Empty },
                { "LinkErrorPage", string.Empty },
                { "LinkLogout", string.Empty },
                { "LinkSwitchProfile", string.Empty },
                { "LinkPassiWeb", string.Empty },
                { "LinkPassiWebC", string.Empty },
                { "LinkPassiWebI", string.Empty },
                { "LinkPassiWebO", string.Empty }
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

            Exception? exception = Record.Exception(() => ConfigureExtensions.ConfigureUrlOptions(new UrlOptions(), configuration));
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData("0")]
        [InlineAutoData("", null, null)]
        [InlineAutoData("", "", "")]
        public void ConfigureConfigurationOptions_Ok(string serviceId, string? gestioneSessione, string? log, bool allowServiceEditing, Uri redirectUrl)
        {
            Dictionary<string, string?> configurationValues = new()
            {
                { "ServiceId", serviceId },
                { "RedirectUrl", redirectUrl.ToString() },
                { "GestioneSessione", gestioneSessione },
                { "Log", log },
                { "AllowServiceIdEditing", allowServiceEditing.ToString() }
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

            Exception? exception = Record.Exception(() => ConfigureExtensions.ConfigureConfigurationOptions(new ConfigurationOptions(), configuration));
            Assert.Null(exception);
        }

        [Fact]
        public void AddDefaultScheme_Ok()
        {
            Exception? exception = Record.Exception(() => ServiceCollectionExtensionsOptions.AddDefaultScheme(new AuthenticationOptions()));
            Assert.Null(exception);
        }

        [Fact]
        public void AddAuthorizationCoreOptions_Ok()
        {
            Exception? exception = Record.Exception(() => ServiceCollectionExtensionsOptions.AddAuthorizationCoreOptions(new AuthorizationOptions()));
            Assert.Null(exception);
        }

        [Fact]
        public void ConfigureCookiePolicyOptions_Ok()
        {
            Exception? exception = Record.Exception(() => ServiceCollectionExtensionsOptions.ConfigureCookiePolicyOptions(new CookiePolicyOptions()));
            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData(false, -1)]
        [InlineAutoData(false, 0)]
        [InlineAutoData(false, 1)]
        [InlineAutoData(false, 2)]
        [InlineAutoData(false, 9)]
        [InlineAutoData(false, 10)]
        [InlineAutoData(false, 11)]
        [InlineAutoData(false, 12)]
        [InlineAutoData(true, 1000)]
        public void UserContactsWith_Ok(bool isObfuscated, int pecVerificationStatus, UserContacts userContacts)
        {
            UserInfo userInfo = fixture.Fixture.Create<UserInfo>();
            userInfo.FiscalCode = fixture.Fixture.Create<string>();
            userInfo.PEC = fixture.Fixture.Create<string>();
            SessionInfo sessionInfo = fixture.Fixture.Create<SessionInfo>();
            sessionInfo.IsInfoPrivacyAccepted = true;
            sessionInfo.PECVerificationStatus = (PecVerificationStatuses)pecVerificationStatus;

            UserContacts result = userContacts.With(userInfo, sessionInfo, isObfuscated);

            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData(false)]
        [InlineAutoData(true, "")]
        public void UserContactsWith_Exception(bool isPrivacyAccepted, string fiscalCode, UserContacts userContacts, bool isObfuscated)
        {
            UserInfo userInfo = fixture.Fixture.Create<UserInfo>();
            userInfo.FiscalCode = fiscalCode;
            SessionInfo sessionInfo = fixture.Fixture.Create<SessionInfo>();
            sessionInfo.IsInfoPrivacyAccepted = isPrivacyAccepted;

            Assert.Throws<ContactsException>(() => userContacts.With(userInfo, sessionInfo, isObfuscated));
        }

        [Theory]
        [InlineAutoData(false)]
        [InlineAutoData(false, "ciao")]
        [InlineAutoData(true, "/api/abc")]
        [InlineAutoData(true, "api/abc")]
        public void StringIsApi_Ok(bool expectedResult, string s)
        {
            bool result = s.IsApi();
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Random_Ok()
        {
            Outcomes result = StringExtensions.Random<Outcomes>();

            Assert.Contains(result, Enum.GetValues<Outcomes>());
        }


        private struct TestStruct
        {
            public const string Field1 = "TEST1";
            public const string Field2 = "TEST2";
            public const string Field3 = "TEST3";
        }

        [Fact]
        public void RandomString_Ok()
        {
            string result = StringExtensions.RandomString<TestStruct>();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void RandomStringWithExclusion_Ok()
        {
            string result = string.Empty;
            // loop per coprire il codice di controllo sulla stringa da escludere
            for (int i = 0; i < 100; ++i)
            {
                result = StringExtensions.RandomString<TestStruct>(TestStruct.Field1);
            }

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.NotEqual(TestStruct.Field1, result);
        }

        [Theory]
        [InlineAutoData("ciao", 1, 1, "ciao")]
        [InlineAutoData("ciao", 2, 2, "ciao")]
        [InlineAutoData("", 0, 0, "")]
        [InlineAutoData("", 0, 5, "")]
        [InlineAutoData("null", 2, 2, "")]
        public void GetString_Ok(string forcedValue, int forcedPosition, int position, string expectedResult, string[] input)
        {
            input[forcedPosition] = forcedValue;
            string result = StringExtensions.GetString(input, position);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineAutoData("ciao", "CIAO")]
        [InlineAutoData("$c)ia$o$", "")]
        [InlineAutoData("ci++ao", "CI  AO")]
        [InlineAutoData("", "")]
        [InlineAutoData("ciao$)$$", "")]
        public void GetStringNoSpecialChars_Ok(string value, string expectedValue)
        {
            string result = StringExtensions.GetStringNoSpecialChars(value);
            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineAutoData("100", 1, 1, 100)]
        [InlineAutoData("34", 2, 2, 34)]
        [InlineAutoData("", 0, 0, 5, 5)]
        [InlineAutoData("", 0, 5, 7, 7)]
        [InlineAutoData("null", 2, 2, 6, 6)]
        public void GetInt_Ok(string forcedValue, int forcedPosition, int position, int expectedResult, int defaultValue, int[] input)
        {
            string[] inputStrings = input.Select(i => i.ToString()).ToArray();
            inputStrings[forcedPosition] = forcedValue;
            int result = StringExtensions.GetInt(inputStrings, position, defaultValue);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineAutoData("true", 1, 1, true)]
        [InlineAutoData("mtrue", 0, 0, true)]
        [InlineAutoData("1", 2, 2, true)]
        [InlineAutoData("false", 2, 2, false)]
        [InlineAutoData("mfalse", 1, 1, false)]
        [InlineAutoData("0", 0, 0, false)]
        [InlineAutoData("", 0, 0, false, false)]
        [InlineAutoData("", 0, 5, true, true)]
        [InlineAutoData("null", 2, 2, true, true)]
        public void GetBool_Ok(string forcedValue, int forcedPosition, int position, bool expectedResult, bool defaultValue, bool[] input)
        {
            string[] inputStrings = input.Select(b => b.ToString()).ToArray();
            inputStrings[forcedPosition] = forcedValue;
            bool result = StringExtensions.GetBool(inputStrings, position, defaultValue);

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineAutoData("1000", 62135596801000)]
        [InlineAutoData("0001-01-01 00:01:00", 60000)]
        [InlineAutoData("abc", 0)]
        public void ToDateTime_Ok(string toParse, double msForExpected)
        {
            DateTime result = DateTimeExtensions.ToDatetime(toParse);
            DateTime expected = DateTime.MinValue.AddMilliseconds(msForExpected);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineAutoData("1000", 1000000)]
        [InlineAutoData("abc", 0)]
        public void ToTimeSpan_Ok(string toParse, double msForExpected)
        {
            TimeSpan result = DateTimeExtensions.ToTimespan(toParse);
            TimeSpan expected = TimeSpan.FromMilliseconds(msForExpected);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineAutoData]
        public void GetRequestString_FromQuery_Ok(string key, string value)
        {
            DefaultHttpContext httpContext = new();
            HttpRequest request = httpContext.Request;
            request.QueryString = new QueryString($"?{key}={value}");

            string result = HttpContextExtensions.GetString(request, key);
            
            Assert.Equal(value, result);
        }

        [Theory]
        [InlineAutoData]
        public void GetRequestString_FromHeader_Ok(string key, string value)
        {
            DefaultHttpContext httpContext = new();
            HttpRequest request = httpContext.Request;
            request.Headers.Add(key, value);

            string result = HttpContextExtensions.GetString(request, key);

            Assert.Equal(value, result);
        }

        [Theory]
        [InlineAutoData]
        public void GetRequestString_FromForm_Ok(string key, string value)
        {
            DefaultHttpContext httpContext = new();
            HttpRequest request = httpContext.Request;
            request.Form = new FormCollection(new Dictionary<string, StringValues> { { key, value } });

            string result = HttpContextExtensions.GetString(request, key);

            Assert.Equal(value, result);
        }

        [Theory]
        [InlineAutoData]
        public void GetRequestString_FromCookies_Ok(string key, string value)
        {
            DefaultHttpContext httpContext = new();
            HttpRequest request = httpContext.Request;
            request.Headers.Add("Cookie", $"{key}={value}");

            string result = HttpContextExtensions.GetString(request, key);

            Assert.Equal(value, result);
        }

        [Theory]
        [InlineAutoData]
        public void GetRequestString_FromRequestServices_Ok(string key, string value)
        {
            Mock<IConfiguration> mockConfiguration = new(MockBehavior.Strict);
            mockConfiguration.SetupGet(c => c[It.Is<string>(s => s == key)]).Returns(value);
            Mock<HttpContext> httpContext = new(MockBehavior.Strict);
            httpContext.Setup(c => c.RequestServices.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);
            Mock<HttpRequest> mockRequest = new(MockBehavior.Strict);
            mockRequest.Setup(r => r.HttpContext).Returns(httpContext.Object);
            mockRequest.Setup(r => r.Query).Returns(new QueryCollection());
            mockRequest.Setup(r => r.Headers).Returns(new HeaderDictionary());
            mockRequest.Setup(r => r.Form).Returns(new FormCollection(new Dictionary<string, StringValues>()));

            HttpRequestFeature requestFeature = new();
            FeatureCollection featureCollection = new();
            requestFeature.Headers = new HeaderDictionary { { HeaderNames.Cookie, new StringValues() } };
            featureCollection.Set<IHttpRequestFeature>(requestFeature);
            RequestCookiesFeature cookiesFeature = new(featureCollection);
            mockRequest.Setup(r => r.Cookies).Returns(cookiesFeature.Cookies);

            string result = HttpContextExtensions.GetString(mockRequest.Object, key);

            Assert.Equal(value, result);
        }

        // vs: ...
        [Theory]
        [InlineAutoData(true, 1)]
        [InlineAutoData(false, 0)]
        public void BoolToInt(bool value, int expected)
        {
            int result = value.ToInt();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineAutoData(true)]
        [InlineAutoData(false)]
        public void BoolToStringLower(bool value)
        {
            string result = value.ToStringLower();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Theory]
        [InlineAutoData]
        [InlineAutoData(null)]
        public void AddToQueryString(string? value, Uri uri, string key)
        {
            Uri result = uri.AddToQueryString(key, value);
            Assert.NotNull(result);
        }
    }
}
