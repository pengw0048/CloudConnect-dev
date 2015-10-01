using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;
using System;

class Module1
{

    private static string token= "K4g_u85PlIIAAAAAAAAAFKg_ygQtYqu0uqH15MBMzEs4YDnqmJ61U65tzdC-l6C_";

    [DataContract]
    class Person
    {

        [DataMember]
        public string display_name;

        [DataMember]
        public string email;

        [DataMember]
        public QuotaInfo quota_info;
    };

    [DataContract]
    class QuotaInfo
    {

        [DataMember]
        public long quota;

        [DataMember]
        public long normal;
    };

    [DataContract]
    class DownloadLink
    {

        [DataMember]
        public string url;

        [DataMember]
        public string expires;
    };

    [DataContract]
    class ChunkedUploadTask
    {

        [DataMember]
        public string upload_id;

        [DataMember]
        public long offset;

        [DataMember]
        public string expires;
    };

    [DataContract]
    class OfflineTask
    {

        [DataMember]
        public string status;

        [DataMember]
        public string job;
    };

    [DataContract]
    class OfflineStatus
    {

        [DataMember]
        public string status;

        [DataMember]
        public string error;
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

    static void RandomFile(int size,string fn)
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

    static HttpWebRequest GenerateRequest(string URL, string Method, string token, bool KeepAlive=false,string ContentType=null,byte[] data=null,int offset=0,int length=0)
    {
        Uri httpUrl = new Uri(URL);
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(httpUrl);
        req.Method = Method;
        if (token != null) req.Headers.Add("Authorization", "Bearer " + token);
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

    static string HttpPut(string URL, string token, byte[] data, int offset=0,int length=-1)
    {
        if (length < 0) length = data.Length;
        HttpWebRequest req = GenerateRequest(URL, "PUT", token, true, "application/octet-stream", data, offset, length);
        return GetResponse(req);
    }

    static void GetAccountInfo()
    {
        string respHTML = HttpGet("https://api.dropboxapi.com/1/account/info", token);
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Person));
        Person personinfo = (Person)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
        Console.WriteLine(personinfo.display_name + " "+ personinfo.email + " "+ personinfo.quota_info.normal + "/" + personinfo.quota_info.quota);
    }

    static void SimpleUpload(int size = 1024*1024)
    {
        RandomFile(size, "simple.txt");
        string respHTML = HttpPut("https://content.dropboxapi.com/1/files_put/auto/simple.txt?", token, File.ReadAllBytes("simple.txt"));
        File.Delete("simple.txt");
    }

    static void GetDownloadLink()
    {
        string respHTML = HttpPost("https://api.dropboxapi.com/1/media/auto/simple.txt", "", token);
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DownloadLink));
        DownloadLink link = (DownloadLink)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
        Console.WriteLine(link.url);
    }

    static void ChunkedUpload(int size = 1024 * 1024, int parts = 10)
    {
        RandomFile(size, "chunked.txt");
        string respHTML = HttpPut("https://content.dropboxapi.com/1/chunked_upload?", token, new byte[0]);
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ChunkedUploadTask));
        ChunkedUploadTask task = (ChunkedUploadTask)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));

        byte[] data = File.ReadAllBytes("chunked.txt");
        while (task.offset < data.Length)
        {
            int uplen = Math.Min(size / parts, data.Length - (int)task.offset);
            respHTML = HttpPut("https://content.dropboxapi.com/1/chunked_upload?upload_id=" + task.upload_id + "&offset=" + task.offset,
                token,data, (int)task.offset, uplen);
            task = (ChunkedUploadTask)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
        }

        respHTML = HttpPost("https://content.dropboxapi.com/1/commit_chunked_upload/auto/chunked.txt?", "upload_id=" + task.upload_id, token);
        File.Delete("chunked.txt");
    }

    static string OfflineDownload(string url,string path)
    {
        string respHTML = HttpPost("https://api.dropboxapi.com/1/save_url/auto/" + path + "?", "url=" + url, token);
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(OfflineTask));
        OfflineTask link = (OfflineTask)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
        return link.job;
    }

    static string QueryOffline(string job)
    {
        string respHTML = HttpGet("https://api.dropboxapi.com/1/save_url_job/" + job, token);
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(OfflineStatus));
        OfflineStatus status = (OfflineStatus)ser.ReadObject(new MemoryStream(Encoding.ASCII.GetBytes(respHTML)));
        return status.status;
    }

    static void Delete(string path)
    {
        string respHTML = HttpPost("https://api.dropboxapi.com/1/fileops/delete?", "root=auto&path=" + path, token);
    }

    static void Main(string[] args)
    {
        GetAccountInfo();
        SimpleUpload();
        GetDownloadLink();
        ChunkedUpload();
        string job=OfflineDownload("http://down.sandai.net/thunder7/Thunder_dl_7.9.40.5006.exe","thunder.exe");
        while (true)
        {
            System.Threading.Thread.Sleep(1000);
            string ret = QueryOffline(job);
            Console.WriteLine(ret);
            if (ret == "COMPLETE") break;
        }
        Delete("thunder.exe");
        Delete("simple.txt");
        Delete("chunked.txt");
    }
}