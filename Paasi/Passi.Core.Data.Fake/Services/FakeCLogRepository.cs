using Passi.Core.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passi.Core.Store.Fake.Services
{
    internal class FakeCLogRepository : ICLogRepository
    {
        public async Task LogAsync(string userId, int eventId, string ip, int executionTime, int returnCode, int tipoUtente, [AllowNull] string? institutionCode, [AllowNull] string? workOfficeCode, [AllowNull] string? parameters, [AllowNull] string? errorMessage)
        {
            await Task.Delay(50);
        }
    }
}
