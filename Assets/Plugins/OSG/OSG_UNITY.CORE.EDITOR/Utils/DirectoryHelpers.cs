// Old Skull Games
// Bernard Barthelemy
// Tuesday, September 11, 2018

using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OSG
{
    public static class DirectoryHelpers
    {
        private static readonly char[] separators = {
            '/','\\'
        };

        private static string NoSlash(string p)
        {
            return p == null ? "" : p.TrimEnd(separators).TrimStart(separators);
        }

        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return "";

            string result = NoSlash(paths[0]);
            for(int i = 1; i < paths.Length;++i)
            {
                result += "/" + NoSlash(paths[i]);
            }
            return result;
        }


        public static void DeleteDirectory(string absolutePath, bool _throw=false)
        {
            try
            {
                if (Directory.Exists(absolutePath))
                {
                    Directory.Delete(absolutePath, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"While deleting directory {absolutePath}");
                Debug.LogException(e);
                if (_throw)
                {
                    throw;
                }
            }
        }


        public static void DeleteFile(string absolutePath, bool _throw=false)
        {
            try
            {
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"While deleting file {absolutePath}");
                Debug.LogException(e);

                if (_throw)
                {
                    throw;
                }
            }
        }


        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static bool CompareDirectories(string path1, string path2)
        {
            bool dir1Exists = Directory.Exists(path1);
            bool dir2Exists = Directory.Exists(path2);

            if(dir1Exists ^ dir2Exists)
            {
                return false;
            }
            if(!dir1Exists)
            {
                return true;
            }
            DirectoryInfo dir1 = new DirectoryInfo(path1);
            DirectoryInfo dir2 = new DirectoryInfo(path2);

            FileInfo[] files1 = dir1.GetFiles();
            FileInfo[] files2 = dir2.GetFiles();

            if (files1.Length != files2.Length)
                return false;

            if (files1.Where((t, i) => !CompareFiles(t, files2[i])).Any())
            {
                return false;
            }

            var dirs1 = dir1.GetDirectories();
            var dirs2 = dir2.GetDirectories();

            if(dirs1.Length != dirs2.Length)
            {
                return false;
            }

            return !dirs1.Where((t, i) => !CompareDirectories(t.FullName, dirs2[i].FullName)).Any();
        }

        private static bool CompareFiles(FileInfo file1, FileInfo file2)
        {
            if (file1.Name != file2.Name)
                return false;
            if (file1.Length != file2.Length)
                return false;
            var stream1 = file1.OpenRead();
            var stream2 = file1.OpenRead();
            byte[] b1 = new byte[file1.Length];
            
            if(file1.Length > (long)0xFFFFFFFF)
                throw new Exception("File size too big");

            stream1.Read(b1, 0, (int)file1.Length);

            byte[] b2 = new byte[file2.Length];
            stream2.Read(b2, 0, (int)file2.Length);

            for(int i = 0; i < file1.Length;++i)
            {
                if (b1[i] != b2[i])
                    return false;
            }
            return true;
        }


        public static bool CompareFiles(string filePath1, string filePath2)
        {
            return CompareFiles(new FileInfo(filePath1), new FileInfo(filePath2));
        }
    }
}
