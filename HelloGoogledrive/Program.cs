using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System;
using Util = CCUtil.CCUtil;

namespace HelloGoogledrive
{
    class Program
    {

        private static string refresh_token = "1/8N6Yf_f-7KH0ms0fNH01zA4DRajTDradB6IxKMLU2RRIgOrJDtdun6zK6XiATCKT";
        private static string client_id = "920585866822-d71k4q781qqr4rhc17jkicjdbdcn9b9d.apps.googleusercontent.com";
        private static string client_secret = "_9rOg4xXuBQ8vDQblIcv4uZ5";

        [DataContract]
        class TokenResponse
        {

            [DataMember]
            public long expires_in;

            [DataMember]
            public string access_token;

        };

        static string GetToken(string refresh_token)
        {
            string respHTML = Util.HttpPost("https://www.googleapis.com/oauth2/v3/token", "client_id=" + client_id + "&client_secret=" + client_secret + "&refresh_token=" + refresh_token + "&grant_type=refresh_token", null);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(TokenResponse));
            TokenResponse token = (TokenResponse)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
            return token.access_token;
        }

        static void Main(string[] args)
        {
            string token = GetToken(refresh_token);

        }
    }
}
