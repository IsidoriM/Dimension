namespace Passi.Core.Exceptions
{
    [Serializable]
    class CLogException : PassiException
    {
        public CLogException()
        {
        }

        public CLogException(string message) : base(message)
        {
        }
    }
}
