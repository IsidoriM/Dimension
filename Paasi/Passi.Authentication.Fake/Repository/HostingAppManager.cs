using Microsoft.AspNetCore.Http;
using Passi.Core.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Passi.Authentication.Fake.Repository
{
    class HostingAppManager : IHostingAppManager
    {
        public Task ClearAllInfoAsync()
        {
            return Task.CompletedTask;
        }

        public Task ClearExternalInfoAsync()
        {
            return Task.CompletedTask;
        }
    }
}
