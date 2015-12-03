using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CCUtil;
using Util = CCUtil.CCUtil;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using librsync.net;

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
                        var metaStream = new FileStream("Cache/" + meta.name + ".meta", FileMode.Open);
                        FileMetadata meta2 = (FileMetadata)formatter.Deserialize(metaStream);
                        metaStream.Close();
                        if (meta.hash != meta2.hash)
                        {
                            Util.writeStream(stream, "DIFF");
                            Console.WriteLine("Sending signature.");
                            var fileStream = new FileStream("Cache/" + meta.name, FileMode.Open);
                            var signatureStream = Librsync.ComputeSignature(fileStream);
                            var resultMem = new MemoryStream();
                            signatureStream.CopyTo(resultMem);
                            resultMem.Close();
                            signatureStream.Close();
                            fileStream.Close();
                            byte[] signature = resultMem.ToArray();
                            Util.writeStream(stream, signature, true);
                            Console.WriteLine("Receiving delta.");
                            var deltaStream = new FileStream("Cache/" + meta.name + ".delta", FileMode.Create);
                            Util.copyBlockReceive(stream, deltaStream);
                            Console.WriteLine("Applying delta.");
                            deltaStream.Seek(0, SeekOrigin.Begin);
                            fileStream = new FileStream("Cache/" + meta.name, FileMode.Open);
                            var newStream = new FileStream("Cache/" + meta.name + ".new", FileMode.Create);
                            var finalStream = Librsync.ApplyDelta(fileStream, deltaStream);
                            finalStream.CopyTo(newStream);
                            finalStream.Close();
                            newStream.Close();
                            fileStream.Close();
                            deltaStream.Close();
                            File.Delete("Cache/" + meta.name + ".diff");
                            File.Delete("Cache/" + meta.name);
                            File.Move("Cache/" + meta.name + ".new", "Cache/" + meta.name);
                            Console.WriteLine("Delta applied.");
                            metaStream = new FileStream("Cache/" + meta.name + ".meta", FileMode.Create);
                            formatter.Serialize(metaStream, meta);
                            metaStream.Close();
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
