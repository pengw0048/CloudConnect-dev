using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CCUtil;
using Util = CCUtil.CCUtil;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using librsync.net;

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
                Util.writeStream(stream, "META");
                FileMetadata meta = new FileMetadata(file);
                MemoryStream ms = new MemoryStream();
                formatter.Serialize(ms, meta);
                ms.Close();
                byte[] bytes = ms.ToArray();
                Util.writeStream(stream, bytes, true);
                string str = Util.readString(stream);
                Console.WriteLine("--" + str);
                if (str == "PASS")
                {
                    Console.WriteLine("File version up to date.");
                }
                else if (str == "NEWF")
                {
                    Console.WriteLine("Sending new file.");
                    FileStream fs = new FileStream(file.FullName, FileMode.Open);
                    fs.CopyTo(stream);
                    fs.Close();
                    stream.Flush();
                    Console.WriteLine("File sent.");
                }
                else if (str == "DIFF")
                {
                    Console.WriteLine("Calculating diff.");
                    byte[] signature = Util.readByte(stream);
                    var signatureStream = new MemoryStream(signature);
                    var fileStream = new FileStream(file.FullName, FileMode.Open);
                    var delta = Librsync.ComputeDelta(signatureStream, fileStream);
                    Console.WriteLine("Sending diff.");
                    Util.copyBlockSend(delta, stream);
                    delta.Close();
                    fileStream.Close();
                    signatureStream.Close();
                    Console.WriteLine("Diff sent.");
                }
            }
            Util.writeStream(stream, "EXIT");
            stream.Close();
            server.Close();
            Console.WriteLine("Disconnected.");
        }
    }
}
