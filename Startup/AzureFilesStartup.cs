// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(Microsoft.Azure.Workflows.ServiceProvider.Extensions.AzureFiles.AzureFilesStartup))]
namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.AzureFiles
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Hosting;
    using Microsoft.Azure.Workflows.ServiceProvider.Extensions.AzureFiles;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// This is a start up function, the discovery of this extension is based upon IWebJobsStartup implementation. 
    /// In the function log file you should be able to see the log "Loading startup extension 'FTPServiceProvider'"
    /// </summary>
    public class AzureFilesStartup : IWebJobsStartup
    {
        /// <summary>
        /// The Configure method is invoked as initialization of the extension.
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<AzureFilesServiceProvider>();
            builder.Services.TryAddSingleton<AzureFilesServiceOperationProvider>();
        }
    }
}
