namespace Passi.Core.Domain.Entities.Info
{
    [Serializable]
    class ProfileInfo
    {
        public string Id { get; set; } = string.Empty;
        public int ProfileTypeId { get; set; } = int.MinValue;
        public string FiscalCode { get; set; } = string.Empty;
        public string OfficeCode { get; set; } = string.Empty;
        public string InstitutionCode { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public DateTime Opening { get; set; } = DateTime.MinValue;
        public DateTime Closing { get; set; } = DateTime.MinValue;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(0);

        public ICollection<Service> Services { get; set; } = new HashSet<Service>();

        public bool HasProfile => !string.IsNullOrWhiteSpace(Id);
        
        public bool IsTimeSlotValid
        {
            get
            {
                var now = DateTime.UtcNow;
                return this.Opening < now && this.Closing > now;
            }
        }

        public bool IsStillValid
        {
            get
            {
                return (DateTime.UtcNow.ToMilliseconds() - this.LastUpdate.ToMilliseconds()) < this.Timeout.TotalMilliseconds;
            }
        }

    }
}
