// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Connectors.AzureFilesCore
{
    public class FileExistsException : Exception
    {
        public FileExistsException(string message)
        : base(message)
        {
        }

        public FileExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class FileDefinitionList
    {
        public List<FileDefinition> FileList { get; set; }
        public string ShareName { get; set; }
        public string DirectoryName { get; set; }
    }

    public class FileDefinition
    {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
    }

    public class FilesConfig
    {
        public FilesConfig()
        {

        }
        public FilesConfig(string filesConnectionString)
        {
            FilesConnectionString = filesConnectionString;
        }

        public string FilesConnectionString { get; set; }

    }

    public class AzureFiles
    {
        private FilesConfig _config;
        public AzureFiles(FilesConfig config)
        {
            _config = config;
        }

        public AzureFiles(string filesConnectionString)
        {
            _config = new FilesConfig
            {   FilesConnectionString = filesConnectionString,
            };
        }

        public async Task<string> DownloadFile(string fileShareName, string fileSourcePath, string fileSourceName)
        {
            string content = string.Empty;
            try
            {
                //string file = System.IO.Path.GetFileName(fileSourcePath);
                ShareClient share = new ShareClient(_config.FilesConnectionString, fileShareName);
                ShareDirectoryClient directory = share.GetDirectoryClient(fileSourcePath);
                ShareFileClient file = directory.GetFileClient(fileSourceName);
                //var contentType = file.GetProperties().Value.ContentType;

                bool exists = file.Exists();

                if (exists)
                {
                    using (Stream fileStream = await file.OpenReadAsync())
                    {
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            content = await reader.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException(fileShareName + "/" + fileSourceName + " not found");
                }
                return (content);

            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> CopyFileToBlob(string fileShareName, string fileSourcePath, string fileSourceName, string destBlobConn, string destBlobPath,bool overwrite)
        {
            string content = string.Empty;
            try
            {
                ShareClient share = new ShareClient(_config.FilesConnectionString, fileShareName);
                ShareDirectoryClient directory = share.GetDirectoryClient(fileSourcePath);
                ShareFileClient file = directory.GetFileClient(fileSourceName);

                bool exists = file.Exists();

                if (exists)
                {
                    using (Stream fileStream = await file.OpenReadAsync())
                    {
                        BlobServiceClient blobServiceClient = new BlobServiceClient(destBlobConn);
                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(destBlobPath);
                        BlobClient blobClient = containerClient.GetBlobClient(fileSourceName);
                        await blobClient.UploadAsync(fileStream,overwrite: overwrite);
                    }
                }
                else
                {
                    throw new FileNotFoundException(fileShareName + "/" + fileSourceName + " not found");
                }
                return (true);

            }
            catch(RequestFailedException ex)
            {
                string errorCode = ex.ErrorCode;

                if (errorCode == "BlobAlreadyExists")
                {
                    throw new FileExistsException(destBlobPath + "/" + fileSourceName + " already exists", ex);
                }
                throw;
            }
            catch(Exception ex)
            {
                throw;
            }

        }

        public async Task<FileDefinitionList> ListShare(string fileShareName, string fileSourceDir, string prefixFilter)
        {
            string content = string.Empty;
            try
            {
                ShareClient share = new ShareClient(_config.FilesConnectionString, fileShareName);
                ShareDirectoryClient directory = share.GetDirectoryClient(fileSourceDir);

                Pageable<ShareFileItem> fileList = directory.GetFilesAndDirectories(prefixFilter);

                FileDefinitionList list = new FileDefinitionList { ShareName = fileShareName, DirectoryName = fileSourceDir };
                list.FileList = new List<FileDefinition>();
                foreach(var f in fileList)
                {
                    list.FileList.Add(new FileDefinition { IsDirectory = f.IsDirectory, Name = f.Name });
                }

                return (list);

            }
            catch
            {
                throw;
            }

        }


    }
}
