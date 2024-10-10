namespace Passi.Core.Exceptions
{
    [Serializable]
    public abstract class PassiException : Exception
    {

        protected PassiException() : base()
        {
        }

        protected PassiException(string message) : base(message)
        {
        }


    }
}
