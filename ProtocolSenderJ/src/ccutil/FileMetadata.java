package ccutil;
import java.io.*;

public class FileMetadata implements Serializable {
    public String name;
    public String hash;
    public long size;
    public long modified;

    public FileMetadata() { }

    public FileMetadata(File file)
    {
        name = file.getName();
        size = file.length();
        //hash = Util.fileMD5(file.FullName);
        modified = file.lastModified();
    }
}

