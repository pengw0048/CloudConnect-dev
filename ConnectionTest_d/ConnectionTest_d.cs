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

namespace ConnectionTest_d
{
    class ConnectionTest_d
    {
        private static string d_token = "K4g_u85PlIIAAAAAAAAAFKg_ygQtYqu0uqH15MBMzEs4YDnqmJ61U65tzdC-l6C_";

        static void ping(string domain, StreamWriter sw, bool PingWithHttpGet = false)
        {
            List<string> ips = Util.ReadLines("ip--" + domain + ".txt", 2);
            sw.WriteLine("---PING " + domain + " " + DateTime.Now.ToString() + "---");
            sw.Flush();
            List<int> rtts = PingWithHttpGet ? Util.PingWithHttpGet(ips) : Util.Ping(ips);
            int i = 0;
            foreach (string ip in ips)
            {
                sw.WriteLine(ip + " " + rtts[i++]);
                sw.Flush();
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
            
            sw = new StreamWriter("log/dropbox" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            sw.Flush();
            ips = Util.ReadLines("ip--content.dropboxapi.com.txt");
            Console.WriteLine("Ping Dropbox");
            ping("content.dropboxapi.com", sw, true);
            
            Console.WriteLine("Upload 10M Dropbox");
            sw.WriteLine("--UPLOAD10M " + DateTime.Now.ToString() + "---");
            sw.Flush();
            foreach (string ip in ips)
            {
                try {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/1/files_put/auto/10M", d_token, data, 0, 10 * 1024 * 1024, null, true, false, false, 5 * 1000);
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    sw.Flush();
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception)
                {
                    sw.WriteLine(ip + " -1");
                    sw.Flush();
                    Console.WriteLine(ip + " -1");
                }
            }
            Console.WriteLine("Upload 1K Dropbox");
            sw.WriteLine("--UPLOAD1K " + DateTime.Now.ToString() + "---");
            sw.Flush();
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/1/files_put/auto/1K", d_token, data, 0, 1 * 1024, null, true, false, false, 3 * 1000);
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    sw.Flush();
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception)
                {
                    sw.WriteLine(ip + " -1");
                    sw.Flush();
                    Console.WriteLine(ip + " -1");
                }
            }
            Console.WriteLine("Download 10M Dropbox");
            sw.WriteLine("--DOWNLOAD10M " + DateTime.Now.ToString() + "---");
            sw.Flush();
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://" + ip + "/1/files/auto/10M", d_token, false, true, true, 5 * 1000);
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    sw.Flush();
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine(ip + " -1");
                    sw.Flush();
                    Console.WriteLine(ip + " -1");
                }
            }
            Console.WriteLine("Download 1K Dropbox");
            sw.WriteLine("--DOWNLOAD1K " + DateTime.Now.ToString() + "---");
            sw.Flush();
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://" + ip + "/1/files/auto/1K", d_token, false, true, true, 3 * 1000);
                    watch.Stop();
                    sw.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                    sw.Flush();
                    Console.WriteLine(ip + " " + watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine(ip + " -1");
                    sw.Flush();
                    Console.WriteLine(ip + " -1");
                }
            }

            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Flush();
            sw.Close();
            SSLUtil.RestoreValidation();
        }
    }
}
