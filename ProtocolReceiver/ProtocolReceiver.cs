using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CCUtil;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using librsync.net;

namespace ProtocolReceiver
{
    class ProtocolReceiver
    {
        private static BinaryFormatter formatter = new BinaryFormatter();

        static void Main(string[] args)
        {
            // Listen, accept connection, open stream
            TcpListener listener = new TcpListener(IPAddress.Any, 1209);
            listener.Start();
            Console.WriteLine("Started listening on port 1209.");
            TcpClient client = listener.AcceptTcpClient();
            listener.Stop();
            Console.WriteLine("Client Connected! {0} <-- {1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
            NetworkStream clientStream = client.GetStream();

            // Main loop
            do
            {
                string str = CCStream.readString(clientStream);
                Console.WriteLine("--" + str);

                // Message switch
                if (str == "EXIT") break;
                else if (str == "META")
                {
                    // Get meta from client
                    byte[] buffer = CCStream.readByte(clientStream);
                    FileMetadata newMeta;
                    using (var memoryStream = new MemoryStream(buffer, 0, buffer.Length))
                        newMeta = (FileMetadata)formatter.Deserialize(memoryStream);
                    Console.WriteLine(newMeta.name);

                    if (File.Exists("Cache/" + newMeta.name + ".meta"))
                    {
                        // Has older version
                        FileMetadata oldMeta;
                        using (var oldMetaStream = new FileStream("Cache/" + newMeta.name + ".meta", FileMode.Open))
                            oldMeta = (FileMetadata)formatter.Deserialize(oldMetaStream);

                        if (newMeta.hash != oldMeta.hash)
                        {
                            // This file has been updated
                            CCStream.writeStream(clientStream, "DIFF");
                            Console.WriteLine("Sending signature.");
                            using (var newMetaStream = new FileStream("Cache/" + newMeta.name, FileMode.Open))
                            using (var signatureStream = Librsync.ComputeSignature(newMetaStream))
                            using (var memoryStream = new MemoryStream())
                            {
                                signatureStream.CopyTo(memoryStream);
                                memoryStream.Close();
                                byte[] signature = memoryStream.ToArray();
                                CCStream.writeStream(clientStream, signature, true);
                            }

                            // Wait for delta from client
                            Console.WriteLine("Receiving delta.");
                            using (var deltaStream = new FileStream("Cache/" + newMeta.name + ".delta", FileMode.Create))
                                CCStream.copyBlockReceive(clientStream, deltaStream);

                            // Apply data and write to a temp file
                            Console.WriteLine("Applying delta.");
                            using (var deltaStream = new FileStream("Cache/" + newMeta.name + ".delta", FileMode.Open))
                            using (var oldStream = new FileStream("Cache/" + newMeta.name, FileMode.Open))
                            using (var newStream = new FileStream("Cache/" + newMeta.name + ".new", FileMode.Create))
                            using (var appliedStream = Librsync.ApplyDelta(oldStream, deltaStream))
                            {
                                appliedStream.CopyTo(newStream);
                                Console.WriteLine("Delta applied.");
                            }

                            // Clean up
                            File.Delete("Cache/" + newMeta.name + ".delta");
                            File.Delete("Cache/" + newMeta.name);
                            File.Move("Cache/" + newMeta.name + ".new", "Cache/" + newMeta.name);

                            // Save new meta
                            using (var metaStream = new FileStream("Cache/" + newMeta.name + ".meta", FileMode.Create))
                                formatter.Serialize(metaStream, newMeta);
                        }
                        else
                        {
                            // File metas match
                            Console.WriteLine("Up to date.");
                            CCStream.writeStream(clientStream, "PASS");
                        }
                    }
                    else
                    {
                        // Don't have meta = new file
                        Console.WriteLine("New file.");
                        CCStream.writeStream(clientStream, "NEWF");

                        // Save new meta
                        using (var newMetaStream = new FileStream("Cache/" + newMeta.name + ".meta", FileMode.Create))
                            formatter.Serialize(newMetaStream, newMeta);

                        // Receive and save new file
                        using (var fileStream = new FileStream("Cache/" + newMeta.name, FileMode.Create))
                            CCStream.copyStream(clientStream, fileStream, (int)newMeta.size);
                        Console.WriteLine("Transfer complete.");
                    }
                }
            } while (true);

            // Final cleanup
            clientStream.Close();
            client.Close();
            Console.WriteLine("Disconnected.");
        }
    }
}
