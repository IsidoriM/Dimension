namespace System
{
    static class DateTimeExtensions
    {

        public static long ToMilliseconds(this DateTime datetime)
        {
            return (datetime.Ticks - DateTime.UnixEpoch.Ticks) / 10000;
        }

        public static DateTime ToDatetime(this string ms)
        {
            if (double.TryParse(ms, out double _ms))
                return DateTime.UnixEpoch.AddMilliseconds(_ms);

            if (DateTime.TryParse(ms, out DateTime _date))
                return _date;

            return DateTime.MinValue;
        }

        public static TimeSpan ToTimespan(this string data)
        {
            if (long.TryParse(data, out long _ms))
                return TimeSpan.FromSeconds(_ms);
            return new TimeSpan();
        }

        public static string ToBirthdayFormat(this DateTime data)
        {
            return data.ToString("dd/MM/yyyy");
        }
    }
}
