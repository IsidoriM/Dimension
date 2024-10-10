namespace Passi.Test.CookieAuthenticationWebApp.Models
{
    public class IndexModel
    {
        public IndexModel(int serviceId, int? srcPortal)
        {
            ServiceId = serviceId;
            SrcPortal = srcPortal;
        }

        public int ServiceId { get; }
        public int? SrcPortal { get; }
        public string Response { get; set; } = string.Empty;
    }
}
