using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using Util = CCUtil.CCUtil;
using SSLUtil = CCUtil.SSLValidator;
using System.Diagnostics;

namespace ConnectionTest
{
    class ConnectionTest
    {
        private static string d_token = "K4g_u85PlIIAAAAAAAAAFKg_ygQtYqu0uqH15MBMzEs4YDnqmJ61U65tzdC-l6C_";

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
            byte[] data = null;
            sw = new StreamWriter("log/dropbox" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            // ping test
            ping("content.dropboxapi.com", sw, true);
            //ping("api.onedrive.com", sw, true);
            //ping("www.googleapis.com", sw);

            // upload test
            sw.WriteLine("--UPLOAD1M " + DateTime.Now.ToString() + "---");
            data = new byte[10 * 1024 * 1024];
            ips = Util.ReadLines("ip--content.dropboxapi.com.txt");
            foreach (string ip in ips)
            {
                try {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/1/files_put/auto/1M", d_token, data, 0, 1 * 1024 * 1024, null, true, false, false, 5 * 1000);
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
            sw.WriteLine("--UPLOAD8M " + DateTime.Now.ToString() + "---");
            foreach (string ip in ips)
            {
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://" + ip + "/1/files_put/auto/8M", d_token, data, 0, 8 * 1024 * 1024, null, true, false, false, 5 * 1000);
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
            SSLUtil.RestoreValidation();
        }
    }
}
