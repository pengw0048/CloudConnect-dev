using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConTestProcessor
{
    class TestEntry
    {
        public int prefix;
        public int test;
        public DateTime time;
        public Dictionary<string, int> result;
    }

    class ConTestProcessor
    {
        static string[] prefix = new string[] { "dropbox", "onedrive", "googledrive" };
        static string[] test = new string[] { "PING", "UPLOAD10M", "UPLOAD1K", "DOWNLOAD10M", "DOWNLOAD1K" };

        static void Main(string[] args)
        {
            List<TestEntry>[] tests = new List<TestEntry>[test.Length];
            for (int i = 0; i < test.Length; i++) tests[i] = new List<TestEntry>();
            DirectoryInfo dir = new DirectoryInfo("log");
            foreach (FileInfo file in dir.GetFiles())
            {
                TestEntry entry = null;
                int pre;
                for(pre = 0; pre < prefix.Length; pre++)
                {
                    if (file.Name.StartsWith(prefix[pre])) break;
                }
                if (pre >= prefix.Length) continue;
                StreamReader sr = new StreamReader(file.FullName);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.Length < 4) continue;
                    if (line.StartsWith("--"))
                    {
                        if (entry != null)
                        {
                            tests[entry.test].Add(entry);
                            entry = null;
                        }
                        int tst;
                        for (tst = 0; tst < test.Length; tst++)
                        {
                            if (line.IndexOf(test[tst]) >= 1) break;
                        }
                        if (tst >= test.Length) continue;
                        entry = new TestEntry();
                        entry.test = tst;
                        entry.result = new Dictionary<string, int>();
                        entry.prefix = pre;
                        entry.time = DateTime.Parse(line.Replace("-", "").Substring(line.Replace("-", "").IndexOf(' ', line.IndexOf(' ')) + 1));
                    }
                    else
                    {
                        try {
                            entry.result.Add(line.Split(' ')[0], int.Parse(line.Split(' ')[1]));
                        }
                        catch { }
                    }
                }
                sr.Close();
            }
            Console.Write("Test server name: ");
            string sn = Console.ReadLine();
            for (int i = 0; i < prefix.Length; i++)
            {
                HashSet<string> ips = new HashSet<string>();
                for (int j = 0; j < test.Length; j++)
                {
                    foreach (TestEntry entry in tests[j])
                    {
                        if (entry.prefix != i) continue;
                        foreach (KeyValuePair<string, int> kvp in entry.result)
                        {
                            ips.Add(kvp.Key);
                        }
                    }
                }
                StreamWriter sw = new StreamWriter(sn + "_" + prefix[i] + ".csv");
                sw.Write("\"ip\"");
                for(int j = 0; j < test.Length; j++)
                {
                    foreach (TestEntry entry in tests[j])
                    {
                        if (entry.prefix != i) continue;
                        sw.Write(",\"" + test[j] + " " + entry.time.ToString() + "\"");
                    }
                    sw.Write(",\"" + test[j] + " average\"");
                }
                sw.WriteLine();
                foreach (string ip in ips)
                {
                    sw.Write("\"" + ip + "\"");
                    for (int j = 0; j < test.Length; j++)
                    {
                        int sum = 0;
                        int count = 0;
                        foreach (TestEntry entry in tests[j])
                        {
                            if (entry.prefix != i) continue;
                            count++;
                            if (entry.result.ContainsKey(ip))
                            {
                                int tv = entry.result[ip];
                                sw.Write("," + tv);
                                if (sum >= 0 && tv >= 0) sum += tv;
                                else if (sum <= 0) sum = -1;
                                else sum--;
                            }
                            else
                            {
                                sw.Write(",");
                            }
                        }
                        if (count > 0) sw.Write("," + (sum >= 0 ? sum / count : sum));
                        else sw.Write(",");
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }

        }
    }
}
