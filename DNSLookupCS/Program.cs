using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSLookupCS
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader("nameservers.txt");
            string[] dns = new string[120000];
            int count = 0;
            while (!sr.EndOfStream)
            {
                string ts = sr.ReadLine();
                if (ts.Length > 4) dns[count++] = ts;
            }
            Console.Write("Domain: ");
            string domain = Console.ReadLine();
            Console.Write("Start: ");
            int start = int.Parse(Console.ReadLine());
            Console.Write("End: ");
            int end = int.Parse(Console.ReadLine());
            HashSet<string> ipset = new HashSet<string>();
            for(int i=start;i<=Math.Min(end,count-1); i++)
            {
                if (dns[i].Contains(":")) continue;
                Console.WriteLine(i);
                try
                {
                    JHSoftware.DnsClient.RequestOptions opt = new JHSoftware.DnsClient.RequestOptions();
                    opt.DnsServers = new IPAddress[] { IPAddress.Parse(dns[i]) };
                    IPAddress[] ips = JHSoftware.DnsClient.LookupHost(domain, JHSoftware.DnsClient.IPVersion.IPv4, opt);
                    foreach (IPAddress ip in ips)
                    {
                        ipset.Add(ip.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            StreamWriter sw = new StreamWriter(domain +start+"_"+end+ ".txt");
            foreach (string ip in ipset)
            {
                sw.WriteLine(ip);
            }
            sw.Close();
        }
    }
}
