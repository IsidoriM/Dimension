using System.Globalization;

namespace System.Data.SqlClient
{
    public static class SqlDataReaderExtensions
    {

        public static IDbCommand AddParameter(this IDbCommand command, string key, object value, DbType type, int? size = null)
        {
            if (!key.StartsWith('@'))
                key = "@" + key;

            var parameter = command.CreateParameter();
            parameter.ParameterName = key;
            parameter.Value = value;
            parameter.DbType = type;
            parameter.Size = 256;
            if (size != null)
                parameter.Size = size.Value;
            command.Parameters.Add(parameter);
            return command;
        }

        public static string GetStringValue(this DataRow dr, string key)
        {
            var item = dr[key];
            if (!string.IsNullOrWhiteSpace(Convert.ToString(item)))
            {
                var stringItem = Convert.ToString(item);
                return stringItem!.Trim();
            }
            return string.Empty;
        }

        public static string GetStringValue(this IDataReader dr, string key)
        {
            var ordinal = dr.GetOrdinal(key);
            var item = dr.GetValue(ordinal);
            if (!string.IsNullOrWhiteSpace(Convert.ToString(item)))
            {
                return item.ToString()!.Trim();
            }
            return string.Empty;
        }

        public static DateTime GetDateValue(this IDataReader dr, string key)
        {
            try
            {
                if (!dr.IsDBNull(dr.GetOrdinal(key)))
                {
                    return dr.GetDateTime(dr.GetOrdinal(key));
                }
            }
            catch (Exception)
            {
                var _value = GetStringValue(dr, key);
                if (!string.IsNullOrWhiteSpace(_value) && DateTime.TryParse(_value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _date))
                {
                    return _date;
                }

            }
            return DateTime.MinValue;
        }

        public static int GetIntValue(this DataRow dr, string key, int _default = -1)
        {
            var _value = GetStringValue(dr, key);
            if (!string.IsNullOrWhiteSpace(_value) && int.TryParse(_value, out int _intValue))
            {
                return _intValue;
            }

            return _default;
        }

        public static int GetIntValue(this IDataReader dr, string key, int _default = -1)
        {
            var _value = GetStringValue(dr, key);
            if (!string.IsNullOrWhiteSpace(_value) && int.TryParse(_value, out int _intValue))
            {
                return _intValue;
            }

            return _default;
        }
    }
}
