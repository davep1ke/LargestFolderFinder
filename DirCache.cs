using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace LargestFolderFinder
{
    public class DirCache
    {
        private int hits = 0;
        private int missed = 0;
        private int cache_Start = 0;
        private int cache_stale = 0;
        string status = "uninitialised";


        [Serializable]
        private class DirectoryCacheInfo
        {
            public String path;
            public DateTime expiry;
            public Guid Guid;
            public int totalFileCount = 0;
            public long totalFileSize = 0;
        }

        
        private List<DirectoryCacheInfo> theCache = new List<DirectoryCacheInfo>();
        
        FileInfo cacheFile;
        String cacheDirectory = "";
        FileInfo lockFile;

        public DirCache()
        {
            if (FindLargestFolder.cachefolder == "")
            {
                cacheDirectory = new Uri(System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
            }
            else
            {
                cacheDirectory = FindLargestFolder.cachefolder;
            }
            status = "unavailable";

            cacheFile = new FileInfo(cacheDirectory + "\\RandomFilePicker.cache");
            lockFile = new FileInfo(cacheDirectory + "\\RandomFilePicker.lock");

            //TODO try and lock

            bool ignoreCache = false;
            while (lockFile.Exists && status == "unavailable" && ignoreCache == false)
            {
                DialogResult r = MessageBox.Show("Cache Locked. Retry, Ignore (skip cache), or Abort?", "Cache Locked", MessageBoxButtons.AbortRetryIgnore);

                //r == DialogResult.Retry - will loop
                if (r == DialogResult.Abort) { Application.Exit(); }
                if (r == DialogResult.Ignore)
                {
                    ignoreCache = true;
                    FindLargestFolder.useCache = false;
                }
            }


            if (!ignoreCache)
            {
                //if there is no existing cache, then the cache is ready (but unpopulated). Should be written on exit.
                if (cacheFile.Exists)
                {
                    //TODO lock cache


                    //load cache
                    using (Stream stream = File.Open(cacheFile.FullName, FileMode.Open))
                    {
                        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        theCache = (List<DirectoryCacheInfo>)binaryFormatter.Deserialize(stream);
                    }

                    cache_Start = theCache.Count;

                    //expire anything that is stale. Add to separate list as we can't edit it as we are looping through. 
                    List<DirectoryCacheInfo> stale = new List<DirectoryCacheInfo>();
                    foreach (DirectoryCacheInfo dic in theCache)
                    {
                        if (DateTime.Compare(DateTime.Now, dic.expiry) > 0)
                        {
                            stale.Add(dic);
                        }
                    }
                    cache_stale = stale.Count;
                    foreach (DirectoryCacheInfo dic in stale)
                    {
                        theCache.Remove(dic);
                    }

                }

                status = "ready";

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns>Null if not in cache</returns>
        public long getCachedDirSize(String directoryName)
        {
            //drop out if we are ignoring the cache for this folder
            if (FindLargestFolder.cacheIgnoreList.Contains(directoryName))
            {
                missed++;
                return -1;
            }
            //TODO check cache status first. should be ready
            foreach (DirectoryCacheInfo dic in theCache)
            {
                if (dic.path == directoryName)
                {
                    hits++;
                    return dic.totalFileSize;
                }
            }
            missed++;
            return -1;
        }

        /// <summary>
        /// Remove an item from the cache
        /// </summary>
        /// <param name="path"></param>
        public void removeCacheData(String path)
        {
            DirectoryCacheInfo existing = null;
            foreach (DirectoryCacheInfo di in theCache)
            {
                if (di.path == path) { existing = di; }
            }
            theCache.Remove(existing);
        }

        public void addCacheData(String path, long totalFileSize, int totalFileCount)
        {
            //TODO check cache status first. should be ready
            //TODO theoretically this shouldnt exist in the cache. we should probably remove it anyway if it does.
            //TODO - for safety should remove any instances of duplicate, not just this one. 

            removeCacheData(path);
            
            Random r = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute);
            Guid g = Guid.NewGuid();

            theCache.Add(new DirectoryCacheInfo()
            {
                totalFileCount = totalFileCount,
                Guid = g,
                totalFileSize = totalFileSize,
                path = path,
                expiry = DateTime.Now.AddHours(r.Next(FindLargestFolder.cacheHoursMin, FindLargestFolder.cacheHoursMax))

            });

        }

        public void saveCache()
        {

            //save the FULL list of cached files (including ones we didnt open).
            using (Stream stream = File.Open(cacheFile.FullName, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, theCache);
            }


            //TODO - clear lock.
            status = "unavailable";
        }

        public string getStats()
        {
            return
                "Cache Expired: " + cache_stale.ToString() + "\n" +
                "Cache Hits: " + hits.ToString() + ", Misses:" + missed.ToString() + "\n" +
                "Cache Size Start: " + cache_Start.ToString() + "\n" +
                "Cache Size End: " + theCache.Count.ToString();
        }
    }
}
