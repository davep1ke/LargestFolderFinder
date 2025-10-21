using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace LargestFolderFinder
{
    public static class Program
    {
        /// <summary>
        /// General program mode - are we picking a file randomly, showing a list of files, or quiting.
        /// </summary>
        enum pickModes
        {
            single = 1,
            noPick = 3
        }

        /// <summary>
        /// List of follow-up strings that we are expecting.
        /// </summary>
        enum parseModes
        {
            Default = 1,
            exclude = 2,
            loadfile = 8,
            cache_min = 9,
            cache_max = 10,
            cachefolder =11,
            cacheignore = 12,
            addSingleFolder = 16,
            addFilteredFolderReg = 17,
            addFilteredFolderDir = 18
        }
        private static pickModes PickMode = pickModes.single;
        private static parseModes ParseMode = parseModes.Default;

        private static List<String> commands = new List<string>();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            //Load all the commands in args[] into a list so that we can add to them later if we get passed a filename,
            foreach (String s1 in args)
            {
                commands.Add (s1);
            }
            string s = "";
            string temporaryArg = ""; //for holding args for commands with 2 params
            
            if (args.Length > 0)
            {
                
                while (commands.Count > 0)
                {
                    s = commands[0];
                    s.Trim();
                    commands.RemoveAt(0);
                    switch(ParseMode)
                    {
                        //deafault mode
                        case parseModes.Default :
                            switch (s)
                            {
                                case "-x":
                                    ParseMode = parseModes.exclude;
                                    break;
                                    
                                case "-nr":
                                    FindLargestFolder.recurse = false;
                                    break;


                                case "-?":
                                case "--?":
                                case "-help":
                                case "-h":
                                    Help helpwindow = new Help(
                                    
                                        "Usage: \n" +
                                        "<dir>" + "\t\t\t" + "Search direct subdirectories of directory" + "\n" +
                                        "-c" + "\t\t\t" + "Choose folder after search" + "\n" +
                                        "-x <dir>" + "\t\t\t" + "Exclude directory" + "\n" +
                                        "-nr" + "\t\t\t" + "Disable recursing directories" + "\n" +
                                        "-l <filename>" + "\t\t" + "Load a set of commands from a file"  + "\n" +
                                        "-f <foldername>" + "\t\t" + "Add a single folder to the list (not subdirectories)" + "\n" +
                                        "-r <foldername> <filter>" + "\t" + "Add subfolders of a folder where they meet a regex" + "\n" +
                                        "-s" + "\t\t\t" + "Switch to smallest folder mode" + "\n" +
                                        "-cachestats" + "\t\t" + "Show cache / pick stats" + "\n" +
                                        "-cache <min> <max>" + "\t" + "Cache file access for min < x < max hours" + "\n" +
                                        "-cachefolder <directory>" + "\t" + "Folder where cache data should be written" + "\n" +
                                        "-cacheignore <directory>" + "\t" + "Never cache this folder"
                                        );
                                    Application.Run(helpwindow);
                                    PickMode = pickModes.noPick;
                                    break;


                                case "-c":
                                    FindLargestFolder.showChooser = true;
                                    break;

                                case "-l":
                                    ParseMode = parseModes.loadfile;
                                    break;

                                case "-f":
                                    ParseMode = parseModes.addSingleFolder;
                                    break;

                                case "-r":
                                    ParseMode = parseModes.addFilteredFolderDir;
                                    break;

                                case "-s":
                                    FindLargestFolder.pickSmallestFolder = true;
                                    break;

                                case "-cachestats":
                                    FindLargestFolder.showCacheStats = true;
                                    break;

                                case "-cache":
                                    FindLargestFolder.useCache = true;
                                    ParseMode = parseModes.cache_min;
                                    break;

                                case "-cachefolder":
                                    ParseMode = parseModes.cachefolder;
                                    break;

                                case "-cacheignore":
                                    ParseMode = parseModes.cacheignore;
                                    break;

                                default:
                                    FindLargestFolder.addPath(s);
                                    break;
                            }
                        break;
                        //exclude directory mode
                        case parseModes.exclude :
                            ParseMode = parseModes.Default;
                            FindLargestFolder.excludePath(s);
                            break;

                        case parseModes.loadfile:
                            //load a file one row at a time into args[]
                            ParseMode = parseModes.Default;
                            StreamReader f = new StreamReader(s);
                            string line;
                            while ((line = f.ReadLine()) != null)
                            {
                                commands.Add(line);
                            }
                            break;

                        case parseModes.addSingleFolder:
                            ParseMode = parseModes.Default;
                            //load a single folder straght into the list to check.
                            FindLargestFolder.addDirectoryDirectly(s);
                            break;

                        case parseModes.addFilteredFolderDir:
                            ParseMode = parseModes.addFilteredFolderReg;
                            temporaryArg = s;
                            break;

                        case parseModes.addFilteredFolderReg:
                            ParseMode = parseModes.Default;
                            FindLargestFolder.addFilteredFolder(temporaryArg, s);
                            break;

                        case parseModes.cachefolder:
                            ParseMode = parseModes.Default;
                            FindLargestFolder.cachefolder = s;
                            break;

                         case parseModes.cacheignore:
                            ParseMode = parseModes.Default;
                            FindLargestFolder.cacheIgnoreList.Add(s);
                            break;

                        case parseModes.cache_min:
                            ParseMode = parseModes.cache_max;
                            FindLargestFolder.cacheHoursMin = Convert.ToInt32(s);
                            break;

                        case parseModes.cache_max:
                            ParseMode = parseModes.Default;
                            FindLargestFolder.cacheHoursMax = Convert.ToInt32(s);
                            break;

                    }

                }
                #region pickModes
                //keep the pick modes as if we selected pickmode.none we dont want to actually do anything here.
                switch (PickMode)
                {
                    case pickModes.single:
                        FindLargestFolder.scanDirectories();
                        if (FindLargestFolder.showChooser)
                        {
                            FindLargestFolder.pickChoice();
                        }
                        else
                        {
                            if (FindLargestFolder.pickSmallestFolder)
                            {
                                FindLargestFolder.pickSmallest();
                            }
                            else
                            {
                                FindLargestFolder.pickLargest();
                            }
                        }
                        break;

                }
                #endregion

            }

            //save the cache
            FindLargestFolder.saveCache();

            //else
            //{


            //    Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //    Application.Run(new Monitor());
            //}
        }
        
    }


}