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
        Socket socket=null;
        String data="Fuck!";
        try{
            socket=new Socket("127.0.0.1",1209);
            System.out.println("Connected: "+socket.toString());
            is=socket.getInputStream();
            os=socket.getOutputStream();
            os.write(data.getBytes());
        }catch(Exception e){
            System.out.println(e);
        }finally{
            try{
                os.close();
                is.close();
                socket.close();
            }catch(Exception e){
            }
        }
        System.out.println("Done.");
    }
    
}
