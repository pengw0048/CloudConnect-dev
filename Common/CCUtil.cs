using System.Net;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace CCUtil
{
    public class CCUtil
    {
        public static string GetResponse(HttpWebRequest req, bool GetLocation = false, bool GetRange = false, bool NeedResponse = true)
        {
            HttpWebResponse res = null;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException e)
            {
                StreamReader ereader = new StreamReader(e.Response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                string erespHTML = ereader.ReadToEnd();
                Console.WriteLine(erespHTML);
                throw new Exception(erespHTML);
            }
            if (GetLocation)
            {
                Console.WriteLine("Location: " + res.Headers["Location"]);
                return res.Headers["Location"];
            }
            if (GetRange && res.ContentLength == 0)
            {
                Console.WriteLine("Range: " + res.Headers["Range"]);
                return res.Headers["Range"];
            }
            if (NeedResponse)
            {
                StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                string respHTML = reader.ReadToEnd();
                res.Close();
                Console.WriteLine(respHTML);
                return respHTML;
            }
            else
            {
                return "";
            }
        }

        public static void RandomFile(int size, string fn)
        {
            StreamWriter sw = new StreamWriter(fn);
            sw.WriteLine("start");
            char c = 'a';
            for (int i = 0; i < size; i++)
            {
                sw.Write(c);
                c++;
                if (c > 'z') c = 'a';
            }
            sw.WriteLine("end");
            sw.WriteLine(DateTime.Now.ToString());
            sw.Close();
        }

        public static HttpWebRequest GenerateRequest(string URL, string Method, string token, bool KeepAlive = false, string ContentType = null, byte[] data = null, int offset = 0, int length = 0, string ContentRange = null, bool PreferAsync = false, int Timeout = 20 * 1000, string host = null)
        {
            Uri httpUrl = new Uri(URL);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpUrl);
            req.Timeout = Timeout;
            req.ReadWriteTimeout = Timeout;
            req.Method = Method;
            if (token != null) req.Headers.Add("Authorization", "Bearer " + token);
            if (host != null) req.Host = host;
            req.KeepAlive = KeepAlive;
            if (ContentType != null) req.ContentType = ContentType;
            if (ContentRange != null) req.Headers.Add("Content-Range", ContentRange);
            if (PreferAsync == true) req.Headers.Add("Prefer", "respond-async");
            if (data != null)
            {
                req.ContentLength = length;
                Stream stream = req.GetRequestStream();
                stream.Write(data, offset, length);
                stream.Close();
            }
            return req;
        }

        public static string HttpPost(string URL, string post, string token = "", bool PreferAsync = false, bool GetLocation = false, bool isJson = false, bool AllowAutoRedirect = true, bool NeedResponse = true, int Timeout = 20 * 1000)
        {
            byte[] data = Encoding.ASCII.GetBytes(post);
            HttpWebRequest req = GenerateRequest(URL, "POST", token, false, isJson ? "application/json" : "application/x-www-form-urlencoded", data, 0, data.Length, null, PreferAsync, Timeout);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req, GetLocation, false, NeedResponse);
        }

        public static string HttpPost(string URL, string token, byte[] data, int offset=0, int length = -1, bool NeedResponse = true, int Timeout = 20 * 1000, string host = null)
        {
            if (length == -1) length = data.Length;
            HttpWebRequest req = GenerateRequest(URL, "POST", token, false, "application/octet-stream", data, 0, data.Length, null, false, Timeout, host);
            return GetResponse(req, false, false, NeedResponse);
        }

        public static string HttpGet(string URL, string token, bool GetLocation = false, bool AllowAutoRedirect = true, bool NeedResponse = true, int Timeout = 20 * 1000)
        {
            HttpWebRequest req = GenerateRequest(URL, "GET", token, false, null, null, 0, 0, null, false, Timeout);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req, GetLocation, false, NeedResponse);
        }

        public static string HttpPut(string URL, string token, byte[] data, int offset = 0, int length = -1, string ContentRange = null, bool AllowAutoRedirect = true, bool GetRange = false, bool NeedResponse = true, int Timeout = 20 * 1000, string host = null)
        {
            if (length < 0) length = data.Length;
            HttpWebRequest req = GenerateRequest(URL, "PUT", token, true, "application/octet-stream", data, offset, length, ContentRange, false, Timeout, host);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req, false, GetRange, NeedResponse);
        }

        public static void HttpDelete(string url, string token)
        {
            HttpWebRequest req = GenerateRequest(url, "DELETE", token);
            GetResponse(req);
        }

        public static List<int> Ping(List<string> ips)
        {
            List<int> RTTs = new List<int>();
            Ping ping = new Ping();
            foreach (string ip in ips)
            {
                int rtt = -1;
                try
                {
                    PingReply reply = ping.Send(IPAddress.Parse(ip));
                    if (reply.Status == IPStatus.Success) rtt = (int)reply.RoundtripTime;
                    Console.WriteLine(ip + " " + rtt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                RTTs.Add(rtt);
            }
            return RTTs;
        }

        public static List<int> PingWithHttpGet(List<string> ips, int repeats = 4, int timeout = 2000)
        {
            List<int> RTTs = new List<int>();
            Stopwatch watch = new Stopwatch();
            foreach (string ip in ips)
            {
                int rtt = 0;
                for (int i = 0; i < repeats; i++)
                {
                    try
                    {
                        HttpWebRequest req = GenerateRequest("http://" + ip + "/", "GET", null);
                        req.Timeout = timeout;
                        watch.Restart();
                        try
                        {
                            req.GetResponse();
                        }
                        catch (WebException) { }
                        watch.Stop();
                        if (watch.ElapsedMilliseconds >= timeout - 10)
                        {
                            rtt = -repeats;
                            break;
                        }
                        rtt += (int)watch.ElapsedMilliseconds;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        rtt = -repeats;
                        break;
                    }
                }
                rtt /= repeats;
                Console.WriteLine(ip + " " + rtt);
                RTTs.Add(rtt);
            }
            return RTTs;
        }

        public static List<string> ReadLines(string file, int minlength = 0)
        {
            List<string> lines = new List<string>();
            StreamReader sr = new StreamReader(file);
            while (!sr.EndOfStream)
            {
                string ts = sr.ReadLine();
                if (ts.Length >= minlength) lines.Add(ts);
            }
            sr.Close();
            return lines;
        }
    }
}
