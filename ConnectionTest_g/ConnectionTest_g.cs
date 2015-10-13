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

namespace ConnectionTest_g
{
    class ConnectionTest_g
    {
        private static string g_refresh_token = "1/8N6Yf_f-7KH0ms0fNH01zA4DRajTDradB6IxKMLU2RRIgOrJDtdun6zK6XiATCKT";
        private static string g_client_id = "920585866822-d71k4q781qqr4rhc17jkicjdbdcn9b9d.apps.googleusercontent.com";
        private static string g_client_secret = "_9rOg4xXuBQ8vDQblIcv4uZ5";


        [DataContract]
        class g_TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

        };

        [DataContract]
        class g_FileResource
        {

            [DataMember]
            public string id;

        };

        static string g_GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://www.googleapis.com/oauth2/v3/token", "client_id=" + g_client_id + "&client_secret=" + g_client_secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(g_TokenResponse));
            g_TokenResponse token = (g_TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return token.access_token;
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
            
            sw = new StreamWriter("log/googledrive" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            ips = Util.ReadLines("ip--www.googleapis.com.txt");
            string g_token = g_GetToken(g_refresh_token);
            Console.WriteLine("Ping GoogleDrive");
            ping("www.googleapis.com", sw);
            
            Console.WriteLine("Upload 10M GoogleDrive");
            sw.WriteLine("--UPLOAD10M " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    string respHTML = Util.HttpPost("https://" + ip + "/upload/drive/v2/files?uploadType=media", g_token, data, 0, 10 * 1024 * 1024, true, 5 * 1000, "www.googleapis.com");
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(g_FileResource));
                    g_FileResource file = (g_FileResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
                    Util.HttpDelete("https://www.googleapis.com/drive/v2/files/" + file.id, g_token);
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
                    string respHTML = Util.HttpPost("https://" + ip + "/upload/drive/v2/files?uploadType=media", g_token, data, 0, 1 * 1024, true, 3 * 1000, "www.googleapis.com");
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(g_FileResource));
                    g_FileResource file = (g_FileResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
                    Util.HttpDelete("https://www.googleapis.com/drive/v2/files/" + file.id, g_token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine(ip + " -1");
                    Console.WriteLine(ip + " -1");
                }
            }
            
            string id1 = g_SimpleUpload(g_token, data, 0, 10 * 1024 * 1024);
            string id2 = g_SimpleUpload(g_token, data, 0, 1 * 1024);
            Console.WriteLine("Download 10M GoogleDrive");
            sw.WriteLine("--DOWNLOAD10M " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://" + ip + "/drive/v2/files/" + id1 + "?alt=media", g_token, false, false, true, 5 * 1000, "www.googleapis.com");
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
            Util.HttpDelete("https://www.googleapis.com/drive/v2/files/" + id1, g_token);
            Util.HttpDelete("https://www.googleapis.com/drive/v2/files/" + id2, g_token);

            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();
            SSLUtil.RestoreValidation();
        }
    }
}
