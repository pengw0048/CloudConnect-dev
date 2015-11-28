using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using Util = CCUtil.CCUtil;
using System.Diagnostics;
using System.Net;

namespace SpeedTest_d
{
    class SpeedTest_d
    {
        
        [DataContract]
        class DownloadLink
        {

            [DataMember]
            public string url;

            [DataMember]
            public string expires;
        };

        private static string token = "K4g_u85PlIIAAAAAAAAAFKg_ygQtYqu0uqH15MBMzEs4YDnqmJ61U65tzdC-l6C_";

        static int SimpleUpload(int size = 1024 * 1024)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("Creating file...");
            Util.RandomFile(size, "simple.txt");
            Console.WriteLine("Uploading...");
            byte[] file = File.ReadAllBytes("simple.txt");
            watch.Start();
            string respHTML = Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/simple.txt?", token, file);
            watch.Stop();
            //File.Delete("simple.txt");
            //Console.WriteLine("Finished.");
            return (int)watch.ElapsedMilliseconds;
        }

        static string GetDownloadLink()
        {
            string respHTML = Util.HttpPost("https://api.dropboxapi.com/1/media/auto/simple.txt", "", token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DownloadLink));
            DownloadLink link = (DownloadLink)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return link.url;
        }

        static int Download(string url)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("Downloading...");
            watch.Start();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            WebResponse res = req.GetResponse();
            StreamReader s = new StreamReader(res.GetResponseStream());
            Console.WriteLine(s.ReadToEnd().Length);
            watch.Stop();
            Console.WriteLine("Finished.");
            return (int)watch.ElapsedMilliseconds;
        }

        static void Delete(string path)
        {
            string respHTML = Util.HttpPost("https://api.dropboxapi.com/1/fileops/delete?", "root=auto&path=" + path, token);
        }

        static void Main(string[] args)
        {
            List<int> time = new List<int>();
            for (int i = 1; i <= 50; i++)
            {
                Console.WriteLine(i);
                SimpleUpload(i * 1024 * 1024);
                //time.Add(SimpleUpload(i * 1024 * 1024));
                time.Add(Download(GetDownloadLink()));
                Delete("simple.txt");
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