namespace Passi.Core.Exceptions
{
    [Serializable]
    class ParameterException : PassiException
    {
        public ParameterException(string message) : base(message)
        {
        }
    }
}
