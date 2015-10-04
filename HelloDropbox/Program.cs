using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System;
using Util = CCUtil.CCUtil;

namespace HelloDropbox
{
    class Program
    {

        private static string token = "K4g_u85PlIIAAAAAAAAAFKg_ygQtYqu0uqH15MBMzEs4YDnqmJ61U65tzdC-l6C_";

        [DataContract]
        class Person
        {

            [DataMember]
            public string display_name;

            [DataMember]
            public string email;

            [DataMember]
            public QuotaInfo quota_info;
        };

        [DataContract]
        class QuotaInfo
        {

            [DataMember]
            public long quota;

            [DataMember]
            public long normal;
        };

        [DataContract]
        class DownloadLink
        {

            [DataMember]
            public string url;

            [DataMember]
            public string expires;
        };

        [DataContract]
        class ChunkedUploadTask
        {

            [DataMember]
            public string upload_id;

            [DataMember]
            public long offset;

            [DataMember]
            public string expires;
        };

        [DataContract]
        class OfflineTask
        {

            [DataMember]
            public string status;

            [DataMember]
            public string job;
        };

        [DataContract]
        class OfflineStatus
        {

            [DataMember]
            public string status;

            [DataMember]
            public string error;
        };

        static void GetAccountInfo()
        {
            string respHTML = Util.HttpGet("https://api.dropboxapi.com/1/account/info", token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Person));
            Person personinfo = (Person)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(personinfo.display_name + " " + personinfo.email + " " + personinfo.quota_info.normal + "/" + personinfo.quota_info.quota);
        }

        static void SimpleUpload(int size = 1024 * 1024)
        {
            Util.RandomFile(size, "simple.txt");
            string respHTML = Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/simple.txt?", token, File.ReadAllBytes("simple.txt"));
            File.Delete("simple.txt");
        }

        static void GetDownloadLink()
        {
            string respHTML = Util.HttpPost("https://api.dropboxapi.com/1/media/auto/simple.txt", "", token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DownloadLink));
            DownloadLink link = (DownloadLink)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(link.url);
        }

        static void ChunkedUpload(int size = 1024 * 1024, int parts = 10)
        {
            Util.RandomFile(size, "chunked.txt");
            string respHTML = Util.HttpPut("https://content.dropboxapi.com/1/chunked_upload?", token, new byte[0]);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChunkedUploadTask));
            ChunkedUploadTask task = (ChunkedUploadTask)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));

            byte[] data = File.ReadAllBytes("chunked.txt");
            while (task.offset < data.Length)
            {
                int uplen = Math.Min(size / parts, data.Length - (int)task.offset);
                respHTML = Util.HttpPut("https://content.dropboxapi.com/1/chunked_upload?upload_id=" + task.upload_id + "&offset=" + task.offset,
                    token, data, (int)task.offset, uplen);
                task = (ChunkedUploadTask)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            }

            respHTML = Util.HttpPost("https://content.dropboxapi.com/1/commit_chunked_upload/auto/chunked.txt?", "upload_id=" + task.upload_id, token);
            File.Delete("chunked.txt");
        }

        static string OfflineDownload(string url, string path)
        {
            string respHTML = Util.HttpPost("https://api.dropboxapi.com/1/save_url/auto/" + path + "?", "url=" + url, token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(OfflineTask));
            OfflineTask link = (OfflineTask)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return link.job;
        }

        static string QueryOffline(string job)
        {
            string respHTML = Util.HttpGet("https://api.dropboxapi.com/1/save_url_job/" + job, token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(OfflineStatus));
            OfflineStatus status = (OfflineStatus)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return status.status;
        }

        static void Delete(string path)
        {
            string respHTML = Util.HttpPost("https://api.dropboxapi.com/1/fileops/delete?", "root=auto&path=" + path, token);
        }

        static void Main(string[] args)
        {
            GetAccountInfo();
            SimpleUpload();
            GetDownloadLink();
            ChunkedUpload(1024 * 1024, 4);
            Delete("simple.txt");
            Delete("chunked.txt");
            string job = OfflineDownload("http://www.worldoftanks-wot.com/wp-content/uploads/wot-september-free.jpg", "1.jpg");
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                string ret = QueryOffline(job);
                Console.WriteLine(ret);
                if (ret == "COMPLETE") break;
            }
            Delete("1.jpg");
        }
    }
}