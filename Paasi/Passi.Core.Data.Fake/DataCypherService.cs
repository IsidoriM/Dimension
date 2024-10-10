using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using System.Collections.Specialized;

namespace Passi.Core.Store.Fake
{
    internal class DataCypherService : IDataCypherService
    {
        public string Crypt(string data, Crypto type = Crypto.KCA)
        {
            return data;
        }
        public string Decrypt(string data, Crypto type = Crypto.KCA)
        {
            return data;
        }

        public string Secure(NameValueCollection collection)
        {
            var values = collection.AllKeys.Select(key => $"{key}={collection[key]}");
            return string.Join("|", values);
        }

        public NameValueCollection Unsecure(string cryptedText)
        {
            NameValueCollection result = new();
            foreach (var entity in cryptedText.Split("|", StringSplitOptions.RemoveEmptyEntries))
            {
                var nameValue = entity.Split("=");
                result.Add(nameValue.First(), nameValue.Last());
            }
            return result;
        }
    }
}
