using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CCUtil;
using Util = CCUtil.CCUtil;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProtocolSender
{
    class ProtocolSender
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("usage: ProtocolSender.exe IP Port Folder_to_Send");
                return;
            }
            TcpClient server = new TcpClient();
            try
            {
                server.Connect(args[0], int.Parse(args[1]));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Server Connected! {0} --> {1}", server.Client.LocalEndPoint, server.Client.RemoteEndPoint);
            NetworkStream stream = server.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            DirectoryInfo dir = new DirectoryInfo(args[2]);
            foreach (FileInfo file in dir.GetFiles())
            {
                Console.WriteLine(file.Name);
                Util.writeStream(stream, "METADATA");
                FileMetadata meta = new FileMetadata(file);
                formatter.Serialize(stream, meta);
                stream.Flush();

            }
            Util.writeStream(stream, "CLOSE");
            stream.Close();
            server.Close();
            Console.WriteLine("Disconnected.");
        }
    }
}
