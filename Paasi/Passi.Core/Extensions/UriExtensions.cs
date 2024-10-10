using Passi.Core.Domain.Const;
using System.Runtime.CompilerServices;
using System.Web;

[assembly:InternalsVisibleTo("Passi.Test.CookieAuthenticationWebApp.Controllers")]
namespace System
{
    static class UriExtensions
    {
        public static Uri Default => new("https://www.inps.it");

        public static Uri AddToQueryString<T>(this Uri uri, string key, T value)
        {
            var currentQs = uri.Query;
            var pieces = currentQs.Split("&");

            var dic = new Dictionary<string, string>();
            foreach (var p in pieces)
            {
                var couple = p.Split('=');
                if (couple.Length == 2)
                {
                    var myKey = couple.FirstOrDefault();
                    var myValue = couple.LastOrDefault();
                    if (!string.IsNullOrWhiteSpace(myValue) && !string.IsNullOrWhiteSpace(myKey) && !dic.ContainsKey(myKey))
                    {
                        dic.Add(myKey, myValue);
                    }
                }
            }

            var _value = value?.ToString();
            if (!string.IsNullOrWhiteSpace(_value) && !dic.ContainsKey(key))
            {
                if (value is Uri)
                {
                    dic.Add(key, HttpUtility.UrlEncode(_value));
                }
                else if (value is ErrorCodes errorCode)
                {
                    dic.Add(key, ((int)errorCode).ToString());
                }
                else
                {
                    dic.Add(key, _value);
                }
            }

            var newQs = string.Join("&", dic.Select(x => $"{x.Key}={x.Value}")).Trim('?').Trim('/');
            return new Uri($"{Schema.Https}://{uri.Host}{uri.AbsolutePath}?{newQs}".Trim('?'));
        }
    }
}
