using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System;
using Util = CCUtil.CCUtil;

namespace HelloGoogledrive
{
    class HelloGoogledrive
    {

        private static string refresh_token = "1/8N6Yf_f-7KH0ms0fNH01zA4DRajTDradB6IxKMLU2RRIgOrJDtdun6zK6XiATCKT";
        private static string client_id = "920585866822-d71k4q781qqr4rhc17jkicjdbdcn9b9d.apps.googleusercontent.com";
        private static string client_secret = "_9rOg4xXuBQ8vDQblIcv4uZ5";

        [DataContract]
        class TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

        };

        [DataContract]
        class DriveInfo
        {

            [DataMember]
            public string name;

            [DataMember]
            public string quotaBytesTotal;

            [DataMember]
            public string quotaBytesUsedAggregate;

        };

        [DataContract]
        class FileResource
        {

            [DataMember]
            public string id;

            [DataMember]
            public string title;

            [DataMember]
            public string mimeType;

            [DataMember]
            public string downloadUrl;

            [DataMember]
            public long fileSize;

        };

        static string GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://www.googleapis.com/oauth2/v3/token", "client_id=" + client_id + "&client_secret=" + client_secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TokenResponse));
            TokenResponse token = (TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return token.access_token;
        }

        static void GetBasicInfo(string token)
        {
            string respHTML = Util.HttpGet("https://www.googleapis.com/drive/v2/about", token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DriveInfo));
            DriveInfo driveinfo = (DriveInfo)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(driveinfo.name + " " + driveinfo.quotaBytesUsedAggregate + "/" + driveinfo.quotaBytesTotal);
        }

        static string SimpleUpload(string token, int size = 1024 * 1024)
        {
            Util.RandomFile(size, "simple.txt");
            string respHTML = Util.HttpPost("https://www.googleapis.com/upload/drive/v2/files?uploadType=media", token, File.ReadAllBytes("simple.txt"));
            File.Delete("simple.txt");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(FileResource));
            FileResource file = (FileResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(file.id + " " + file.title + " " + file.fileSize + " " + file.mimeType + " " + file.downloadUrl);
            return file.id;
        }

        static string ChunkedUpload(string token, int size = 1024 * 1024, int parts = 10)
        {
            Util.RandomFile(size, "chunked.txt");
            string location = Util.HttpPost("https://www.googleapis.com/upload/drive/v2/files?uploadType=resumable", "", token, false, true);

            byte[] data = File.ReadAllBytes("chunked.txt");
            int offset = 0;
            string ret = "";
            while (offset < data.Length)
            {
                int uplen = Math.Min(size / parts, data.Length - offset);
                ret = Util.HttpPut(location, token, data, offset, uplen,
                    "bytes " + offset + "-" + (offset + uplen - 1) + "/" + data.Length, false, true);
                if (ret.StartsWith("bytes") == false) break;
                offset = int.Parse(ret.Split('-')[1]) + 1;
            }
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(FileResource));
            FileResource file = (FileResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(ret)));
            Console.WriteLine(file.id + " " + file.title + " " + file.fileSize + " " + file.mimeType + " " + file.downloadUrl);
            File.Delete("chunked.txt");
            return file.id;
        }

        static string GetDownloadLink(string token, string id)
        {
            return Util.HttpGet("https://www.googleapis.com/drive/v2/files/" + id + "?alt=media", token, false, false);
        }

        static void Delete(string token, string id)
        {
            Util.HttpDelete("https://www.googleapis.com/drive/v2/files/" + id, token);
        }

        static void Main(string[] args)
        {
            string token = GetToken(refresh_token);
            GetBasicInfo(token);
            string id1 = SimpleUpload(token, 1024);
            string id2 = ChunkedUpload(token, 1024 * 1024, 3);
            GetDownloadLink(token, id1);
            Delete(token, id1);
            Delete(token, id2);
        }
    }
}
