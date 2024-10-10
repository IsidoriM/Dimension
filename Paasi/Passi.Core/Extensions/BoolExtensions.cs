namespace Passi.Core.Extensions
{
    static class BoolExtensions
    {
        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }
        public static string ToStringLower(this bool value)
        {
            return value.ToString().ToLower();
        }
    }
}
