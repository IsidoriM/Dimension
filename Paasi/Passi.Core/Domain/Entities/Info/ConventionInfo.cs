namespace Passi.Core.Domain.Entities.Info
{
    [Serializable]
    class ConventionInfo
    {
        public int UserTypeId { get; set; } = int.MinValue;
        public string UserId { get; set; } = string.Empty;
        public string WorkOfficeCode { get; set; } = string.Empty;
        public ICollection<Convention> Conventions { get; set; } = new HashSet<Convention>();
        public bool HasConvention => !string.IsNullOrWhiteSpace(UserId);
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(0);
        public bool HasValidCacheSlot
        {
            get
            {
                return (DateTime.UtcNow.ToMilliseconds() - this.LastUpdate.ToMilliseconds()) < this.Timeout.TotalMilliseconds;
            }
        }
    }
}
