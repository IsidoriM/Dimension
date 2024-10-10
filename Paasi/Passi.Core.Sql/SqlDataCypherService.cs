using Passi.Core.Application.Services;
using Passi.Core.Domain.Const;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Passi.Core.Store.Sql
{
    internal class SqlDataCypherService : IDataCypherService
    {
        private readonly IDbConnectionFactory connectionFactory;
        private readonly IInstanceStore<byte[]> instanceStore;
        private const string _decryptor = "Fs__425fne.||e49Ex..$EWrz.4934u8";
        private const string _iv = "=?^bsyEr346A2.k4";
        private const string seed = "Hfki_5896hd-Htg4Kqs9523_TerYkht5";

        public SqlDataCypherService(IDbConnectionFactory connectionFactory, IInstanceStore<byte[]> instanceStore)
        {
            this.connectionFactory = connectionFactory;
            this.instanceStore = instanceStore;
        }

        public string Crypt(string data, Crypto type = Crypto.KCA)
        {
            return EncryptString(data, Key(type));
        }

        public string Decrypt(string data, Crypto type = Crypto.KCA)
        {
            return DecryptString(data, Key(type));
        }

        private static byte[] Iv()
        {
            return Encoding.UTF8.GetBytes(_iv);
        }

        private byte[] Key(Crypto type)
        {
            var cachedKey = Enum.GetName(type) ?? string.Empty;
            var cachedValue = instanceStore.Get(cachedKey);

            if (cachedValue.Length > 0)
            {
                return cachedValue;
            }

            string key = string.Empty;
            switch (type)
            {
                case Crypto.KCA:
                    key = RetrieveKeyFromSql("KCA");
                    break;
                case Crypto.KSTA:
                    key = RetrieveKeyFromSql("KSTA");
                    break;
                case Crypto.KTOK:
                    key = RetrieveKeyFromSql("KTOK");
                    break;
                case Crypto.OUT:
                    key = "83__.2jfoEId$$..fjksdg23.458DF1A";
                    break;
                default:
                    break;
            }

            var value = Encoding.UTF8.GetBytes(key);
            instanceStore.Add(cachedKey, value);
            return value;
        }

        private static string EncryptString(string text, byte[] keyBytes)
        {
            if (keyBytes.Length == 0)
                return text;

            byte[] plainBytes = Encoding.UTF8.GetBytes(text);

            using var aesAlg = Aes.Create();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.KeySize = 256;
            aesAlg.BlockSize = 128;

            using var encryptor = aesAlg.CreateEncryptor(keyBytes, Iv());
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            var base64Encrypted = Convert.ToBase64String(encryptedBytes);

            return base64Encrypted;
        }

        private static string DecryptString(string cipherText, byte[] keyBytes)
        {
            if (keyBytes.Length == 0)
                return cipherText;

            using var aesAlg = Aes.Create();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.KeySize = 256;
            aesAlg.BlockSize = 128;
            aesAlg.Key = keyBytes;
            using var decryptor = aesAlg.CreateDecryptor(keyBytes, Iv());

            byte[] encryptedBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        private string RetrieveKeyFromSql(string id)
        {
            using var connection = connectionFactory.CreateConnection(Config.CONN_SICUREZZA);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "spGetHexString";
            command.AddParameter("id", id, DbType.String);

            var outParameter = command.CreateParameter();
            outParameter.ParameterName = "@Valore";
            outParameter.Direction = ParameterDirection.Output;
            outParameter.DbType = DbType.String;
            outParameter.Size = 256;
            command.Parameters.Add(outParameter);

            using var dr = command.ExecuteReader(CommandBehavior.CloseConnection);
            dr.Close();

            if (outParameter.Value != null)
            {
                var outValue = outParameter.Value.ToString();
                if (!string.IsNullOrWhiteSpace(outValue))
                {
                    /// Decriptare la chiave
                    if (!string.IsNullOrWhiteSpace(_decryptor))
                        outValue = DecryptString(outValue, Encoding.UTF8.GetBytes(_decryptor));

                    // vs: testare il caso di valore vuoto è praticamente impossibile visto che il crypt di una stringa
                    // vuota dà un cyphertext vuoto, e non so se potrà mai esistere un caso di cyphertext non vuoto e plaintext vuoto
                    if (!string.IsNullOrWhiteSpace(outValue))
                        return outValue;
                }
            }
            return string.Empty;
        }

        public string Secure(NameValueCollection collection)
        {
            var secureString = string.Empty;
            foreach (var (s, v) in from string s in collection.AllKeys
                                   let _value = collection.Get(s)
                                   select (s, _value))
            {
                var value = v.Replace("|", "$!#");
                secureString = secureString + s + "=" + value + "|";
            }

            secureString += seed;
            secureString = Crypt(secureString, Crypto.KSTA);
            return secureString;
        }

        public NameValueCollection Unsecure(string cryptedText)
        {
            NameValueCollection collection = new();
            var decryptedText = Decrypt(cryptedText, Crypto.KSTA);
            decryptedText = decryptedText.Replace(seed, "").TrimEnd('|');
            foreach (string s in decryptedText.Split("|", StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = s.Split("=");
                collection.Add(keyValue.FirstOrDefault(), keyValue.LastOrDefault()?.Replace("$!#", "|"));
            }

            return collection;
        }
    }
}
