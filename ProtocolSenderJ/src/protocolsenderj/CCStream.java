package protocolsenderj;
import java.io.*;

public class CCStream {
    
    /**
     * 整型转换为4位字节数组
     * @param intValue
     * @return
     */
    public static byte[] int2Byte(int intValue) {
        byte[] b = new byte[4];
        for (int i = 0; i < 4; i++) {
            b[i] = (byte) (intValue >> 8 * (3 - i) & 0xFF);
        }
        return b;
    }

    /**
     * 4位字节数组转换为整型
     * @param b
     * @return
     */
    public static int byte2Int(byte[] b) {
        int intValue = 0;
        for (int i = 0; i < b.length; i++) {
            intValue += (b[i] & 0xFF) << (8 * (3 - i));
        }
        return intValue;
    }
    
    // Write an integer to stream
    public static void writeStream(OutputStream stream, int val) throws Exception
    {
        byte[] bytes = int2Byte(val);
        stream.write(bytes, 0, bytes.length);
        stream.flush();
    }

    // Read an intger from stream
    public static int readInt(InputStream stream) throws Exception
    {
        byte[] bytes = new byte[4];
        stream.read(bytes, 0, 4);
        return byte2Int(bytes);
    }
    
    // Write byte array to stream, can decide whether attach its length ahead
    public static void writeStream(OutputStream stream, byte[] data, boolean withLength, int len, boolean flush) throws Exception
    {
        if (len == -1) len = data.length;
        if (withLength) writeStream(stream, len);
        stream.write(data, 0, len);
        if (flush) stream.flush();
    }
    public static void writeStream(OutputStream stream, byte[] data, boolean withLength, int len) throws Exception
    {
        writeStream(stream,data,withLength,len,true);
    }
    public static void writeStream(OutputStream stream, byte[] data, boolean withLength) throws Exception
    {
        writeStream(stream,data,withLength,-1);
    }
    public static void writeStream(OutputStream stream, byte[] data) throws Exception
    {
        writeStream(stream,data,false);
    }
    
    // Read a byte array from stream with its length ahead
    public static byte[] readByte(InputStream stream) throws Exception
    {
        int metalen = readInt(stream);
        byte[] buffer = new byte[metalen];
        int pos = 0;
        while (pos < metalen)
        {
            int bytesRead = stream.read(buffer, pos, metalen-pos);
            pos += bytesRead;
        }
        return buffer;
    }
    
    // Write a string to stream with its length ahead
    public static void writeStream(OutputStream stream, String str) throws Exception
    {
        byte[] msg = str.getBytes();
        writeStream(stream, msg, true);
    }
    
    // Read string with its length ahead
    public static String readString(InputStream stream) throws Exception
    {
        int strlen = readInt(stream);
        byte[] buffer = new byte[strlen];
        int bytesRead = stream.read(buffer, 0, buffer.length);
        String str = new String(buffer).trim();
        return str;
    }
    
    // Send all remaining data in src to dest, with maximum block size, in special format
    public static void copyBlockSend(InputStream src, OutputStream dest, int buflen) throws Exception
    {
        byte[] buffer = new byte[buflen];
        while (true)
        {
            int bytesRead = src.read(buffer, 0, buflen);
            if (bytesRead <= 0) break;
            writeStream(dest, buffer, true, bytesRead, false);
        }
        writeStream(dest, -1);
        dest.flush();
    }
    public static void copyBlockSend(InputStream src, OutputStream dest) throws Exception
    {
        copyBlockSend(src,dest,256*1024);
    }

    // Read data from src to dest in special format
    public static void copyBlockReceive(InputStream src, OutputStream dest, int buflen) throws Exception
    {
        byte[] buffer = new byte[buflen];
        while (true)
        {
            int seglen = readInt(src);
            if (seglen == -1) break;
            while (seglen > 0)
            {
                int bytesRead = src.read(buffer, 0, Math.min(buflen, seglen));
                dest.write(buffer, 0, bytesRead);
                seglen -= bytesRead;
            }
        }
    }
    public static void copyBlockReceive(InputStream src, OutputStream dest) throws Exception
    {
        copyBlockReceive(src,dest,256*1024);
    }
    
    // Copy specified length of data from src to dest
    public static void copyStream(InputStream src, OutputStream dest, int len) throws Exception
    {
        int tot = 0;
        byte[] buffer = new byte[256 * 1024];
        while (tot < len)
        {
            int bytesRead = src.read(buffer, 0, Math.min(buffer.length, len - tot));
            dest.write(buffer, 0, bytesRead);
            tot += bytesRead;
        }
    }
}
