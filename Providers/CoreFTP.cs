using System.Net;
using System.Threading.Tasks;

namespace SFTPCore
{
    public class FTPConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class CoreFTP
    {
        private SftpConfig _config;
        public CoreFTP(string host, int port, string username, string password)
        {
            _config = new SftpConfig
            {
                Host = host,
                Port = port,
                UserName = username,
                Password = password
            };
        }
        public async Task<string> GetFile(string path)
        {
            string s = string.Empty;
            using (System.IO.MemoryStream mem = new System.IO.MemoryStream())
            {
                _client.DownloadFile(path, mem);
                mem.Position = 0;
                var arr = mem.ToArray();
                mem.Position = 0;
                s = Encoding.ASCII.GetString(arr);
                _client.Dispose();
            }
            //string resp = Convert.ToBase64String(arr);
            //var s2 = Convert.FromBase64String(resp);
            //string s3 = Encoding.ASCII.GetString(s2);
            return (s);
        }
    }
}