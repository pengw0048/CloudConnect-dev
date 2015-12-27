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

namespace Exp1_1
{
    class Exp1_1
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
            var watch = new Stopwatch();
            byte[] data = new byte[10 * 1024 * 1024];
            using (var sw = new StreamWriter("exp1-1/dropbox" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt"))
            {
                sw.WriteLine(DateTime.Now.ToString());
                Console.WriteLine("Upload 10M Dropbox");
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://content.dropboxapi.com/1/files_put/auto/10M", d_token, data, 0, 10 * 1024 * 1024, null, true, false, false, 8 * 1000);
                    watch.Stop();
                    sw.WriteLine(watch.ElapsedMilliseconds);
                    Console.WriteLine(watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine("-1");
                    Console.WriteLine("-1");
                }

                Console.WriteLine("Download 10M Dropbox");
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://content.dropboxapi.com/1/files/auto/10M", d_token, false, true, true, 8 * 1000);
                    watch.Stop();
                    sw.WriteLine(watch.ElapsedMilliseconds);
                    Console.WriteLine(watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine("-1");
                    Console.WriteLine("-1");
                }
            }
            using (var sw = new StreamWriter("exp1-1/onedrive" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".txt"))
            {
                sw.WriteLine(DateTime.Now.ToString());
                string o_token = o_GetToken(o_refresh_token);
                Console.WriteLine("Upload 10M Onedrive");
                try
                {
                    watch.Restart();
                    Util.HttpPut("https://api.onedrive.com/v1.0/drive/root:/10M:/content", o_token, data, 0, 10 * 1024 * 1024, null, false, false, false, 10 * 1000);
                    watch.Stop();
                    sw.WriteLine(watch.ElapsedMilliseconds);
                    Console.WriteLine(watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine("-1");
                    Console.WriteLine("-1");
                }

                Console.WriteLine("Download 10M OneDrive");
                try
                {
                    watch.Restart();
                    Util.HttpGet("https://api.onedrive.com/v1.0/drive/root:/10M:/content", o_token, Timeout: 10 * 1000);
                    watch.Stop();
                    sw.WriteLine(watch.ElapsedMilliseconds);
                    Console.WriteLine(watch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    sw.WriteLine("-1");
                    Console.WriteLine("-1");
                }

                Console.WriteLine("Completed.");
            }
        }
    }
}
