using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using Util = CCUtil.CCUtil;
using SSLUtil = CCUtil.SSLValidator;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;

namespace ConnectionTest
{
    class ConnectionTest
    {
        private static string d_token = "K4g_u85PlIIAAAAAAAAAFKg_ygQtYqu0uqH15MBMzEs4YDnqmJ61U65tzdC-l6C_";
        private static string g_refresh_token = "1/8N6Yf_f-7KH0ms0fNH01zA4DRajTDradB6IxKMLU2RRIgOrJDtdun6zK6XiATCKT";
        private static string g_client_id = "920585866822-d71k4q781qqr4rhc17jkicjdbdcn9b9d.apps.googleusercontent.com";
        private static string g_client_secret = "_9rOg4xXuBQ8vDQblIcv4uZ5";
        private static string o_clientid = "000000004816FA26";
        private static string o_secret = "UBVcd-hCr6JYFnldXyY5pn85T2rkUGMW";
        private static string o_refresh_token = "MCtAD7A4A9Z1zIRBj3Rv6zUGRgPJRkwwWNnThF3J07UxaDhvJncJXD8jHk5Be3sApsShMLUaonNtRbZBUMkh69Gsuf5gdDbir9JYKTEoe1ujGkznVlsNZncsEIhSc3IwMN*qXyoGHlmHwUuSFRjh5mtb3iDXfxOZHuCyAmwax4UqPNKlMvrQLDXrvcpMZuoRsnQt504!X*m7tgssfsbK0SsMduE5KfSpYLolJQ0YRZvmsFIm2dLP1sFPHOJofuIBOHDK3J5Dk6Skw*9w9rrSJx13OS27*PHNJQVrmarvK0jhfLCCNCbpGgHc43zmn2uFZRuGLiMVFVf8KXL*1QEykCxyghNfl3RazcDJQCzK8c!TOmPcBJxph3VhW9pVyGfOtNA$$";
        private static string o_redirect_uri = "http://stomakun.tk/echo.php";


        [DataContract]
        class g_TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

        };

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

        [DataContract]
        class g_FileResource
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

        static string g_GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://www.googleapis.com/oauth2/v3/token", "client_id=" + g_client_id + "&client_secret=" + g_client_secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(g_TokenResponse));
            g_TokenResponse token = (g_TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return token.access_token;
        }

        static string o_GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://login.live.com/oauth20_token.srf", "client_id=" + o_clientid + "&redirect_uri=" + Uri.EscapeUriString(o_redirect_uri) + "&client_secret=" + o_secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(o_TokenResponse));
            o_TokenResponse personinfo = (o_TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return personinfo.access_token;
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

        static string g_SimpleUpload(string token, byte[] data, int offset, int length)
        {
            string respHTML = Util.HttpPost("https://www.googleapis.com/upload/drive/v2/files?uploadType=media", token, data, offset, length);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(g_FileResource));
            g_FileResource file = (g_FileResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return file.id;
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
            
            /*sw = new StreamWriter("log/dropbox" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            Console.WriteLine("Ping Dropbox");
            ping("content.dropboxapi.com", sw, true);
            
            Console.WriteLine("Upload 10M Dropbox");
            sw.WriteLine("--UPLOAD10M " + DateTime.Now.ToString() + "---");
            ips = Util.ReadLines("ip--content.dropboxapi.com.txt");
            foreach (string ip in ips)
            {
                try {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/1/files_put/auto/10M", d_token, data, 0, 10 * 1024 * 1024, null, true, false, false, 8 * 1000);
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception)
                {
                    sw.WriteLine(ip + " -1");
                    Console.WriteLine(ip + " -1");
                }
            }
            Console.WriteLine("Upload 1K Dropbox");
            sw.WriteLine("--UPLOAD1K " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/1/files_put/auto/1K", d_token, data, 0, 1 * 1024, null, true, false, false, 3 * 1000);
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception)
                {
                    sw.WriteLine(ip + " -1");
                    Console.WriteLine(ip + " -1");
                }
            }
            //ips = Util.ReadLines("ip--content.dropboxapi.com.txt");
            Console.WriteLine("Download 10M Dropbox");
            sw.WriteLine("--DOWNLOAD10M " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://" + ip + "/1/files/auto/10M", d_token, false, true, true, 5 * 1000);
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
            Console.WriteLine("Download 1K Dropbox");
            sw.WriteLine("--DOWNLOAD1K " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://" + ip + "/1/files/auto/1K", d_token, false, true, true, 3 * 1000);
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

            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();*/

            sw = new StreamWriter("log/googledrive" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            string g_token = g_GetToken(g_refresh_token);
            /*Console.WriteLine("Ping GoogleDrive");
            ping("www.googleapis.com", sw);

            Console.WriteLine("Upload 10M GoogleDrive");
            sw.WriteLine("--UPLOAD10M " + DateTime.Now.ToString() + "---");
            ips = Util.ReadLines("ip--www.googleapis.com.txt");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPost("https://" + ip + "/upload/drive/v2/files?uploadType=media", g_token, data, 0, 10 * 1024 * 1024, false, 6 * 1000, "www.googleapis.com");
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
            Console.WriteLine("Upload 1K GoogleDrive");
            sw.WriteLine("--UPLOAD1K " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPost("https://" + ip + "/upload/drive/v2/files?uploadType=media", g_token, data, 0, 1 * 1024, false, 3 * 1000, "www.googleapis.com");
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
            }*/

            ips = Util.ReadLines("ip--www.googleapis.com.txt");
            string id1 = g_SimpleUpload(g_token, data, 0, 10 * 1024 * 1024);
            string id2 = g_SimpleUpload(g_token, data, 0, 1 * 1024);
            Console.WriteLine("Download 10M GoogleDrive");
            sw.WriteLine("--DOWNLOAD10M " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://" + ip + "/drive/v2/files/" + id1 + "?alt=media", g_token, false, false, true, 6 * 1000, "www.googleapis.com");
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
            Console.WriteLine("Download 1K GoogleDrive");
            sw.WriteLine("--DOWNLOAD1K " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://" + ip + "/drive/v2/files/" + id2 + "?alt=media", g_token, false, false, true, 3 * 1000, "www.googleapis.com");
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

            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();





            /*sw = new StreamWriter("log/onedrive" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            string o_token = o_GetToken(o_refresh_token);
            Console.WriteLine("Ping OneDrive");
            //ping("api.onedrive.com", sw, true);

            Console.WriteLine("Upload 10M OneDrive");
            sw.WriteLine("--UPLOAD10M " + DateTime.Now.ToString() + "---");
            ips = Util.ReadLines("ip--api.onedrive.com.txt");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/v1.0/drive/root:/10M:/content", o_token, data, 0, 10 * 1024 * 1024, null, false, false, false, 8 * 1000, "api.onedrive.com");
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
                    Util.HttpPut("https://" + ip + "/v1.0/drive/root:/10M:/content", o_token, data, 0, 1 * 1024, null, false, false, false, 3 * 1000, "api.onedrive.com");
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

            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();*/

            SSLUtil.RestoreValidation();
        }
    }
}
