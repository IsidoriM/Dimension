namespace Passi.Core.Exceptions
{
    [Serializable]
    class NotFoundException : PassiException
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }
}
