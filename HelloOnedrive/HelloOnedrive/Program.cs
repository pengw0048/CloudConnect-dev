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

        [DataContract]
        class DriveResource
        {

            [DataMember]
            public string id;

            [DataMember]
            public Owner owner; 

            [DataMember]
            public Quota quota;

        };

        [DataContract]
        class Owner
        {
            
            [DataMember]
            public UserInfo user;

        };

        [DataContract]
        class UserInfo
        {

            [DataMember]
            public string id;

            [DataMember]
            public string displayName;

        };

        [DataContract]
        class Quota
        {

            [DataMember]
            public long total;

            [DataMember]
            public long used;

            [DataMember]
            public long remaining;

        };

        [DataContract]
        class UploadSession
        {

            [DataMember]
            public string uploadUrl;

            [DataMember]
            public string expirationDateTime;

            [DataMember]
            public string[] nextExpectedRanges;

        };

        [DataContract]
        class AsyncOperationStatus
        {

            [DataMember]
            public string operation;

            [DataMember]
            public double percentageComplete;

            [DataMember]
            public string status;

        };

        static string GetResponse(HttpWebRequest req, bool GetLocation = false)
        {
            HttpWebResponse res = null;
            try {
                res = (HttpWebResponse)req.GetResponse();
            } catch (WebException e) {
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

        static HttpWebRequest GenerateRequest(string URL, string Method, string token, bool KeepAlive = false, string ContentType = null, byte[] data = null, int offset = 0, int length = 0, string ContentRange = null, bool PreferAsync = false)
        {
            Uri httpUrl = new Uri(URL);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpUrl);
            req.Method = Method;
            if(token != null) req.Headers.Add("Authorization", "Bearer " + token);
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

        static string HttpPost(string URL, string post, string token, bool PreferAsync = false, bool GetLocation = false, bool isJson = false, bool AllowAutoRedirect = true)
        {
            byte[] data = Encoding.ASCII.GetBytes(post);
            HttpWebRequest req = GenerateRequest(URL, "POST", token, false, isJson ? "application/json" : "application/x-www-form-urlencoded", data, 0, data.Length, null, PreferAsync);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req, GetLocation);
        }

        static string HttpGet(string URL, string token, bool GetLocation = false, bool AllowAutoRedirect = true)
        {
            HttpWebRequest req = GenerateRequest(URL, "GET", token);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req, GetLocation);
        }

        static string HttpPut(string URL, string token, byte[] data, int offset = 0, int length = -1, string ContentRange = null, bool AllowAutoRedirect = true)
        {
            if (length < 0) length = data.Length;
            HttpWebRequest req = GenerateRequest(URL, "PUT", token, true, "application/octet-stream", data, offset, length, ContentRange);
            if (AllowAutoRedirect == false) req.AllowAutoRedirect = false;
            return GetResponse(req);
        }

        static string GetToken(string refresh_token)
        {
            string respHTML = HttpPost("https://login.live.com/oauth20_token.srf", "client_id=" + clientid + "&redirect_uri=" + Uri.EscapeUriString( redirect_uri) + "&client_secret=" + secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token", null);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TokenResponse));
            TokenResponse personinfo = (TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return personinfo.access_token;
        }

        static void GetBasicInfo(string token)
        {
            string respHTML = HttpGet("https://api.onedrive.com/v1.0/drive",token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DriveResource));
            DriveResource driveinfo = (DriveResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(driveinfo.owner.user.displayName + " " + driveinfo.quota.remaining + "/" + driveinfo.quota.total);
        }

        static void SimpleUpload(string token, int size = 1024 * 1024)
        {
            RandomFile(size, "simple.txt");
            string respHTML = HttpPut("https://api.onedrive.com/v1.0/drive/root:/simple.txt:/content", token, File.ReadAllBytes("simple.txt"));
            File.Delete("simple.txt");
        }

        static void ChunkedUpload(string token,int size = 1024 * 1024, int parts = 10)
        {
            RandomFile(size, "chunked.txt");
            string respHTML = HttpPost("https://api.onedrive.com/v1.0/drive/root:/chunked.txt:/upload.createSession", "", token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(UploadSession));
            UploadSession task = (UploadSession)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));

            byte[] data = File.ReadAllBytes("chunked.txt");
            int offset = int.Parse(task.nextExpectedRanges[0].Split(new char[] { '-' })[0]);
            while (offset < data.Length)
            {
                int uplen = Math.Min(size / parts, data.Length - offset);
                respHTML = HttpPut(task.uploadUrl, token, data, offset, uplen,
                    "bytes " + offset + "-" + (offset + uplen - 1) + "/" + data.Length);
                if (offset + uplen < data.Length)
                {
                    UploadSession ttask = (UploadSession)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
                    offset = int.Parse(ttask.nextExpectedRanges[0].Split(new char[] { '-' })[0]);
                }
                else
                    offset = data.Length;
            }

            File.Delete("chunked.txt");
        }

        static string OfflineDownload(string url, string name, string token)
        {
            string location = HttpPost("https://api.onedrive.com/v1.0/drive/root/children",
                "{ \"@content.sourceUrl\": \"" + url + "\", \"name\": \"" + name + "\", \"file\": { } }"
                , token, true, true, true, false);
            return location;
        }

        static string QueryOffline(string job, string token)
        {
            string respHTML = HttpGet(job, token, false, false);
            if (respHTML.Length < 3)
            {
                return HttpGet(job, token, true);
            }
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AsyncOperationStatus));
            AsyncOperationStatus status = (AsyncOperationStatus)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return " " + status.percentageComplete + "% " + status.status;
        }

        static void Delete(string path, string token)
        {
            HttpWebRequest req = GenerateRequest("https://api.onedrive.com/v1.0/drive/root:/" + path, "DELETE", token);
            GetResponse(req);
        }

        static string GetDownloadLink(string token)
        {
            return HttpGet("https://api.onedrive.com/v1.0/drive/root:/simple.txt:/content", token, true, false);
        }

        static void Main(string[] args)
        {
            string token = GetToken(refresh_token);
            GetBasicInfo(token);
            SimpleUpload(token);
            Console.WriteLine(GetDownloadLink(token));
            ChunkedUpload(token);
            Delete("simple.txt", token);
            Delete("chunked.txt", token);
            string job = OfflineDownload("http://www.worldoftanks-wot.com/wp-content/uploads/wot-september-free.jpg", "1.jpg", token);
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                string ret = QueryOffline(job,token);
                Console.WriteLine(ret);
                if (ret.StartsWith(" ") == false) break;
            }
            Delete("1.jpg", token);
        }
    }
}
