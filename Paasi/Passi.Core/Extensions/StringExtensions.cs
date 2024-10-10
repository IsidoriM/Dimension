using System.Text.RegularExpressions;

namespace System
{
    static class StringExtensions
    {
        public static bool IsApi(this string s)
        {
            var pieces = s.ToLower().Trim('/').Split('/');
            var first = pieces.First();
            return first == "api";
        }

        public static T Random<T>() where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            Random random = new();
            return (T)values.GetValue(random.Next(values.Length))!;
        }

        public static string RandomString<T>(string exent) where T : struct
        {
            string result = RandomString<T>();

            while (result.ToString() == exent)
            {
                result = RandomString<T>();
            }
            return result;
        }

        public static string RandomString<T>() where T : struct
        {
            var x = from d in typeof(T).GetFields()
                    select d.GetRawConstantValue();
            Random random = new();
            return (string)x.ElementAt(random.Next(x.Count()))!;
        }

        public static string GetString(this string[] value, int position)
        {
            var _data = value.ElementAtOrDefault(position);
            if (_data?.ToLower() == "null")
                return string.Empty;
            if (!string.IsNullOrWhiteSpace(_data))
                return _data.Trim();
            return string.Empty;
        }

        public static string GetStringNoSpecialChars(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value.Trim()))
            {
                string patternStrict = @"^(a-z|A-Z|0-9)*[^#$%^&*()<>=]*$";
                Regex reStrict = new(patternStrict, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(5));
                value = reStrict.IsMatch(value) ? value.Trim().Replace('+', ' ').ToUpper() : string.Empty;
            }
            return value;
        }

        public static int GetInt(this string[] value, int position, int _default = -1)
        {
            var _data = value.ElementAtOrDefault(position);
            if (!string.IsNullOrWhiteSpace(_data) && int.TryParse(_data, out int _dataInt))
            {
                return _dataInt;
            }
            return _default;
        }

        public static bool GetBool(this string[] value, int position, bool _default = false)
        {
            var _data = value.ElementAtOrDefault(position);
            if (!string.IsNullOrWhiteSpace(_data))
            {
                return _data.ToBoolean(_default);
            }
            return _default;
        }

        public static bool ToBoolean(this string data, bool _default = false)
        {
            if (data == "1" || data.ToLower() == "true" || data.ToLower() == "mtrue")
                return true;

            if (data == "0" || data.ToLower() == "false" || data.ToLower() == "mfalse")
                return false;

            return _default;
        }

        public static string ObfuscateEmail(this string email)
        {
            //Email/PEC: AB***********CD@inps.it se len della parte sinistra > 6 caratteri ELSE AB***********@inps.it  
            //(in entrambi i casi gli * sono = numero caratteri sostituiti)
            if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
            {
                var index = email.IndexOf("@");

                string datoSx = email[..index];
                if (datoSx.Length < 7)
                {
                    if (datoSx.Length > 2)
                        email = string.Concat(datoSx.AsSpan(0, 2), new string('*', datoSx.Length - 2), email.AsSpan(index, email.Length - index));
                }
                else
                {
                    email = string.Concat(datoSx.AsSpan(0, 2), new string('*', datoSx.Length - 4), datoSx.AsSpan(datoSx.Length - 2, 2), email.AsSpan(index, email.Length - index));
                }
            }
            return email;
        }

        public static string ObfuscatePhoneNumber(this string phone)
        {
            //telefono e cellulare (partendo da destra) in chiaro le ultime 3 cifre, 4 oscurate, resto in chiaro: 065****365, 335****206, +39335****206
            //Tel. 0654***12 se len del numero < 12 caratteri ELSE 065405***12  (in entrambi i casi gli * sono = numero caratteri sostituiti) 
            // primo caso: 2 caratteri alla fine, 4 caratteri all'inizio e in mezzo tanti * quante sono le cifre rimanenti
            // secondo caso: 2 caratteri alla fine, 6 caratteri all'inizio e in mezzo tanti * quante sono le cifre rimanenti
            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (phone.Length < 12)
                {
                    if (phone.Length > 6)
                        phone = string.Concat(phone.AsSpan()[..4], new string('*', phone.Length - 6), phone.AsSpan(phone.Length - 2, 2));
                }
                else
                {
                    phone = string.Concat(phone.AsSpan()[..6], new string('*', phone.Length - 8), phone.AsSpan(phone.Length - 2, 2));
                }
            }
            return phone;
        }


    }
}
