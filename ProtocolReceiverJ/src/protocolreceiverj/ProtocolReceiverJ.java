package protocolreceiverj;
import java.io.*;
import java.net.*;

public class ProtocolReceiverJ {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        InputStream is=null;
        OutputStream os=null;
        ServerSocket serverSocket=null;
        Socket socket=null;
        try{
            serverSocket=new ServerSocket(1209,1);
            System.out.println("Listening: "+serverSocket.toString());
            socket=serverSocket.accept();
            serverSocket.close();
            System.out.println("Accepted: "+socket.toString());
            is=socket.getInputStream();
            os=socket.getOutputStream();
            byte[] b=new byte[1024];
            int n=is.read(b, 0, 1024);
            System.out.println("客户端发送内容为：" + new String(b,0,n));
        }catch(Exception e){
            System.out.println(e);
        }finally{
            try{
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
