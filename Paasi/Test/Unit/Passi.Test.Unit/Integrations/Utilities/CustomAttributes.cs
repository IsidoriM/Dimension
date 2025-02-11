﻿namespace Passi.Test.Unit.Integrations.Utilities
{
    public sealed class IgnoreOnAzureDevopsFactAttribute : FactAttribute
    {
        public IgnoreOnAzureDevopsFactAttribute()
        {
            if (!IsRunningOnAzureDevOps())
            {
                return;
            }

            Skip = "Ignored on Azure DevOps";
        }

        /// <summary>Determine if runtime is Azure DevOps.</summary>
        /// <returns>True if being executed in Azure DevOps, false otherwise.</returns>
        public static bool IsRunningOnAzureDevOps()
        {
            return Environment.GetEnvironmentVariable("SYSTEM_DEFINITIONID") != null;
        }
    }

    public sealed class IgnoreOnAzureDevopsTheoryAttribute : TheoryAttribute
    {
        public IgnoreOnAzureDevopsTheoryAttribute()
        {
            if (!IsRunningOnAzureDevOps())
            {
                return;
            }

            Skip = "Ignored on Azure DevOps";
        }

        /// <summary>Determine if runtime is Azure DevOps.</summary>
        /// <returns>True if being executed in Azure DevOps, false otherwise.</returns>
        public static bool IsRunningOnAzureDevOps()
        {
            return Environment.GetEnvironmentVariable("SYSTEM_DEFINITIONID") != null;
        }
    }
}
