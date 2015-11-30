using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CCUtil;
using Util = CCUtil.CCUtil;
using System.Runtime.Serialization.Formatters.Binary;

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
                byte[] buffer = new byte[32];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string str = System.Text.Encoding.Default.GetString(buffer).Trim('\0');
                Console.WriteLine("--" + str);
                if (str == "CLOSE") break;
                else if (str == "METADATA")
                {
                    buffer = new byte[1024];
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    FileMetadata meta = (FileMetadata)formatter.Deserialize(new MemoryStream(buffer));

                }
            } while (true);
            stream.Close();
            client.Close();
            Console.WriteLine("Disconnected.");
        }
    }
}
