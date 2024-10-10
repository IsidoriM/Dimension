using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Passi.Test.CookieAuthenticationWebApp.Extensions
{
    public static class ConversionExtensions
    {

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize<T>(this T item)
        {
            JsonSerializerOptions myOptions = options;
            return JsonSerializer.Serialize(item, myOptions);
        }
    }
}
