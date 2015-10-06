using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using Util = CCUtil.CCUtil;

namespace ConnectionTest
{
    class ConnectionTest
    {
        static void ping(string domain, StreamWriter sw)
        {
            List<string> ips = Util.ReadLines("ip--" + domain + ".txt", 4);
            sw.WriteLine("---PING " + domain + " " + DateTime.Now.ToString() + "---");
            List<int> rtts = Util.PingWithHttpGet(ips);
            int i = 0;
            foreach (string ip in ips)
            {
                sw.WriteLine(ip + " " + rtts[i++]);
            }
        }

        static void Main(string[] args)
        {
            StreamWriter sw = new StreamWriter("log/test" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt");
            sw.WriteLine("---START " + DateTime.Now.ToString() + "---");
            ping("content.dropboxapi.com", sw);


            sw.WriteLine("---END " + DateTime.Now.ToString() + "---");
            sw.Close();
        }
    }
}
