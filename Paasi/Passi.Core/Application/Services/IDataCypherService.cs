using Passi.Core.Domain.Const;
using System.Collections.Specialized;

namespace Passi.Core.Application.Services
{
    internal interface IDataCypherService
    {
        public string Crypt(string data, Crypto type = Crypto.KCA);
        public string Decrypt(string data, Crypto type = Crypto.KCA);
        public string Secure(NameValueCollection collection);
        public NameValueCollection Unsecure(string cryptedText);
    }
}
