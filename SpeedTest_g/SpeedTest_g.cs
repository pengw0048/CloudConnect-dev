using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using Util = CCUtil.CCUtil;
using System.Diagnostics;
using System.Net;

namespace SpeedTest_g
{
    class SpeedTest_g
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

        static int SimpleUpload(string token, int size = 1024 * 1024)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("Creating file...");
            Util.RandomFile(size, "simple.txt");
            Console.WriteLine("Uploading...");
            byte[] fileb = File.ReadAllBytes("simple.txt");
            watch.Start();
            string respHTML = Util.HttpPost("https://www.googleapis.com/upload/drive/v2/files?uploadType=media", token, fileb);
            watch.Stop();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(FileResource));
            FileResource file = (FileResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(file.id + " " + file.title + " " + file.fileSize + " " + file.mimeType + " " + file.downloadUrl);
            //File.Delete("simple.txt");
            Console.WriteLine("Finished.");
            Delete(token, file.id);
            return (int)watch.ElapsedMilliseconds;
        }

        static string GetDownloadLink(string token, string id)
        {
            return ("https://www.googleapis.com/drive/v2/files/" + id + "?alt=media");
        }

        static int Download(string token, int size = 1024 * 1024)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("Creating file...");
            Util.RandomFile(size, "simple.txt");
            Console.WriteLine("Uploading...");
            byte[] fileb = File.ReadAllBytes("simple.txt");
            watch.Start();
            string respHTML = Util.HttpPost("https://www.googleapis.com/upload/drive/v2/files?uploadType=media", token, fileb);
            watch.Stop();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(FileResource));
            FileResource file = (FileResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(file.id + " " + file.title + " " + file.fileSize + " " + file.mimeType + " " + file.downloadUrl);
            string url = GetDownloadLink(token, file.id);
            Console.WriteLine(url);
            Console.WriteLine("Downloading...");
            watch.Start();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = 50000;
            req.Headers.Add("Authorization", "Bearer " + token);
            req.Method = "GET";
            req.ReadWriteTimeout = 50000;
            WebResponse res = req.GetResponse();
            StreamReader s = new StreamReader(res.GetResponseStream());
            Console.WriteLine(s.ReadToEnd().Length);
            watch.Stop();
            Console.WriteLine("Finished.");
            Delete(token, file.id);
            return (int)watch.ElapsedMilliseconds;
        }

        static void Delete(string token, string id)
        {
            Util.HttpDelete("https://www.googleapis.com/drive/v2/files/" + id, token);
        }

        static void GetBasicInfo(string token)
        {
            string respHTML = Util.HttpGet("https://www.googleapis.com/drive/v2/about", token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DriveInfo));
            DriveInfo driveinfo = (DriveInfo)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(driveinfo.name + " " + driveinfo.quotaBytesUsedAggregate + "/" + driveinfo.quotaBytesTotal);
        }

        static void Main(string[] args)
        {
            string token = GetToken(refresh_token);
            GetBasicInfo(token);
            List<int> time = new List<int>();
            for (int i = 1; i <= 51; i += 10)
            {
                Console.WriteLine(i);
                //time.Add(SimpleUpload(token, i * 1024 * 1024));
                time.Add(Download(token, i * 1024 * 1024));
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            foreach (int val in time)
            {
                Console.WriteLine(val);
            }
        }
    }
}