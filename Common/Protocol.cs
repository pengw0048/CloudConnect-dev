using System.Net;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Diagnostics;
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
