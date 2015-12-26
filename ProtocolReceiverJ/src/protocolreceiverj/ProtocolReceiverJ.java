package protocolreceiverj;
import ccutil.*;
import java.io.*;
import java.net.*;
import java.util.*;

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
                    
                    if(new File(args[0] + newMeta.name + ".meta").exists()){
                        // Has older version
                        FileInputStream fis=new FileInputStream(args[0] + newMeta.name + ".meta");
                        ois=new ObjectInputStream(fis);
                        FileMetadata oldMeta=(FileMetadata)ois.readObject();
                        ois.close();
                        fis.close();
                        
                        //if (newMeta.hash != oldMeta.hash)
                        if (newMeta.modified != oldMeta.modified){
                            // This file has been updated
                            CCStream.writeStream(bos,"DIFF");
                            System.out.println("Sending signature.");
                            FileInputStream fis2=new FileInputStream(args[0] + newMeta.name);
                            BufferedInputStream bis2=new BufferedInputStream(fis2);
                            Vector signature=SSync.getSignature(bos,bis2,64*1024);
                            bis2.close();
                            fis2.close();
                            CCStream.writeObject(bos,signature);
                            
                            // Wait for delta from client
                            System.out.println("Receiving delta.");
                            RandomAccessFile raf=new RandomAccessFile(args[0] + newMeta.name,"rw");
                            FileOutputStream fos2=new FileOutputStream(args[0] + newMeta.name + ".new");
                            BufferedOutputStream bos2=new BufferedOutputStream(fos2);
                            SSync.applyDelta(raf,bis,bos2);
                            bos2.close();
                            fos2.close();
                            raf.close();
                            System.out.println("Delta applied.");
                            
                            // Clean up
                            new File(args[0] + newMeta.name).delete();
                            new File(args[0] + newMeta.name + ".new").renameTo(new File(args[0] + newMeta.name));
                            
                            // Save new meta
                            FileOutputStream fos=new FileOutputStream(args[0] + newMeta.name + ".meta");
                            ObjectOutputStream oos=new ObjectOutputStream(fos);
                            oos.writeObject(newMeta);
                            oos.close();
                            fos.close();
                        }else{
                            // File metas match
                            System.out.println("Up to date.");
                            CCStream.writeStream(bos, "PASS");
                        }
                    }else{
                        // Don't have meta = new file
                        System.out.println("New file.");
                        CCStream.writeStream(bos, "NEWF");

                        // Save new meta
                        FileOutputStream fos=new FileOutputStream(args[0] + newMeta.name + ".meta");
                        ObjectOutputStream oos=new ObjectOutputStream(fos);
                        oos.writeObject(newMeta);
                        oos.close();
                        fos.close();

                        // Receive and save new file
                        fos=new FileOutputStream(args[0] + newMeta.name);
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
