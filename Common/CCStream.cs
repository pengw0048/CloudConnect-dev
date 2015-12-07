using System;
using System.IO;
using System.Text;

namespace CCUtil
{
    public class CCStream
    {

        // Write a string to stream with its length ahead
        public static void writeStream(Stream stream, string str)
        {
            byte[] msg = Encoding.Default.GetBytes(str);
            writeStream(stream, msg, true);
        }

        // Write byte array to stream, can decide whether attach its length ahead
        public static void writeStream(Stream stream, byte[] data, bool withLength = false, int len = -1, bool flush = true)
        {
            if (len == -1) len = data.Length;
            if (withLength) writeStream(stream, len);
            stream.Write(data, 0, len);
            if (flush) stream.Flush();
        }

        // Read string with its length ahead
        public static string readString(Stream stream)
        {
            int strlen = readInt(stream);
            byte[] buffer = new byte[strlen];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string str = Encoding.Default.GetString(buffer).Trim('\0');
            return str;
        }

        // Write an integer to stream
        public static void writeStream(Stream stream, int val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        // Read an intger from stream
        public static int readInt(Stream stream)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        // Read a byte array from stream with its length ahead
        public static byte[] readByte(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            int metalen = readInt(stream);
            copyStream(stream, ms, metalen);
            ms.Close();
            return ms.ToArray();
        }

        // Copy specified length of data from src to dest
        public static void copyStream(Stream src, Stream dest, int len)
        {
            int tot = 0;
            byte[] buffer = new byte[256 * 1024];
            while (tot < len)
            {
                int bytesRead = src.Read(buffer, 0, Math.Min(buffer.Length, len - tot));
                dest.Write(buffer, 0, bytesRead);
                tot += bytesRead;
            }
        }

        // Send all remaining data in src to dest, with maximum block size, in special format
        public static void copyBlockSend(Stream src, Stream dest, int buflen = 256 * 1024)
        {
            byte[] buffer = new byte[buflen];
            while (true)
            {
                int bytesRead = src.Read(buffer, 0, buflen);
                if (bytesRead <= 0) break;
                writeStream(dest, buffer, true, bytesRead, false);
            }
            writeStream(dest, -1);
            dest.Flush();
        }

        // Read data from src to dest in special format
        public static void copyBlockReceive(Stream src, Stream dest, int buflen = 256 * 1024)
        {
            byte[] buffer = new byte[buflen];
            while (true)
            {
                int seglen = readInt(src);
                if (seglen == -1) break;
                while (seglen > 0)
                {
                    int bytesRead = src.Read(buffer, 0, Math.Min(buflen, seglen));
                    dest.Write(buffer, 0, bytesRead);
                    seglen -= bytesRead;
                }
            }
        }
    }
}
