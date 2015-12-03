using System.IO;
using System;
using Util = CCUtil.CCUtil;

namespace CCUtil
{
    [Serializable]
    public class FileMetadata
    {
        public string name;
        public string hash;
        public long size;
        public DateTime modified;

        public FileMetadata() { }

        public FileMetadata(FileInfo file)
        {
            name = file.Name;
            size = file.Length;
            hash = Util.fileMD5(file.FullName);
            modified = file.LastWriteTimeUtc;
        }
    }
}
