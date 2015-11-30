using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util = CCUtil.CCUtil;

namespace ProtocolSender
{
    class ProtocolSender
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: ProtocolSender.exe IP Port");
                return;
            }
            TcpClient client = new TcpClient();
            try
            {
                client.Connect(args[0], int.Parse(args[1]));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("Server Connected! {0} --> {1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

            Console.ReadLine();
        }
    }
}
