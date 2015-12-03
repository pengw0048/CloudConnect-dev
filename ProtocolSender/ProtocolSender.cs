using System;
using System.IO;
using System.Net.Sockets;
using CCUtil;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using librsync.net;

namespace ProtocolSender
{
    class ProtocolSender
    {
        private static BinaryFormatter formatter = new BinaryFormatter();

        static void Main(string[] args)
        {
            // Check startup arguments
            if (args.Length != 3)
            {
                Console.WriteLine("usage: ProtocolSender.exe IP Port Folder_to_Send");
                return;
            }

            // Establish connection to server and get stream
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
            NetworkStream serverStream = server.GetStream();

            // Enumerate files
            DirectoryInfo dir = new DirectoryInfo(args[2]);
            foreach (FileInfo file in dir.GetFiles())
            {
                // Send meta
                Console.WriteLine(file.Name);
                CCStream.writeStream(serverStream, "META");
                FileMetadata meta = new FileMetadata(file);
                using (var memoryStream = new MemoryStream())
                {
                    formatter.Serialize(memoryStream, meta);
                    memoryStream.Close();
                    byte[] bytes = memoryStream.ToArray();
                    CCStream.writeStream(serverStream, bytes, true);
                }

                // Switch server response
                string str = CCStream.readString(serverStream);
                Console.WriteLine("--" + str);
                if (str == "PASS")
                {
                    // Server has the newest version
                    Console.WriteLine("File version up to date.");
                }
                else if (str == "NEWF")
                {
                    // This is a new file
                    Console.WriteLine("Sending new file.");
                    using (var fileStream = new FileStream(file.FullName, FileMode.Open))
                        fileStream.CopyTo(serverStream);
                    serverStream.Flush();
                    Console.WriteLine("File sent.");
                }
                else if (str == "DIFF")
                {
                    // Require diff
                    Console.WriteLine("Receiving signature.");
                    byte[] signature = CCStream.readByte(serverStream);

                    Console.WriteLine("Sending delta.");
                    using (var signatureStream = new MemoryStream(signature))
                    using (var fileStream = new FileStream(file.FullName, FileMode.Open))
                    using (var deltaStream = Librsync.ComputeDelta(signatureStream, fileStream))
                        CCStream.copyBlockSend(deltaStream, serverStream);
                    Console.WriteLine("Delta sent.");
                }
            }

            // Final cleanup
            CCStream.writeStream(serverStream, "EXIT");
            serverStream.Close();
            server.Close();
            Console.WriteLine("Disconnected.");
        }
    }
}
