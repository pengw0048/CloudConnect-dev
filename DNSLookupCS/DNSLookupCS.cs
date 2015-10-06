using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DNSLookupCS
{
    class ThreadClass
    {
        public static int pos = 0;
        public static int count = 0;
        public static string domain;
        public static string[] dns = new string[120000];
        public static HashSet<string> res = new HashSet<string>();
        public void ThreadFunction()
        {
            while (pos < count)
            {
                if (pos % 1000 == 0) Console.WriteLine(pos);
                string tdns = dns[pos++];
                if (tdns.Contains(":")) continue;
                try
                {
                    JHSoftware.DnsClient.RequestOptions opt = new JHSoftware.DnsClient.RequestOptions();
                    opt.DnsServers = new IPAddress[] { IPAddress.Parse(tdns) };
                    IPAddress[] ips = JHSoftware.DnsClient.LookupHost(domain, JHSoftware.DnsClient.IPVersion.IPv4, opt);
                    foreach (IPAddress ip in ips)
                    {
                        res.Add(ip.ToString());
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
        }
    }
    class DNSLookupCS
    {
        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader("nameservers.txt");
            while (!sr.EndOfStream)
            {
                string ts = sr.ReadLine();
                if (ts.Length > 4) ThreadClass.dns[ThreadClass.count++] = ts;
            }
            Console.Write("Domain: ");
            ThreadClass.domain = Console.ReadLine();
            
            for(int i = 0; i < 50; i++)
            {
                ThreadClass aThreadClass = new ThreadClass();
                Thread aThread = new Thread(new ThreadStart(aThreadClass.ThreadFunction));
                aThread.Start();
            }
            while (ThreadClass.pos < ThreadClass.count)
            {
                Thread.Sleep(3000);
            }
            Thread.Sleep(10000);
            StreamWriter sw= new StreamWriter(ThreadClass.domain + ".txt");
            foreach (string ip in ThreadClass.res)
            {
                sw.WriteLine(ip);
            }
            sw.Close();
        }
    }
}
