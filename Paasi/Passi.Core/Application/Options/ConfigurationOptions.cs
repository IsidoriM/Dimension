using Microsoft.AspNetCore.Http;
using Passi.Core.Domain.Const;
using System.Data;

namespace Passi.Core.Application.Options
{
    class ConfigurationOptions
    {
        public string SessionManagementFlag { get; set; } = string.Empty;
        public string Log { get; set; } = string.Empty;
        public int ServiceId { get; set; } = 0;
        public bool AllowServiceIdEditing { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
