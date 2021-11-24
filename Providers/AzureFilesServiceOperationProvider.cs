// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using Connectors.AzureFilesCore;

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.AzureFiles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using Microsoft.Azure.Workflows.ServiceProviders.WebJobs.Abstractions.Providers;
    using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Json;
    using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class AzureFilesConnectionParameters : ConnectionParameters
    {
        /// <summary>
        /// Gets or sets the AzureFiles connection server name.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public ConnectionStringParameters FilesConnectionString { get; set; }

    }
    /// <summary>
    /// This is the service operation provider class where you define all the operations and apis.
    /// </summary>
    [ServiceOperationsProvider(Id = AzureFilesServiceOperationProvider.ServiceId, Name = AzureFilesServiceOperationProvider.ServiceName)]
    public class AzureFilesServiceOperationProvider : IServiceOperationsTriggerProvider
    {
        /// <summary>
        /// The service name.
        /// </summary>
        public const string ServiceOperationTrigger = "AzureFilesTrigger";
        public const string ServiceName = "AzureFilesbuiltin";
        public const string ServiceOperationGetFile = "AzureFilesGetFile";
        public const string ServiceOperationListFile = "AzureFilesList";
        public const string ServiceOperationUploadFile = "AzureFilesUploadFile";
        public const string ServiceOperationDeleteFile = "AzureFilesDeleteFile";
        public const string ServiceOperationCopyFileToBlob = "AzureFilesCopyFileToBlob";

        private TelemetryClient _telemetryClient;
        IServiceProviderLogger _logger;
        /// <summary>
        /// The service id.
        /// </summary>
        public const string ServiceId = "/serviceProviders/AzureFilesbuiltin";

        /// <summary>
        /// Gets or sets service Operations.
        /// </summary>
        private readonly List<ServiceOperation> serviceOperationsList;

        /// <summary>
        /// The set of all API Operations.
        /// </summary>
        private readonly InsensitiveDictionary<ServiceOperation> apiOperationsList;

        /// <summary>
        /// Constructor for Service operation provider.
        /// </summary>
        
        public AzureFilesServiceOperationProvider(IServiceProviderLogger logger)
        {
            _logger = logger;
            this.serviceOperationsList = new List<ServiceOperation>();
            this.apiOperationsList = new InsensitiveDictionary<ServiceOperation>();

            this.apiOperationsList.AddRange(new InsensitiveDictionary<ServiceOperation>
            {
//                { ServiceOperationTrigger, AzureFilesTriggerOperation() },
                { ServiceOperationListFile, AzureFilesListOperation() },
                { ServiceOperationGetFile,AzureFilesGetFile() },
                { ServiceOperationUploadFile,AzureFilesUploadFile() },
                { ServiceOperationDeleteFile,AzureFilesDeleteFile() },
                { ServiceOperationCopyFileToBlob,AzureFilesCopyFileToBlob() },


            });

            this.serviceOperationsList.AddRange(new List<ServiceOperation>
            {
//                                { AzureFilesTriggerOperation().CloneWithManifest(AzureFilesTriggerOperationServiceOperationManifest()) },
                                { AzureFilesListOperation().CloneWithManifest(AzureFilesListOperationServiceOperationManifest()) },
                                { AzureFilesGetFile().CloneWithManifest(AzureFilesGetOperationServiceOperationManifest()) },
                                { AzureFilesUploadFile().CloneWithManifest(AzureFilesUploadOperationServiceOperationManifest()) },
                                { AzureFilesDeleteFile().CloneWithManifest(AzureFilesDeleteOperationServiceOperationManifest()) },
                                { AzureFilesCopyFileToBlob().CloneWithManifest(AzureFilesCopyFileToBlobOperationServiceOperationManifest()) }
            });
        }
        public string GetFunctionTriggerType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get binding connection information, needed for Azure function triggers.
        /// </summary>
        /// <param name="operationId"></param>
        /// <param name="connectionParameters"></param>
        /// <returns></returns>
        public string GetBindingConnectionParameter(string operationId, InsensitiveDictionary<JToken> connectionParameters, string paramName)
        {
            return ServiceOperationsProviderUtilities
                    .GetRequiredParameterValue(
                        serviceId: ServiceId,
                        operationId: operationId,
                        parameterName: paramName,
                        parameters: connectionParameters)?
                    .ToValue<string>();
        }
        public string GetBindingConnectionInformation(string operationId, InsensitiveDictionary<JToken> connectionParameters)
        {
            return ServiceOperationsProviderUtilities
                    .GetRequiredParameterValue(
                        serviceId: ServiceId,
                        operationId: operationId,
                        parameterName: "FilesConnectionString",
                        parameters: connectionParameters)?
                    .ToValue<string>();
        }

        /// <summary>
        /// Get operations.
        /// </summary>
        /// <param name="expandManifest">Expand manifest generation.</param>
        public IEnumerable<ServiceOperation> GetOperations(bool expandManifest)
        {
            return expandManifest ? serviceOperationsList : GetApiOperations();
        }

        /// <summary>
        /// Gets the api operations.
        /// </summary>
        private IEnumerable<ServiceOperation> GetApiOperations()
        {
            return this.apiOperationsList.Values;
        }

        /// <summary>
        /// Get service operation.
        /// </summary>
        public ServiceOperationApi GetService()
        {
            return this.GetServiceOperationApi();
        }


        private ServiceOperationApi GetServiceOperationApi()
        {
            ServiceOperationApi api = 
            new ServiceOperationApi
            {
                Name = "AzureFilesbuiltin",
                Id = ServiceId,
                Type = DesignerApiType.ServiceProvider,
                Properties = new ServiceOperationApiProperties
                {
                    
                    BrandColor = 0xC4D5FF,
                    Description = "Connect to AzureFiles.",
                    DisplayName = "AzureFiles",
                    IconUri = new Uri("https://iconsdp.blob.core.windows.net/icons/AzureFiles.png"),
                    
                    Capabilities = new ApiCapability[] { ApiCapability.Actions,ApiCapability.Triggers},

                    ConnectionParameters = new AzureFilesConnectionParameters
                    {
                        FilesConnectionString = new ConnectionStringParameters
                        {
                            Type = ConnectionStringType.StringType,
                            ParameterSource = ConnectionParameterSource.AppConfiguration,
                            UIDefinition = new UIDefinition
                            {
                                DisplayName = "Files Connection String",
                                Description = "Files Connection String",
                                Tooltip = "Files Connection String",
                                Constraints = new Constraints
                                {
                                    Required = "true",
                                },
                            },
                        },
                    }
                }
            };

            return (api);
        }

        private ServiceOperationManifest AzureFilesListOperationServiceOperationManifest()
        {
            return new ServiceOperationManifest
            {
                ConnectionReference = new ConnectionReferenceFormat
                {
                    ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                },
                Settings = new OperationManifestSettings
                {
                    SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                    TrackedProperties = new OperationManifestSetting
                    {
                        Scopes = new OperationScope[] { OperationScope.Action },
                    },
                },
                InputsLocation = new InputsLocation[]
               {
               InputsLocation.Inputs,
               InputsLocation.Parameters,
               },
                Outputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.Object,
                                Properties = new OrdinalDictionary<SwaggerSchema>
                                {
                                    {
                                        "ShareName", new SwaggerSchema
                                        {
                                            Type = SwaggerSchemaType.String,
                                            Title = "Share Name",
                                            Format = "string",
                                            Description = "ShareName",
                                        }
                                    },
                                    {
                                        "DirectoryName", new SwaggerSchema
                                        {
                                            Type = SwaggerSchemaType.String,
                                            Title = "Directory Name",
                                            Format = "string",
                                            Description = "Directory Name",
                                        }
                                    },
                                    {
                                        "FileList", new SwaggerSchema
                                        {
                                            Type = SwaggerSchemaType.Array,
                                            Title = "AzureFiles File List",
                                            Description = "AzureFiles File List",
                                            Items = new SwaggerSchema
                                            {
                                                Type = SwaggerSchemaType.Object,
                                                Properties = new OrdinalDictionary<SwaggerSchema>
                                                {
                                                    {
                                                        "Name", new SwaggerSchema
                                                        {
                                                            Type = SwaggerSchemaType.String,
                                                            Title = "Name",
                                                            Format = "string",
                                                            Description = "Name",
                                                        }
                                                    },
                                                    {
                                                        "IsDirectory", new SwaggerSchema
                                                        {
                                                            Type = SwaggerSchemaType.String,
                                                            Title = "Is Directory",
                                                            Format = "string",
                                                            Description = "Is Directory (true/false)",
                                                        }
                                                    }

                                                    },
                                                },
                                            }
                                        },

                                    }
                            }
                        }
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                {
                    {
                        "fileShare", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "File Share",
                            Description = "Azure Files share name",
                        }
                    },
                    {
                        "folder", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Folder",
                            Description = "Folder in the share",
                        }
                    }
                },
                    Required = new string[]
                   {
                   "fileShare",
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }

        private ServiceOperationManifest AzureFilesTriggerOperationServiceOperationManifest()
        {
            return new ServiceOperationManifest
            {
                ConnectionReference = new ConnectionReferenceFormat
                {
                    ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                },
                Settings = new OperationManifestSettings
                {
                    SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                    TrackedProperties = new OperationManifestSetting
                    {
                        Scopes = new OperationScope[] { OperationScope.Trigger },
                    },
                },
                InputsLocation = new InputsLocation[]
               {
               InputsLocation.Inputs,
               InputsLocation.Parameters,
               },
                Outputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.Object,
                                Properties = new OrdinalDictionary<SwaggerSchema>
                                {
                                    {
                                        "ShareName", new SwaggerSchema
                                        {
                                            Type = SwaggerSchemaType.String,
                                            Title = "Share Name",
                                            Format = "string",
                                            Description = "ShareName",
                                        }
                                    },
                                    {
                                        "DirectoryName", new SwaggerSchema
                                        {
                                            Type = SwaggerSchemaType.String,
                                            Title = "Directory Name",
                                            Format = "string",
                                            Description = "Directory Name",
                                        }
                                    },
                                    {
                                        "FileList", new SwaggerSchema
                                        {
                                            Type = SwaggerSchemaType.Array,
                                            Title = "AzureFiles File List",
                                            Description = "AzureFiles File List",
                                            Items = new SwaggerSchema
                                            {
                                                Type = SwaggerSchemaType.Object,
                                                Properties = new OrdinalDictionary<SwaggerSchema>
                                                {
                                                    {
                                                        "Name", new SwaggerSchema
                                                        {
                                                            Type = SwaggerSchemaType.String,
                                                            Title = "Name",
                                                            Format = "string",
                                                            Description = "Name",
                                                        }
                                                    },
                                                    {
                                                        "IsDirectory", new SwaggerSchema
                                                        {
                                                            Type = SwaggerSchemaType.String,
                                                            Title = "Is Directory",
                                                            Format = "string",
                                                            Description = "Is Directory (true/false)",
                                                        }
                                                    }

                                                    },
                                                },
                                            }
                                        },

                                    }
                            }
                        }
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                {
                    {
                        "fileShare", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "File Share",
                            Description = "Azure Files share name",
                        }
                    },
                    {
                        "folder", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Folder",
                            Description = "Folder in the share",
                        }
                    },
                    {
                        "PrefixFilter", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Prefix",
                            Format = "string",
                            Description = "Prefix filter",
                        }
                    },

                },
                    Required = new string[]
                   {
                   "fileShare",
                   },
                },
                Connector = this.GetServiceOperationApi(),
                Trigger = TriggerType.Batch,
                Recurrence = new RecurrenceSetting
                {
                    Type = RecurrenceType.Basic,
                },
            };
        }

        private ServiceOperationManifest AzureFilesCopyFileToBlobOperationServiceOperationManifest()
        {
            return new ServiceOperationManifest
            {
                ConnectionReference = new ConnectionReferenceFormat
                {
                    ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                },
                Settings = new OperationManifestSettings
                {
                    SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                    TrackedProperties = new OperationManifestSetting
                    {
                        Scopes = new OperationScope[] { OperationScope.Action },
                    },
                },
                InputsLocation = new InputsLocation[]
               {
               InputsLocation.Inputs,
               InputsLocation.Parameters,
               },
                Outputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.Array,
                                Title = "Copy File to Blob",
                                Description = "Copy File to Blob",
                                Items = new SwaggerSchema
                                {
                                    Type = SwaggerSchemaType.Object,
                                    Properties = new OrdinalDictionary<SwaggerSchema>
                                    {
                                        {
                                            "Success", new SwaggerSchema
                                            {
                                                Type = SwaggerSchemaType.String,
                                                Title = "Name",
                                                Format = "string",
                                                Description = "Name",
                                            }
                                        }
                                        },
                                    },
                                }
                            },

                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                {
                    {
                        "fileShare", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "File Share",
                            Description = "Azure Files share name",
                        }
                    },
                    {
                        "folder", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Folder",
                            Description = "Folder in the share",
                        }
                    },
                    {
                        "sourcefile", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Source File Name",
                            Description = "Source File Name",
                        }
                    },
                    {
                        "blobconnection", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Blob Connection String",
                            Description = "Blob Connection String",
                        }
                    },
                    {
                        "blobfolder", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Blob Folder",
                            Description = "Blob Folder",
                        }
                    },
                    {
                        "overwrite", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.Boolean,
                            Title = "Overwrite",
                            Description = "Overwrite if exists",
                        }
                    }

                },
                    Required = new string[]
                   {
                   "fileShare",
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }

        private ServiceOperationManifest AzureFilesDeleteOperationServiceOperationManifest()
        {
            return new ServiceOperationManifest
            {
                ConnectionReference = new ConnectionReferenceFormat
                {
                    ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                },
                Settings = new OperationManifestSettings
                {
                    SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                    TrackedProperties = new OperationManifestSetting
                    {
                        Scopes = new OperationScope[] { OperationScope.Action },
                    },
                },
                InputsLocation = new InputsLocation[]
               {
               InputsLocation.Inputs,
               InputsLocation.Parameters,
               },
                Outputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        { "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Result",
                                Description = "Result"
                            }
                        }
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "inputParam", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "File path",
                                Description = "Destination file path",
                            }
                        }
                    },
                    Required = new string[]
                   {
                   "inputParam"                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }


        private ServiceOperationManifest AzureFilesUploadOperationServiceOperationManifest()
        {
            return new ServiceOperationManifest
            {
                ConnectionReference = new ConnectionReferenceFormat
                {
                    ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                },
                Settings = new OperationManifestSettings
                {
                    SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                    TrackedProperties = new OperationManifestSetting
                    {
                        Scopes = new OperationScope[] { OperationScope.Action },
                    },
                },
                InputsLocation = new InputsLocation[]
               {
               InputsLocation.Inputs,
               InputsLocation.Parameters,
               },
                Outputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        { "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Body",
                                Description = "Body"
                            }
                        }
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        {
                            "inputParam", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "File path",
                                Description = "Destination file path",
                            }
                        },
                        {
                            "content", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Content",
                                Description = "Content to upload",
                            }
                        }

                    },
                    Required = new string[]
                   {
                   "inputParam",
                   "content"
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }

        private ServiceOperationManifest AzureFilesGetOperationServiceOperationManifest()
        {
            return new ServiceOperationManifest
            {
                ConnectionReference = new ConnectionReferenceFormat
                {
                    ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
                },
                Settings = new OperationManifestSettings
                {
                    SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                    TrackedProperties = new OperationManifestSetting
                    {
                        Scopes = new OperationScope[] { OperationScope.Action },
                    },
                },
                InputsLocation = new InputsLocation[]
               {
               InputsLocation.Inputs,
               InputsLocation.Parameters,
               },
                Outputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                    {
                        { "body", new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.String,
                                Title = "Body",
                                Description = "Body"
                            } 
                        }                    
                    }
                },
                Inputs = new SwaggerSchema
                {
                    Type = SwaggerSchemaType.Object,
                    Properties = new OrdinalDictionary<SwaggerSchema>
                {
                    {
                        "fileShare", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "File Share",
                            Description = "Azure Files share name",
                        }
                    },
                    {
                        "folder", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Folder",
                            Description = "Folder in the share",
                        }
                    },
                    {
                        "sourcefile", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.String,
                            Title = "Source File Name",
                            Description = "Source File Name",
                        }
                    }
                },
                    Required = new string[]
                   {
                       "sourcefile",
                       "folder",
                       "fileShare"
                   },
                },
                Connector = this.GetServiceOperationApi(),
            };
        }

        /// <summary>
        /// The debug action.
        /// </summary>
        public ServiceOperation AzureFilesTriggerOperation()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationTrigger,
                Id = ServiceOperationTrigger,
                Type = ServiceOperationTrigger,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Azure Files Receive Files",
                    Description = "Azure Files Receive Files",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://iconsdp.blob.core.windows.net/icons/AzureFiles.png"),
                    Trigger = TriggerType.Batch

                },
            };
            return (Operation);
        }

        public ServiceOperation AzureFilesListOperation()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationListFile,
                Id = ServiceOperationListFile,
                Type = ServiceOperationListFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "List Files",
                    Description = "List Files",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://iconsdp.blob.core.windows.net/icons/AzureFiles.png")

                },
            };
            return (Operation);
        }
        public ServiceOperation AzureFilesGetFile()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationGetFile,
                Id = ServiceOperationGetFile,
                Type = ServiceOperationGetFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Get File",
                    Description = "Get File",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://iconsdp.blob.core.windows.net/icons/AzureFiles.png")
                },
            };
            return (Operation);
        }

        
        public ServiceOperation AzureFilesCopyFileToBlob()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationCopyFileToBlob,
                Id = ServiceOperationCopyFileToBlob,
                Type = ServiceOperationCopyFileToBlob,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Copy File to Blob",
                    Description = "Copy File to Blob",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://iconsdp.blob.core.windows.net/icons/AzureFiles.png")
                },
            };
            return (Operation);
        }

        public ServiceOperation AzureFilesDeleteFile()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationDeleteFile,
                Id = ServiceOperationDeleteFile,
                Type = ServiceOperationDeleteFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Delete File",
                    Description = "Delete File",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://iconsdp.blob.core.windows.net/icons/AzureFiles.png")
                },
            };
            return (Operation);
        }

        public ServiceOperation AzureFilesUploadFile()
        {
            ServiceOperation Operation = new ServiceOperation
            {
                Name = ServiceOperationUploadFile,
                Id = ServiceOperationUploadFile,
                Type = ServiceOperationUploadFile,
                Properties = new ServiceOperationProperties
                {
                    Api = this.GetServiceOperationApi().GetFlattenedApi(),
                    Summary = "Upload File",
                    Description = "Upload File",
                    Visibility = Visibility.Important,
                    //This is the magic setting that makes it an Action
                    OperationType = OperationType.ServiceProvider,
                    BrandColor = 0x1C3A56,
                    IconUri = new Uri("https://iconsdp.blob.core.windows.net/icons/AzureFiles.png")
                },
            };
            return (Operation);
        }

        private string GetParameter(ServiceOperationRequest serviceOperationRequest, string param)
        {
            string ret;

            var paramVal = serviceOperationRequest.Parameters.GetValueOrDefault(param);

            if (paramVal == null)
            {
                throw new ApplicationException("Unable to read " + param);
            }
            ret = paramVal.ToString();

            return (ret);

        }
        private string GetParameter(ServiceOperationRequest serviceOperationRequest, string param, string defaultVal)
        {
            string ret;

            var paramVal = serviceOperationRequest.Parameters.GetValueOrDefault(param, defaultVal);

            if (paramVal == null)
            {
                throw new ApplicationException("Unable to read " + param);
            }
            ret = paramVal.ToString();

            return (ret);

        }
        private string GetParameter(ServiceOperationRequest serviceOperationRequest,string param, bool defaultVal )
        {
            string ret;

            var paramVal = serviceOperationRequest.Parameters.GetValueOrDefault(param, defaultVal);

            if (paramVal == null)
            {
                throw new ApplicationException("Unable to read " + param);
            }
            ret = paramVal.ToString();

            return (ret);

        }
        Task<ServiceOperationResponse> IServiceOperationsProvider.InvokeOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            string share = string.Empty;
            string filesConnectionString = string.Empty;

            ServiceOperationResponse resp = null;
            var opResp = Task.Run(() =>
            {

                try
                {
                    string shareFolder = string.Empty;

                    //FilesConnectionString = GetBindingConnectionInformation(operationId, connectionParameters);
                    var fileShare = serviceOperationRequest.Parameters.GetValueOrDefault("fileshare");

                    if (fileShare != null)
                    {
                        share = fileShare.ToString();
                    }

                    if (string.IsNullOrEmpty(share))
                    {
                        throw new ApplicationException("File Share");
                    }

                    filesConnectionString = GetBindingConnectionParameter(operationId, connectionParameters, "filesconnectionstring");

                    FilesConfig config = new FilesConfig(filesConnectionString);
                    Connectors.AzureFilesCore.AzureFiles AzureFiles = new Connectors.AzureFilesCore.AzureFiles(config);

                    _logger.Debug(ServiceName, operationId, "Called");
                    switch (operationId)
                    {
                        case ServiceOperationTrigger:
                            string prefixFilter  = GetParameter(serviceOperationRequest, "prefixfilter", "");
                            string triggerFolder = GetParameter(serviceOperationRequest, "folder", "");
                            var triggerFileList = AzureFiles.ListShare(share, triggerFolder, prefixFilter);

                            JProperty triggerOutProp = new JProperty("body", JProperty.FromObject(triggerFileList.Result));
                            resp = new ServiceOperationResponse(triggerOutProp.Value, System.Net.HttpStatusCode.OK);
                            break;

                        case ServiceOperationListFile:
                            string listPrefixFilter = GetParameter(serviceOperationRequest, "prefixfilter", "");
                            shareFolder = GetParameter(serviceOperationRequest, "folder","");
                            var fileList = AzureFiles.ListShare(share, shareFolder, listPrefixFilter);

                            JProperty outProp = new JProperty("body", JProperty.FromObject( fileList.Result));
                            resp = new ServiceOperationResponse(outProp.Value, System.Net.HttpStatusCode.OK);

                            break;

                        case ServiceOperationGetFile:
                            shareFolder = GetParameter(serviceOperationRequest, "folder", "");
                            string file = GetParameter(serviceOperationRequest, "sourcefile");
                            var fileContent = AzureFiles.DownloadFile(share, shareFolder, file);
                            JProperty fileProp = new JProperty("body", JProperty.FromObject(fileContent.Result));
                            resp = new ServiceOperationResponse(fileProp.Value, System.Net.HttpStatusCode.OK);

                            break;

                        case ServiceOperationCopyFileToBlob:
                            string storageFilesConnectionString;
                            var storageConn = serviceOperationRequest.Parameters.GetValueOrDefault("blobconnection");

                            if (storageConn == null)
                            {
                                throw new ApplicationException("Unable to read Storage Connection String");
                            }
                            storageFilesConnectionString = storageConn.ToString();

                            string copyshareFolder = GetParameter(serviceOperationRequest, "folder");
                            string sourcefile = GetParameter(serviceOperationRequest, "sourcefile");
                            string blobconnection = GetParameter(serviceOperationRequest, "blobconnection");
                            string blobfolder = GetParameter(serviceOperationRequest, "blobfolder");
                            string overwriteString = GetParameter(serviceOperationRequest, "overwrite",true);

                            bool overwrite = true;
                            bool.TryParse(overwriteString, out overwrite);

                            var copyFileOutput = AzureFiles.CopyFileToBlob(share, copyshareFolder, sourcefile,blobconnection, blobfolder, overwrite);

                            JProperty copyProp = new JProperty("body", JProperty.FromObject(copyFileOutput.Result));
                            resp = new ServiceOperationResponse(copyProp.Value, System.Net.HttpStatusCode.OK);

                            break;

                        default:
                            throw new NotImplementedException();
                    }

                }
                catch(FileNotFoundException ex)
                {
                    throw new ServiceOperationsProviderException(
                        httpStatus: HttpStatusCode.NotFound,
                        errorCode: ServiceOperationsErrorResponseCode.ServiceOperationFailed,
                        errorMessage: ex.Message,
                        innerException: ex.InnerException
                        );
                }
                catch (FileExistsException ex)
                {
                    throw new ServiceOperationsProviderException(
                        httpStatus: HttpStatusCode.Conflict,
                        errorCode: ServiceOperationsErrorResponseCode.ServiceOperationFailed,
                        errorMessage: ex.Message,
                        innerException: ex.InnerException
                        );
                }
                catch (Exception ex)
                {
                        throw new ServiceOperationsProviderException(
                            httpStatus: HttpStatusCode.InternalServerError,
                            errorCode: ServiceOperationsErrorResponseCode.ServiceOperationFailed,
                            errorMessage: ex.Message,
                            innerException: ex.InnerException
                            );
                }

                return (resp);
            });
            return (opResp);

        }

        public HttpStatusCode GetStatusCode(string code)
        {
            code = code.Trim();

            HttpStatusCode resp;

            if (code == "550") // file not found
            {
                resp = HttpStatusCode.NotFound;
            }
            else if (code.StartsWith("4")) // AzureFiles temporary failure
            {
                resp = HttpStatusCode.ServiceUnavailable;
            }
            else
            {
                resp = HttpStatusCode.InternalServerError;
            }

            return (resp);
        }

    }

}
