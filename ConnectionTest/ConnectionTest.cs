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

        [DataContract]
        class g_TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

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

        static void Main(string[] args)
        {
            SSLUtil.OverrideValidation();
            StreamWriter sw = null;
            Stopwatch watch = new Stopwatch();
            List<string> ips = null;
            byte[] data = new byte[1 * 1024 * 1024];
            /*
            sw = new StreamWriter("log/dropbox" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
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
            
            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();

    */

            sw = new StreamWriter("log/googledrive" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            string g_token = g_GetToken(g_refresh_token);
            Console.WriteLine("Ping GoogleDrive");
            //ping("www.googleapis.com", sw);

            Console.WriteLine("Upload 10M GoogleDrive");
            sw.WriteLine("--UPLOAD10M " + DateTime.Now.ToString() + "---");
            ips = Util.ReadLines("ip--www.googleapis.com.txt");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPost("https://" + ip + "/upload/drive/v2/files?uploadType=media", g_token, data, 0, 10 * 1024 * 1024, false, 10 * 1000, "www.googleapis.com");
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
            }

            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();

            SSLUtil.RestoreValidation();
        }
    }
}
