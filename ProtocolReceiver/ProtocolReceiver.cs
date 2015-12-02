using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CCUtil;
using Util = CCUtil.CCUtil;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace ProtocolReceiver
{
    class ProtocolReceiver
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 1209);
            listener.Start();
            Console.WriteLine("Started listening on port 1209.");
            TcpClient client = listener.AcceptTcpClient();
            listener.Stop();
            Console.WriteLine("Client Connected! {0} <-- {1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
            NetworkStream stream = client.GetStream();
            BinaryFormatter formatter = new BinaryFormatter();
            do
            {
                string str = Util.readString(stream);
                Console.WriteLine("--" + str);
                if (str == "EXIT") break;
                else if (str == "META")
                {
                    byte[] buffer = Util.readByte(stream);
                    FileMetadata meta = (FileMetadata)formatter.Deserialize(new MemoryStream(buffer, 0, buffer.Length));
                    Console.WriteLine(meta.name);
                    if (File.Exists("Cache/" + meta.name + ".meta"))
                    {
                        FileMetadata meta2 = (FileMetadata)formatter.Deserialize(new FileStream("Cache/" + meta.name + ".meta", FileMode.Open));
                        if (meta.hash != meta2.hash)
                        {
                            Util.writeStream(stream, "DIFF");

                        }
                        else
                        {
                            Console.WriteLine("Up to date.");
                            Util.writeStream(stream, "PASS");
                        }
                    }
                    else
                    {
                        Console.WriteLine("New file.");
                        Util.writeStream(stream, "NEWF");
                        formatter.Serialize(new FileStream("Cache/" + meta.name + ".meta", FileMode.Create), meta);
                        FileStream fs = new FileStream("Cache/" + meta.name, FileMode.Create);
                        Util.copyStream(stream, fs, (int)meta.size);
                        Console.WriteLine("Transfer complete.");
                    }
                }
            } while (true);
            stream.Close();
            client.Close();
            Console.WriteLine("Disconnected.");
        }
    }
}
