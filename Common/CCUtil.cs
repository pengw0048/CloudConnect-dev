using System.Net;
using System.IO;
using System.Text;
using System;

namespace CCUtil
{
    public class CCUtil
    {
        public static string GetResponse(HttpWebRequest req, bool GetLocation = false)
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
            StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            string respHTML = reader.ReadToEnd();
            res.Close();
            Console.WriteLine(respHTML);
            return respHTML;
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

        public static HttpWebRequest GenerateRequest(string URL, string Method, string token, bool KeepAlive = false, string ContentType = null, byte[] data = null, int offset = 0, int length = 0, string ContentRange = null, bool PreferAsync = false)
        {
            Uri httpUrl = new Uri(URL);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpUrl);
            req.Method = Method;
            if (token != null) req.Headers.Add("Authorization", "Bearer " + token);
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

        public static string HttpPost(string URL, string post, string token, bool PreferAsync = false, bool GetLocation = false, bool isJson = false, bool AllowAutoRedirect = true)
        {
            byte[] data = Encoding.ASCII.GetBytes(post);
            HttpWebRequest req = GenerateRequest(URL, "POST", token, false, isJson ? "application/json" : "application/x-www-form-urlencoded", data, 0, data.Length, null, PreferAsync);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req, GetLocation);
        }

        public static string HttpGet(string URL, string token, bool GetLocation = false, bool AllowAutoRedirect = true)
        {
            HttpWebRequest req = GenerateRequest(URL, "GET", token);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req, GetLocation);
        }

        public static string HttpPut(string URL, string token, byte[] data, int offset = 0, int length = -1, string ContentRange = null, bool AllowAutoRedirect = true)
        {
            if (length < 0) length = data.Length;
            HttpWebRequest req = GenerateRequest(URL, "PUT", token, true, "application/octet-stream", data, offset, length, ContentRange);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req);
        }

        public static void HttpDelete(string url, string token)
        {
            HttpWebRequest req = GenerateRequest(url, "DELETE", token);
            GetResponse(req);
        }
    }
}
