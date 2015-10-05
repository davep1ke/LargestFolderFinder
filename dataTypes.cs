using System;
using System.Collections.Generic;
using System.Text;

namespace LargestFolderFinder
{
    public class extAppPair
    {
        public string extension;
        public string application;

        public extAppPair(String ext, String app)
        {
            extension = ext;
            application = app;
        }
    }

    public class folderAndSize
    {
        public string folderPath;
        public long size;

        public folderAndSize(String folderPath, long size)
        {
            this.folderPath = folderPath;
            this.size = size;
        }

        public override string ToString()
        {
            return folderPath + " (" + size.ToString() + ")";
        }
    }


}
