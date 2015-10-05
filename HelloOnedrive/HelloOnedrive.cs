using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System;
using Util = CCUtil.CCUtil;

namespace HelloOnedrive
{
    class HelloOnedrive
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

        static string GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://login.live.com/oauth20_token.srf", "client_id=" + clientid + "&redirect_uri=" + Uri.EscapeUriString( redirect_uri) + "&client_secret=" + secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TokenResponse));
            TokenResponse personinfo = (TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return personinfo.access_token;
        }

        static void GetBasicInfo(string token)
        {
            string respHTML = Util.HttpGet("https://api.onedrive.com/v1.0/drive",token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DriveResource));
            DriveResource driveinfo = (DriveResource)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            Console.WriteLine(driveinfo.owner.user.displayName + " " + driveinfo.quota.remaining + "/" + driveinfo.quota.total);
        }

        static void SimpleUpload(string token, int size = 1024 * 1024)
        {
            Util.RandomFile(size, "simple.txt");
            string respHTML = Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/simple.txt:/content", token, File.ReadAllBytes("simple.txt"));
            File.Delete("simple.txt");
        }

        static void ChunkedUpload(string token,int size = 1024 * 1024, int parts = 10)
        {
            Util.RandomFile(size, "chunked.txt");
            string respHTML = Util.HttpPost("https://api.onedrive.com/v1.0/drive/root:/chunked.txt:/upload.createSession", "", token);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(UploadSession));
            UploadSession task = (UploadSession)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));

            byte[] data = File.ReadAllBytes("chunked.txt");
            int offset = int.Parse(task.nextExpectedRanges[0].Split(new char[] { '-' })[0]);
            while (offset < data.Length)
            {
                int uplen = Math.Min(size / parts, data.Length - offset);
                respHTML = Util.HttpPut(task.uploadUrl, token, data, offset, uplen,
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
            string location = Util.HttpPost("https://api.onedrive.com/v1.0/drive/root/children",
                "{ \"@content.sourceUrl\": \"" + url + "\", \"name\": \"" + name + "\", \"file\": { } }"
                , token, true, true, true, false);
            return location;
        }

        static string QueryOffline(string job, string token)
        {
            string respHTML = Util.HttpGet(job, token, false, false);
            if (respHTML.Length < 3)
            {
                return Util.HttpGet(job, token, true);
            }
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AsyncOperationStatus));
            AsyncOperationStatus status = (AsyncOperationStatus)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return " " + status.percentageComplete + "% " + status.status;
        }

        static void Delete(string path, string token)
        {
            Util.HttpDelete("https://api.onedrive.com/v1.0/drive/root:/" + path, token);
        }

        static string GetDownloadLink(string token)
        {
            return Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/simple.txt:/content", token, true, false);
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
