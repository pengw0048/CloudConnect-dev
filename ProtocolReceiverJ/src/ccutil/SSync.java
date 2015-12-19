package ccutil;
import java.io.*;;
import java.util.*;

public class SSync {
    public static Vector getSignature(OutputStream os, InputStream is, int block_size) throws Exception{
        Vector vec=new Vector();
        vec.add(block_size);
        byte[] buf=new byte[block_size];
        Checksum32 cs=new Checksum32();
        while(true){
            int bytesRead=is.read(buf);
            if(bytesRead<0)break;
            cs.check(buf, 0, bytesRead);
            vec.add(cs.getValue());
            vec.add(Crypto.MD5(buf));
        }
        return vec;
    }
    public static void sendDelta(OutputStream os, InputStream is, Vector vec) throws Exception{
        int block_size=(int)vec.get(0);
        CCStream.writeStream(os,block_size);
        HashMap<Integer,Integer> dict=new HashMap<>();
        for(int i=1;i<vec.size();i+=2){
            dict.put((Integer)vec.get(i),(Integer)(i/2));
        }
        Checksum32 cs=new Checksum32();
        byte[] buf=new byte[block_size];
        byte[] deltaBlock=new byte[64*1024];
        int dbpos=0;
        int bytesRead=0;

        boolean nextBlock=true;
        while(true) {
            byte lastByte = 0;
            if (nextBlock) {
                bytesRead = is.read(buf);
                if (bytesRead <= 0) break;
                cs.check(buf, 0, bytesRead);
            } else {
                int byteRead = is.read();
                if (byteRead < 0) break;
                lastByte = cs.block[cs.k];
                cs.roll((byte) byteRead);
            }
            boolean blockMatch = false;
            if (dict.containsKey(cs.getValue())) {
                int pos = dict.get(cs.getValue());
                if (vec.get(pos + 1).equals(Crypto.MD5(cs.getBlock()))) {
                    blockMatch = true;
                    if (dbpos > 0) {
                        CCStream.writeStream(os, dbpos, false);
                        os.write(deltaBlock);
                        dbpos = 0;
                    }
                    CCStream.writeStream(os, -pos, false);
                    nextBlock = true;
                }
            }
            if (!blockMatch){
                if (!nextBlock) {
                    deltaBlock[dbpos++] = lastByte;
                    if (dbpos == deltaBlock.length) {
                        CCStream.writeStream(os, dbpos, false);
                        os.write(deltaBlock);
                        dbpos = 0;
                    }
                } else {
                    nextBlock = false;
                }
            }
        }
        if(dbpos>0){
            CCStream.writeStream(os, dbpos, false);
            os.write(deltaBlock);
        }else{
            CCStream.writeStream(os, bytesRead, false);
            os.write(buf, 0, bytesRead);
        }
        CCStream.writeStream(os, 0x7fffffff);
    }
    public static void applyDelta(RandomAccessFile oldf, InputStream delta, OutputStream newf) throws Exception {
        int block_size=CCStream.readInt(delta);
        byte[] buf=new byte[block_size];
        while(true){
            int op=CCStream.readInt(delta);
            if(op==0x7fffffff)break;
            else if(op>0){
                int bytesRead=delta.read(buf,0,op);
                newf.write(buf,0,bytesRead);
            }else{
                oldf.seek(-op*block_size);
                oldf.read(buf);
                newf.write(buf);
            }
        }
    }
}
