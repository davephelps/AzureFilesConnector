// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.AzureFiles
{
    using System.Collections.Generic;
    using Microsoft.Azure.WebJobs.Description;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Service Provider class which injects all the service operation provider..
    /// </summary>
    [Extension("AzureFilesServiceProvider", configurationSection: "AzureFilesServiceProvider")]
    public class AzureFilesServiceProvider : IExtensionConfigProvider
    {
        /// <summary>
        /// Register the service provider.
        /// </summary>
        /// <param name="serviceOperationsProvider"></param>
        /// <param name="operationsProvider"></param>
        public AzureFilesServiceProvider(ServiceOperationsProvider serviceOperationsProvider,
            AzureFilesServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(serviceName: AzureFilesServiceOperationProvider.ServiceName, serviceOperationsProviderId: AzureFilesServiceOperationProvider.ServiceId, serviceOperationsProviderInstance: operationsProvider);
        }

        /// <summary>
        /// You can add any custom implementation in Initialize method.
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(ExtensionConfigContext context)
        {
        }
    }
}
