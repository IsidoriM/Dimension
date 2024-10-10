using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passi.Core.Application.Repositories
{
    interface IHostingAppManager
    {
        public Task ClearExternalInfoAsync();
        public Task ClearAllInfoAsync();
    }
}
