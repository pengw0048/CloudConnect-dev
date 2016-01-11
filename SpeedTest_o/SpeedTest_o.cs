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

        private static string clientid = "000000004816FA26";
        private static string secret = "UBVcd-hCr6JYFnldXyY5pn85T2rkUGMW";
        private static string refresh_token = "MCtAD7A4A9Z1zIRBj3Rv6zUGRgPJRkwwWNnThF3J07UxaDhvJncJXD8jHk5Be3sApsShMLUaonNtRbZBUMkh69Gsuf5gdDbir9JYKTEoe1ujGkznVlsNZncsEIhSc3IwMN*qXyoGHlmHwUuSFRjh5mtb3iDXfxOZHuCyAmwax4UqPNKlMvrQLDXrvcpMZuoRsnQt504!X*m7tgssfsbK0SsMduE5KfSpYLolJQ0YRZvmsFIm2dLP1sFPHOJofuIBOHDK3J5Dk6Skw*9w9rrSJx13OS27*PHNJQVrmarvK0jhfLCCNCbpGgHc43zmn2uFZRuGLiMVFVf8KXL*1QEykCxyghNfl3RazcDJQCzK8c!TOmPcBJxph3VhW9pVyGfOtNA$$";
        private static string redirect_uri = "http://stomakun.tk/echo.php";

        [DataContract]
        class TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

            [DataMember]
            public string refresh_token;

        };

        static string GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://login.live.com/oauth20_token.srf", "client_id=" + clientid + "&redirect_uri=" + Uri.EscapeUriString(redirect_uri) + "&client_secret=" + secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TokenResponse));
            TokenResponse personinfo = (TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return personinfo.access_token;
        }

        static int SimpleUpload(string token, int size = 1024 * 1024)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("Creating file...");
            Util.RandomFile(size, "simple.txt");
            Console.WriteLine("Uploading...");
            byte[] file = File.ReadAllBytes("simple.txt");
            watch.Start();
            string respHTML = Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/simple.txt:/content", token, File.ReadAllBytes("simple.txt"), Timeout: 30000);
            watch.Stop();
            //File.Delete("simple.txt");
            Console.WriteLine("Finished.");
            return (int)watch.ElapsedMilliseconds;
        }

        static string GetDownloadLink(string token)
        {
            return Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/simple.txt:/content", token, true, false);
        }

        static int Download(string url)
        {
            Stopwatch watch = new Stopwatch();
            Console.WriteLine("Downloading...");
            watch.Start();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = 50000;
            req.ReadWriteTimeout = 50000;
            WebResponse res = req.GetResponse();
            StreamReader s = new StreamReader(res.GetResponseStream());
            Console.WriteLine(s.ReadToEnd().Length);
            watch.Stop();
            Console.WriteLine("Finished.");
            return (int)watch.ElapsedMilliseconds;
        }

        static void Delete(string path, string token)
        {
            Util.HttpDelete("https://api.onedrive.com/v1.0/drive/root:/" + path, token);
        }

        static void Main(string[] args)
        {
            string token = GetToken(refresh_token);
            List<int> time = new List<int>();
            for (int i = 1; i <= 51; i+=10)
            {
                Console.WriteLine(i);
                time.Add(SimpleUpload(token, i * 1024 * 1024));
                //SimpleUpload(token, i * 1024 * 1024);
                //time.Add(Download(GetDownloadLink(token)));
                Delete("simple.txt", token);
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