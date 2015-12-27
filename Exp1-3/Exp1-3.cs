using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Util = CCUtil.CCUtil;

namespace Exp1_3
{
    class Exp1_3
    {
        private static string d_token = "K4g_u85PlIIAAAAAAAAAFKg_ygQtYqu0uqH15MBMzEs4YDnqmJ61U65tzdC-l6C_";

        private static string o_clientid = "000000004816FA26";
        private static string o_secret = "UBVcd-hCr6JYFnldXyY5pn85T2rkUGMW";
        private static string o_refresh_token = "MCtAD7A4A9Z1zIRBj3Rv6zUGRgPJRkwwWNnThF3J07UxaDhvJncJXD8jHk5Be3sApsShMLUaonNtRbZBUMkh69Gsuf5gdDbir9JYKTEoe1ujGkznVlsNZncsEIhSc3IwMN*qXyoGHlmHwUuSFRjh5mtb3iDXfxOZHuCyAmwax4UqPNKlMvrQLDXrvcpMZuoRsnQt504!X*m7tgssfsbK0SsMduE5KfSpYLolJQ0YRZvmsFIm2dLP1sFPHOJofuIBOHDK3J5Dk6Skw*9w9rrSJx13OS27*PHNJQVrmarvK0jhfLCCNCbpGgHc43zmn2uFZRuGLiMVFVf8KXL*1QEykCxyghNfl3RazcDJQCzK8c!TOmPcBJxph3VhW9pVyGfOtNA$$";
        private static string o_redirect_uri = "http://stomakun.tk/echo.php";

        [DataContract]
        class o_TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

            [DataMember]
            public string refresh_token;

        };

        static string o_GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://login.live.com/oauth20_token.srf", "client_id=" + o_clientid + "&redirect_uri=" + Uri.EscapeUriString(o_redirect_uri) + "&client_secret=" + o_secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token");
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(o_TokenResponse));
            o_TokenResponse personinfo = (o_TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return personinfo.access_token;
        }

        static string o_GetDownloadLink(string token, string file)
        {
            return Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/" + file + ":/content", token, true, false);
        }

        static void Main(string[] args)
        {
            byte[] data = new byte[10 * 1024 * 1024];
            Console.WriteLine("Upload 10KB Dropbox");
            try
            {
                Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/10KB", d_token, data, 0, 10 * 1024, null, true, false, false, 5 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Download 10KB Dropbox");
            try
            {
                Util.HttpGet("https://content.dropboxapi.com/1/files/auto/10KB", d_token, false, true, true, 5 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Upload 1MB Dropbox");
            try
            {
                Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/1MB", d_token, data, 0, 1024 * 1024, null, true, false, false, 6 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Download 1MB Dropbox");
            try
            {
                Util.HttpGet("https://content.dropboxapi.com/1/files/auto/1MB", d_token, false, true, true, 6 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Upload 10MB Dropbox");
            try
            {
                Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/10MB", d_token, data, 0, 10 * 1024 * 1024, null, true, false, false, 8 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Download 10MB Dropbox");
            try
            {
                Util.HttpGet("https://content.dropboxapi.com/1/files/auto/10MB", d_token, false, true, true, 8 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Upload 100*10KB Dropbox");
            for (int i = 0; i < 100; i++)
                try
                {
                    Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/10KB", d_token, data, 0, 10 * 1024, null, true, false, false, 5 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            Console.WriteLine("Download 100*10KB Dropbox");
            for (int i = 0; i < 100; i++)
                try
                {
                    Util.HttpGet("https://content.dropboxapi.com/1/files/auto/10KB", d_token, false, true, true, 5 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            Console.WriteLine("Upload 10*1MB Dropbox");
            for (int i = 0; i < 10; i++)
                try
                {
                    Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/1MB", d_token, data, 0, 1024 * 1024, null, true, false, false, 8 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            Console.WriteLine("Download 10*1MB Dropbox");
            for (int i = 0; i < 10; i++)
                try
                {
                    Util.HttpGet("https://content.dropboxapi.com/1/files/auto/1MB", d_token, false, true, true, 8 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            string o_token = o_GetToken(o_refresh_token);
            Console.WriteLine("Upload 10KB Onedrive");
            try
            {
                Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/10KB:/content", o_token, data, 0, 10 * 1024, null, false, false, false, 6 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Download 10KB OneDrive");
            try
            {
                Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/10KB:/content", o_token, Timeout: 6 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Upload 1MB Onedrive");
            try
            {
                Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/1MB:/content", o_token, data, 0, 1024 * 1024, null, false, false, false, 8 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Download 1MB OneDrive");
            try
            {
                Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/1MB:/content", o_token, Timeout: 8 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Upload 10MB Onedrive");
            try
            {
                Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/10MB:/content", o_token, data, 0, 10 * 1024 * 1024, null, false, false, false, 8 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Download 10MB OneDrive");
            try
            {
                Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/10MB:/content", o_token, Timeout: 8 * 1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();

            Console.WriteLine("Upload 100*10KB Onedrive");
            for (int i = 0; i < 100; i++)
                try
                {
                    Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/10KB:/content", o_token, data, 0, 10 * 1024, null, false, false, false, 6 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            Console.WriteLine("Download 10KB OneDrive");
            for (int i = 0; i < 100; i++)
                try
                {
                    Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/10KB:/content", o_token, Timeout: 6 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            Console.WriteLine("Upload 10*1MB Onedrive");
            for (int i = 0; i < 10; i++)
                try
                {
                    Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/1MB:/content", o_token, data, 0, 1024 * 1024, null, false, false, false, 8 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            Console.WriteLine("Download 10*1MB OneDrive");
            for (int i = 0; i < 10; i++)
                try
                {
                    Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/1MB:/content", o_token, Timeout: 8 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    i--;
                }
            Console.ReadLine();

            Console.WriteLine("Completed.");
        }
    }
}
