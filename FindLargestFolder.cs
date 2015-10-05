using System;
using System.Diagnostics;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace LargestFolderFinder
{
    public static class FindLargestFolder
    {
        public static List<DirectoryInfo> folderList = new List<DirectoryInfo>();
        public static List<DirectoryInfo> excludeList = new List<DirectoryInfo>();

        public static List<folderAndSize> allFolders = new List<folderAndSize>();
        public static bool recurse = true;
       
        //public static bool firstfileonly = false;
       // public static bool hideSpawnedWindows = false;

        //cacheing
        public static bool useCache = false;
        public static bool showCacheStats = false;
        public static int cacheHoursMin = 0;
        public static int cacheHoursMax = 0;
        public static string cachefolder = "";
        public static List<string> cacheIgnoreList = new List<string>();

        private static DirCache cache;
     
        public static bool showChooser = false;

        public static void addDirectoryDirectly(string folderPath)
        {
            DirectoryInfo d = new DirectoryInfo(folderPath);
            long size = getSizeOfFolder(d);
            allFolders.Add(new folderAndSize(d.FullName, size));
        }

        public static void scanDirectories()
        {

            double tim_dir = 0;
            int dirs_scanned = 0;
            DateTime time_start = DateTime.Now;

            //if in cache mode, load the cache.
            try
            {
                if (useCache) { cache = new DirCache(); }
            }
            catch (Exception e)
            {
                throw new Exception("Error loading cache", e);
            }

            //add all files from our pathlist to an arraylist
            //(or use cache where appropriate)
            foreach (DirectoryInfo d in FindLargestFolder.folderList)
            {
                foreach (DirectoryInfo sd in d.GetDirectories())
                {
                    dirs_scanned++;
                    long size = -1;
                    if (useCache)
                    {
                        //TODO

                        size = cache.getCachedDirSize(sd.FullName);
                    }

                    //cache may have returned -1
                    if (size == -1)
                    {
                        size = getSizeOfFolder(sd);
                    }
                    allFolders.Add(new folderAndSize(sd.FullName, size));

                    //now update the cache
                    if (useCache) { cache.addCacheData(sd.FullName, size, -1); }

                }
                tim_dir = DateTime.Now.Subtract(time_start).Seconds;
            }

            if (showCacheStats)
            {
                MessageBox.Show(
                    "Total Time: " + DateTime.Now.Subtract(time_start).TotalSeconds.ToString() + "\n" +
                    "Scan Time: " + tim_dir.ToString() +
                    (useCache ? "\n" + cache.getStats() : "")
                    );
            }


        }
        public static void pickLargest()
        {

            folderAndSize largest = null;
            foreach (folderAndSize f in allFolders)
            {
                if (largest == null || f.size > largest.size)
                {
                    largest = f;
                }
            }

            //pick largest folder from our list
            if (largest != null)
            {
                openFolder(largest.folderPath);
                //remove the items as we have picked it, and want to rescan it next time. 
                cache.removeCacheData(largest.folderPath);
            }
        }
        public static void pickChoice()
        {
            Chooser c = new Chooser();
            c.ShowDialog();
            if (c.DialogResult == DialogResult.OK)
            {
                openFolder(c.result.folderPath);
                //remove the items as we have picked it, and want to rescan it next time. 
                cache.removeCacheData(c.result.folderPath);
            }
        }

       

        static long getSizeOfFolder(DirectoryInfo d)
        {
            long totalSize = 0;
            try
            {
                if (!FindLargestFolder.isDirectoryExcluded(d.FullName))
                {
                    //loop through any subdirs
                    if (recurse == true)
                    {

                        foreach (DirectoryInfo subdir in d.GetDirectories())
                        {
                            //recurse and all all subdirs
                            try
                            {
                                totalSize = totalSize + getSizeOfFolder(subdir);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                //ignore it..
                            }
                        }
                    }

                    //add files from current folder
                    FileInfo[] fiArray = d.GetFiles();

                    foreach (FileInfo f in fiArray)
                    {
                        totalSize += f.Length;
                    }
                }
            }

            catch (DirectoryNotFoundException) { };




            return totalSize;
        }
        public static void addPath(String folderPath)
        {
            DirectoryInfo d = new DirectoryInfo(folderPath);
            folderList.Add(d);
        }


        public static void excludePath(String folderPath)
        {
            DirectoryInfo d = new DirectoryInfo(folderPath);
            excludeList.Add(d);
        }

       

        public static bool isDirectoryExcluded(string fullname)
        {
            foreach (DirectoryInfo di in excludeList)
            {
                if (di.FullName == fullname)
                {
                    return true;
                }
            }
            return false;
        }



        public static void openFolder(string folderPath)
        {
            ProcessStartInfo startinfo = new ProcessStartInfo();

            startinfo = new ProcessStartInfo("explorer.exe", "\"" + folderPath + "\"");

            Process.Start(startinfo);
        }

        public static void saveCache()
        {
            if (useCache) { cache.saveCache(); }
        }
    }
}
