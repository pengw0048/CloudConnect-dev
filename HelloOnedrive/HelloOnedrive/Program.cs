using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace HelloOnedrive
{
    class Program
    {
        private static string clientid = "000000004816FA26";
        private static string secret = "UBVcd-hCr6JYFnldXyY5pn85T2rkUGMW";
        private static string refresh_token = "MCtAD7A4A9Z1zIRBj3Rv6zUGRgPJRkwwWNnThF3J07UxaDhvJncJXD8jHk5Be3sApsShMLUaonNtRbZBUMkh69Gsuf5gdDbir9JYKTEoe1ujGkznVlsNZncsEIhSc3IwMN*qXyoGHlmHwUuSFRjh5mtb3iDXfxOZHuCyAmwax4UqPNKlMvrQLDXrvcpMZuoRsnQt504!X*m7tgssfsbK0SsMduE5KfSpYLolJQ0YRZvmsFIm2dLP1sFPHOJofuIBOHDK3J5Dk6Skw*9w9rrSJx13OS27*PHNJQVrmarvK0jhfLCCNCbpGgHc43zmn2uFZRuGLiMVFVf8KXL*1QEykCxyghNfl3RazcDJQCzK8c!TOmPcBJxph3VhW9pVyGfOtNA$$";
        private static string redirect_uri = "http://stomakun.tk/echo.php";

        [DataContract]
        class TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

            [DataMember]
            public string refresh_token;

        };

        static string GetResponse(HttpWebRequest req)
        {
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream(), Encoding.GetEncoding("utf-8"));
            string respHTML = reader.ReadToEnd();
            res.Close();
            Console.WriteLine(respHTML);
            return respHTML;
        }

        static void RandomFile(int size, string fn)
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

        static HttpWebRequest GenerateRequest(string URL, string Method, string token, bool KeepAlive = false, string ContentType = null, byte[] data = null, int offset = 0, int length = 0)
        {
            Uri httpUrl = new Uri(URL);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpUrl);
            req.Method = Method;
            if(token != null) req.Headers.Add("Authorization", "Bearer " + token);
            req.KeepAlive = KeepAlive;
            if (ContentType != null) req.ContentType = ContentType;
            if (data != null)
            {
                req.ContentLength = length;
                Stream stream = req.GetRequestStream();
                stream.Write(data, offset, length);
                stream.Close();
            }
            return req;
        }

        static string HttpPost(string URL, string post, string token)
        {
            byte[] data = Encoding.ASCII.GetBytes(post);
            HttpWebRequest req = GenerateRequest(URL, "POST", token, false, "application/x-www-form-urlencoded", data, 0, data.Length);
            return GetResponse(req);
        }

        static string HttpGet(string URL, string token)
        {
            HttpWebRequest req = GenerateRequest(URL, "GET", token);
            return GetResponse(req);
        }

        static string HttpPut(string URL, string token, byte[] data, int offset = 0, int length = -1)
        {
            if (length < 0) length = data.Length;
            HttpWebRequest req = GenerateRequest(URL, "PUT", token, true, "application/octet-stream", data, offset, length);
            return GetResponse(req);
        }

        static string GetToken(string refresh_token)
        {
            string respHTML = HttpPost("https://login.live.com/oauth20_token.srf", "client_id=" + clientid + "&redirect_uri=" + Uri.EscapeUriString( redirect_uri) + "&client_secret=" + secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token", null);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TokenResponse));
            TokenResponse personinfo = (TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return personinfo.access_token;
        }

        static void Main(string[] args)
        {
            string token = GetToken(refresh_token);

        }
    }
}
