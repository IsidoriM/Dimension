using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TestMenuEnteMvc.Class
{
    public class CryptDecrypt
    {
        private static readonly byte[] Salt = Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["SaltValue"]);

        /// <summary>
        /// Cifra la stringa fornita come parametro usando l'algoritmo AES.
        /// La stringa può essere decifrata usando <c>DecryptStringAES</c>.
        /// Per generare la chiave di cifratura è usata la password secretKey.
        /// </summary>
        /// <param name="text">Il testo da cifrare.</param>
        /// <param name="secretKey">Una password usata per generare una chiave di cifratura.</param>
        /// <returns>Null se non ha potuto cifrare, la stringa cifrata in caso contrario</returns>
        public static string EncryptStringAes(string text, string secretKey)
        {
            ////SV: 
            if (text == null || string.IsNullOrEmpty(secretKey))
            {
                return null;
            }

            string encriptedText;
            RijndaelManaged aesAlg = null;

            try
            {
                // Genera una chiave di cifratura.
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(secretKey, Salt);

                // Crea un'istanza dell'oggetto RijndaelManaged che implementa l'algoritmo AES.
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                using (MemoryStream encryptMemoryStream = new MemoryStream())
                {
                    // Crea il cifratore a partire dalla chiave di cifratura e dal Vettore Iniziale (IV)
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    encryptMemoryStream.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    encryptMemoryStream.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                    using (CryptoStream encryptCryptoStream = new CryptoStream(encryptMemoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter encryptStreamWriter = new StreamWriter(encryptCryptoStream))
                        {
                            // Scrive tutto il testo nello stream cifrandolo di conseguenza.
                            encryptStreamWriter.Write(text);
                        }
                    }

                    encriptedText = Convert.ToBase64String(encryptMemoryStream.ToArray());
                }
            }
            finally
            {
                // Libera le risorse.
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }

            // ritorna il testo cifrato come stringa encodata in base 64.
            return encriptedText;
        }

        /// <summary>
        /// Decifra la stringa data come parametro. La stringa deve essere stata cifrata con
        /// <c>EncryptStringAES</c>.
        /// </summary>
        /// <param name="text">Il testo da decifrare.</param>
        /// <param name="secretKey">Una password usata per generare una chiave di decifratura.</param>
        /// <returns>Null se la stringa non può essere decodificata, la stringa decodificata altrimenti.</returns>
        public static string DecryptStringAes(string text, string secretKey)
        {
            ////SV:
            string decriptedText = null;

            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(secretKey))
            {
                RijndaelManaged aesAlg = null;

                try
                {
                    // Genera una chiave.
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(secretKey, Salt);
                    byte[] bytes = Convert.FromBase64String(text);

                    using (MemoryStream decryptMemoryStream = new MemoryStream(bytes))
                    {
                        // Crea un'istanza dell'oggetto RijndaelManaged che implementa l'algoritmo AES.
                        aesAlg = new RijndaelManaged();
                        aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                        // Prende l'IV (Vettore Iniziale)
                        aesAlg.IV = ReadByteArray(decryptMemoryStream);

                        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                        using (CryptoStream decryptCryptoStream = new CryptoStream(decryptMemoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptStreamReader = new StreamReader(decryptCryptoStream))
                            {
                                decriptedText = decryptStreamReader.ReadToEnd();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                    throw new Exception("DecryptStringAes: " + ex.ToString());
                }
                finally
                {
                    if (aesAlg != null)
                    {
                        aesAlg.Clear();
                    }
                }
            }

            return decriptedText;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Lo stream non contiene un array formattato correttamente.");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Errore durante la lettura dell'array.");
            }

            return buffer;
        }
    }
}