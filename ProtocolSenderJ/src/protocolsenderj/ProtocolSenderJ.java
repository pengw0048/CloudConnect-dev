package protocolsenderj;
import ccutil.*;
import java.io.*;
import java.net.*;
import java.util.*;

public class ProtocolSenderJ {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        InputStream is=null;
        OutputStream os=null;
        BufferedInputStream bis=null;
        BufferedOutputStream bos=null;
        Socket socket=null;
        try{
            // Establish connection to server and get stream
            socket=new Socket(args[0],1209);
            System.out.println("Connected: "+socket.toString());
            is=socket.getInputStream();
            os=socket.getOutputStream();
            bis=new BufferedInputStream(is);
            bos=new BufferedOutputStream(os);
            
            // Enumerate files
            File[] files=new File(args[1]).listFiles();
            for(File file:files){
                // Send meta
                System.out.println(file.getName());
                CCStream.writeStream(bos, "META");
                FileMetadata meta = new FileMetadata(file);
                CCStream.writeObject(bos, meta);
                
                // Switch server response
                String str = CCStream.readString(bis);
                System.out.println("--" + str);
                if (str.equals("PASS")){
                    // Server has the newest version
                    System.out.println("File version up to date.");
                }else if (str.equals("NEWF")){
                    // This is a new file
                    System.out.println("Sending new file.");
                    FileInputStream fis=new FileInputStream(file.getAbsoluteFile());
                    CCStream.copyStream(fis, bos, (int)file.length());
                    bos.flush();
                    fis.close();
                    System.out.println("File sent.");
                }else if (str.equals("DIFF")){
                    System.out.println("Receiving signature.");
                    Vector signature=(Vector)CCStream.readObject(bis);
                    
                    System.out.println("Sending delta.");
                    FileInputStream fis=new FileInputStream(file.getAbsoluteFile());
                    BufferedInputStream bis2=new BufferedInputStream(fis);
                    SSync.sendDelta(bos, bis2, signature);
                    bis2.close();
                    fis.close();
                    System.out.println("Delta sent.");
                }
            }
            CCStream.writeStream(bos, "EXIT");
        }catch(Exception e){
            System.out.println(e);
        }finally{
            try{
                bos.close();
                bis.close();
                os.close();
                is.close();
                socket.close();
            }catch(Exception e){
            }
        }
        System.out.println("Disconnected.");
    }
    
}
