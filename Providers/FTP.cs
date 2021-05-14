using FluentFTP;
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

namespace Connectors.FTPCore
{
    public class FTPConfig
    {
        public FTPConfig()
        {

        }
        public FTPConfig(string host, string username, string password, bool useSSL, bool implicitMode, bool useSelfSignedCert, bool activeMode, bool useBinaryMode)
        {
            UseSSL = useSSL;
            Host = host;
            UserName = username;
            Password = password;
            ImplicitMode = implicitMode;
            ActiveMode = activeMode;
            UseSelfSignedCert = useSelfSignedCert;
            UseBinaryMode = useBinaryMode;
        }

        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseSSL{ get; set; }
        public bool ActiveMode { get; set; }
        public bool ImplicitMode { get; set; }
        public bool UseSelfSignedCert { get; set; }
        public bool UseBinaryMode { get; set; }

    }
    public class FileList
    {
        public string Name{ get; set; }
        public string FullName { get; set; }
    }

    public class FTP
    {
        private FTPConfig _config;
        public FTP(FTPConfig config)
        {
            _config = config;
        }

        public FTP(string host, string username, string password, bool useSSL, bool implicitMode, bool useSelfSignedCert, bool activeMode, bool useBinaryMode)
        {
            _config = new FTPConfig
            {   UseSSL = useSSL,
                Host = host,
                UserName = username,
                Password = password,
                ImplicitMode = implicitMode,
                ActiveMode = activeMode,
                UseSelfSignedCert = useSelfSignedCert,
                UseBinaryMode = useBinaryMode
            };
        }
        private static void Client_ValidateCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            e.Accept = true;
        }

        public void ConfigureClient(FtpClient client)
        {
            if (_config.UseSSL)
            {
                if (_config.UseSelfSignedCert)
                {
                    client.ValidateCertificate += Client_ValidateCertificate;
                }

                if (_config.ImplicitMode)
                {
                    client.EncryptionMode = FtpEncryptionMode.Implicit;
                }
                else
                {
                    client.EncryptionMode = FtpEncryptionMode.Explicit;
                }
            }

            if(_config.ActiveMode)
            {
                client.DataConnectionType = FtpDataConnectionType.AutoActive;
            }

            client.Credentials = new NetworkCredential(_config.UserName, _config.Password);
        }
        public async Task<string> FluentUploadFile(string path,string data)
        {
            string content = string.Empty;
            try
            {
                FtpClient client = new FtpClient(_config.Host);
                ConfigureClient(client);

                await client.ConnectAsync();
                Stream responseStream = await client.OpenWriteAsync(path);

                if (_config.UseBinaryMode)
                {
                    byte[] binaryData = Convert.FromBase64String(data);

                    client.UploadDataType = FtpDataType.Binary;
                    using (BinaryWriter writer = new BinaryWriter(responseStream))
                    {
                        writer.Write(binaryData);
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(responseStream))
                    {
                        await writer.WriteAsync(data);
                    }
                }
                CancellationToken token = new CancellationToken();
                var reply = await client.GetReplyAsync(token);
                client.Disconnect();
            }
            catch
            {
                throw;
            }

            return (content);

        }

        public async Task<string> FluentGetFile(string path)
        {
            string content = string.Empty;
            try
            {
                FtpClient client = new FtpClient(_config.Host);
                ConfigureClient(client);

                await client.ConnectAsync();

                Stream responseStream = await client.OpenReadAsync(path);
                if (_config.UseBinaryMode)
                {
                    MemoryStream ms = new MemoryStream();
                    client.DownloadDataType = FtpDataType.Binary;
                    BinaryReader reader = new BinaryReader(responseStream);
                    await responseStream.CopyToAsync(ms);
                    ms.Position = 0;
                    content = Convert.ToBase64String(ms.ToArray());
                }
                else
                {
                    client.DownloadDataType = FtpDataType.ASCII;
                    StreamReader reader = new StreamReader(responseStream);
                    content = await reader.ReadToEndAsync();
                }

                await client.DisconnectAsync();
            }
            catch 
            {
                throw;
            }

            return (content);

        }

        public async Task<string> FluentDeleteFile(string path)
        {
            string resp = "success";
            try
            {
                FtpClient client = new FtpClient(_config.Host);
                ConfigureClient(client);

                await client.ConnectAsync();

                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                await client.DeleteFileAsync(path);
                await client.DisconnectAsync();

                return (resp);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<FileList>> FluentListFiles(string path)
        {
            string resp = string.Empty;
            try
            {
                FtpClient client = new FtpClient(_config.Host);
                ConfigureClient(client);

                client.DownloadDataType = FtpDataType.ASCII;

                await client.ConnectAsync();

                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                var list = await client.GetListingAsync(path);
                await client.DisconnectAsync();

                List<FileList> fileList = new List<FileList>();
                foreach (var file in list)
                {
                    fileList.Add(new FileList { Name = file.Name, FullName = file.FullName });
                }

                return (fileList);
            }
            catch
            {
                throw;
            }
        }
    }
}
