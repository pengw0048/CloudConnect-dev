package protocolsenderj;
import java.io.*;
import java.net.*;

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
            socket=new Socket("127.0.0.1",1209);
            System.out.println("Connected: "+socket.toString());
            is=socket.getInputStream();
            os=socket.getOutputStream();
            bis=new BufferedInputStream(is);
            bos=new BufferedOutputStream(os);
            
            // Enumerate files
            File[] files=new File("Z:\\1").listFiles();
            for(File file:files){
                // Send meta
                System.out.println(file.getName());
                CCStream.writeStream(bos, "META");
                FileMetadata meta = new FileMetadata(file);
                ByteArrayOutputStream baos=new ByteArrayOutputStream();
                ObjectOutputStream oos=new ObjectOutputStream(baos);
                oos.writeObject(meta);
                oos.close();
                byte[] bytes = baos.toByteArray();
                baos=null;
                CCStream.writeStream(bos, bytes, true);
                
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
                    CCStream.copyBlockReceive(fis, bos);
                    bos.flush();
                    fis.close();
                    System.out.println("File sent.");
                }
            }
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
        System.out.println("Done.");
    }
    
}
