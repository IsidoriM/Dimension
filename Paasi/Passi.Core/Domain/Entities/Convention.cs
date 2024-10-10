namespace Passi.Core.Domain.Entities
{
    class Convention
    {
        public int ServiceId { get; internal set; } = 0;
        public bool IsAvailable { get; internal set; } = false;
        public ICollection<Filter> Filters { get; internal set; } = new HashSet<Filter>();
        public ICollection<Role> Roles { get; internal set; } = new HashSet<Role>();
    }
}
