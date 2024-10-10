using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Passi.Authentication.Cookie.Providers
{
    class RegistryKeyConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            string? registryKeyPath = "SOFTWARE\\APPLICAZIONI WEB\\Passi\\Produzione";
            return new RegistryKeyConfigurationProvider(registryKeyPath);
        }
    }

    class RegistryKeyConfigurationProvider : ConfigurationProvider
    {
        private readonly string registryKeyPath;

        public RegistryKeyConfigurationProvider(string registryKeyPath)
        {
            this.registryKeyPath = registryKeyPath;
        }
        public override void Load()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKeyPath);
                    if (key != null)
                    {
                        foreach (var valueName in key.GetValueNames())
                        {
                            var value = key.GetValue(valueName);
                            if (value != null)
                            {
                                Data.TryAdd(valueName, value.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

}
