using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using Util = CCUtil.CCUtil;
using SSLUtil = CCUtil.SSLValidator;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Linq;

namespace ConnectionTest_o
{
    class ConnectionTest_o
    {
        private static string o_clientid = "000000004816FA26";
        private static string o_secret = "UBVcd-hCr6JYFnldXyY5pn85T2rkUGMW";
        private static string o_refresh_token = "MCtAD7A4A9Z1zIRBj3Rv6zUGRgPJRkwwWNnThF3J07UxaDhvJncJXD8jHk5Be3sApsShMLUaonNtRbZBUMkh69Gsuf5gdDbir9JYKTEoe1ujGkznVlsNZncsEIhSc3IwMN*qXyoGHlmHwUuSFRjh5mtb3iDXfxOZHuCyAmwax4UqPNKlMvrQLDXrvcpMZuoRsnQt504!X*m7tgssfsbK0SsMduE5KfSpYLolJQ0YRZvmsFIm2dLP1sFPHOJofuIBOHDK3J5Dk6Skw*9w9rrSJx13OS27*PHNJQVrmarvK0jhfLCCNCbpGgHc43zmn2uFZRuGLiMVFVf8KXL*1QEykCxyghNfl3RazcDJQCzK8c!TOmPcBJxph3VhW9pVyGfOtNA$$";
        private static string o_redirect_uri = "http://stomakun.tk/echo.php";
        

        [DataContract]
        class o_TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

            [DataMember]
            public string refresh_token;

        };
        
        static string o_GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://login.live.com/oauth20_token.srf", "client_id=" + o_clientid + "&redirect_uri=" + Uri.EscapeUriString(o_redirect_uri) + "&client_secret=" + o_secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(o_TokenResponse));
            o_TokenResponse personinfo = (o_TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return personinfo.access_token;
        }

        static string o_GetDownloadLink(string token, string file)
        {
            return Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/" + file + ":/content", token, true, false);
        }

        static void ping(string domain, StreamWriter sw, bool PingWithHttpGet = false)
        {
            List<string> ips = Util.ReadLines("ip--" + domain + ".txt", 2);
            sw.WriteLine("---PING " + domain + " " + DateTime.Now.ToString() + "---");
            List<int> rtts = PingWithHttpGet ? Util.PingWithHttpGet(ips) : Util.Ping(ips);
            int i = 0;
            foreach (string ip in ips)
            {
                sw.WriteLine(ip + " " + rtts[i++]);
            }
        }
        
        static void Main(string[] args)
        {
            SSLUtil.OverrideValidation();
            System.Net.ServicePointManager.DefaultConnectionLimit = 20000;
            StreamWriter sw = null;
            Stopwatch watch = new Stopwatch();
            List<string> ips = null;
            byte[] data = new byte[10 * 1024 * 1024];
            Random rand = new Random();
            for (int i = 0; i < data.Length; i++) data[i] = (byte)rand.Next(97, 97 + 25);
            
            sw = new StreamWriter("log/onedrive" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            ips = Util.ReadLines("ip--api.onedrive.com.txt");
            string o_token = o_GetToken(o_refresh_token);
            Console.WriteLine("Ping OneDrive");
            ping("api.onedrive.com", sw, true);

            Console.WriteLine("Upload 10M OneDrive");
            sw.WriteLine("--UPLOAD10M " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/v1.0/drive/root:/10M:/content", o_token, data, 0, 10 * 1024 * 1024, null, false, false, false, 5 * 1000, "api.onedrive.com");
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine(ip + " -1");
                    Console.WriteLine(ip + " -1");
                }
            }
            Console.WriteLine("Upload 1K OneDrive");
            sw.WriteLine("--UPLOAD1K " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/v1.0/drive/root:/1K:/content", o_token, data, 0, 1 * 1024, null, false, false, false, 3 * 1000, "api.onedrive.com");
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine(ip + " -1");
                    Console.WriteLine(ip + " -1");
                }
            }
            

            List<string>[] ipss = new List<string>[5];
            for (int i = 1; i <= 4; i++) ipss[i] = Util.ReadLines("ip--public.bn130" + i + ".livefilestore.com.txt");
            Console.WriteLine("Download 10M OneDrive");
            sw.WriteLine("--DOWNLOAD10M " + DateTime.Now.ToString() + "---");
            string file1 = o_GetDownloadLink(o_token, "10M").Split(new string[] { ".com" }, StringSplitOptions.None)[1];
            for (int i = 1; i <= 4; i++)
            {
                foreach (string ip in ipss[i])
                {
                    try
                    {
                        watch.Restart();
                        Util.HttpGet("https://" + ip + file1, o_token, false, false, true, 6 * 1000, "public.bn130" + i + ".livefilestore.com");
                        watch.Stop();
                        sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                        Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        sw.WriteLine(ip + " -1");
                        Console.WriteLine(ip + " -1");
                    }
                }
            }
            Console.WriteLine("Download 1K OneDrive");
            sw.WriteLine("--DOWNLOAD1K " + DateTime.Now.ToString() + "---");
            string file2 = o_GetDownloadLink(o_token, "1K").Split(new string[] { ".com" }, StringSplitOptions.None)[1];
            for (int i = 1; i <= 4; i++)
            {
                foreach (string ip in ipss[i])
                {
                    try
                    {
                        watch.Restart();
                        Util.HttpGet("https://" + ip + file2, o_token, false, false, true, 3 * 1000, "public.bn130" + i + ".livefilestore.com");
                        watch.Stop();
                        sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                        Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        sw.WriteLine(ip + " -1");
                        Console.WriteLine(ip + " -1");
                    }
                }
            }

            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();

            SSLUtil.RestoreValidation();
        }
    }
}
