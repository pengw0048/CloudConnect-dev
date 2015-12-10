package protocolreceiverj;
import ccutil.*;
import java.io.*;
import java.net.*;

public class ProtocolReceiverJ {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        InputStream is=null;
        OutputStream os=null;
        BufferedInputStream bis=null;
        BufferedOutputStream bos=null;
        ServerSocket serverSocket=null;
        Socket socket=null;
        try{
            // Listen, accept connection, open stream
            serverSocket=new ServerSocket(1209,1);
            System.out.println("Listening: "+serverSocket.toString());
            socket=serverSocket.accept();
            serverSocket.close();
            System.out.println("Accepted: "+socket.toString());
            is=socket.getInputStream();
            os=socket.getOutputStream();
            bis=new BufferedInputStream(is);
            bos=new BufferedOutputStream(os);
            
            // Main loop
            do{
                String str=CCStream.readString(bis);
                System.out.println("--"+str);
                
                // Message switch
                if (str.equals("EXIT")) break;
                else if (str.equals("META")){
                    // Get meta from client
                    byte[] buffer=CCStream.readByte(bis);
                    FileMetadata newMeta=null;
                    ByteArrayInputStream bais=new ByteArrayInputStream(buffer);
                    ObjectInputStream ois=new ObjectInputStream(bais);
                    newMeta=(FileMetadata)ois.readObject();
                    ois.close();
                    bais=null;
                    System.out.println(newMeta.name);
                    
                    if(new File("Z:/Cache/" + newMeta.name + ".meta").exists()){
                        
                    }else{
                        // Don't have meta = new file
                        System.out.println("New file.");
                        CCStream.writeStream(bos, "NEWF");

                        // Save new meta
                        FileOutputStream fos=new FileOutputStream("Z:/Cache/" + newMeta.name + ".meta");
                        ObjectOutputStream oos=new ObjectOutputStream(fos);
                        oos.writeObject(newMeta);
                        oos.close();
                        fos.close();

                        // Receive and save new file
                        fos=new FileOutputStream("Z:/Cache/" + newMeta.name);
                        CCStream.copyStream(bis, fos, (int)newMeta.size);
                        System.out.println("Transfer complete.");
                    }
                }
                
            }while(true);
        }catch(Exception e){
            System.out.println(e);
        }finally{
            try{
                bos.close();
                bis.close();
                os.close();
                is.close();
                socket.close();
                serverSocket.close();
            }catch(Exception e){
            }
        }
        System.out.println("Done.");
    }
    
}
