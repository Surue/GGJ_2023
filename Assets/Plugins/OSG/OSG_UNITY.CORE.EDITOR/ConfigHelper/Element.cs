// Old Skull Games
// Bernard Barthelemy
// Friday, April 26, 2019

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OSG.ConfigHelper
{
    [Serializable]
    internal class Element 
    {
        public enum ElementState
        {
            Unknown,
            Ok,
            Missing,
            Different,
        }


        [HideInInspector][SerializeField] public string guid;
        [HideInInspector][SerializeField] public string assetRelativePath;
        public bool isFolder;

        private string n;

        private bool pathSet;

        public string name
        {
            get
            {
                if(string.IsNullOrEmpty(n))
                    n = Path.GetFileName(assetRelativePath);
                return n;
            }
        }


        private ElementState State = ElementState.Unknown;
        private Config configForState;

        public Element(string path)
        {
            guid = AssetDatabase.AssetPathToGUID(path);
            SetPath();
            isFolder = Directory.Exists(AssetAbsolutePath);
        }
        /// <summary>
        /// Copies a file or a folder and its .meta file from source ABSOLUTE path to destination ABSOLUTE path
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public void Copy(string source, string destination)
        {
            string destinationMetaPath = destination + ".meta";
            if (isFolder)
            {
                DirectoryHelpers.DeleteDirectory(destination);
                DirectoryHelpers.DirectoryCopy(source, destination, true);
            }
            else
            {
                string directoryName = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                    File.Copy(source, destination, true);
                }
            }
            File.Copy(source + ".meta", destinationMetaPath, true);
        }

        public string AssetAbsolutePath
        {
            get {
                if(!pathSet)
                {
                    SetPath();
                }
                return DirectoryHelpers.Combine(Application.dataPath, assetRelativePath); 
            }
        }

        public void Include(string archiveAbsolutePath)
        {
            string assetAbsolutePath = AssetAbsolutePath;
            Debug.Log("copy " + archiveAbsolutePath + " to " + assetAbsolutePath);
            Copy(archiveAbsolutePath, assetAbsolutePath);
        }


        public void Delete(string absolutePath)
        {
            Debug.Log("Delete " + absolutePath);
            if (isFolder)
            {
                DirectoryHelpers.DeleteDirectory(absolutePath);
            }
            else
            {
                DirectoryHelpers.DeleteFile(absolutePath);
            }

            string meta = absolutePath + ".meta";
            DirectoryHelpers.DeleteFile(meta);
        }

        public void Exclude()
        {
            Delete(AssetAbsolutePath);
        }

        public bool Exists(string path)
        {
            return isFolder ? Directory.Exists(path) : File.Exists(path);
        }


        private void SetPath()
        {
            string guidToAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            if(string.IsNullOrEmpty(guidToAssetPath))
            {
                pathSet = !string.IsNullOrEmpty(assetRelativePath);
                return;
            }
            string newPath = guidToAssetPath.Remove(0, 6);
            if(!string.IsNullOrEmpty(newPath))
            {
                if(!string.IsNullOrEmpty(assetRelativePath) && assetRelativePath != newPath)
                {
                    Debug.LogWarning(assetRelativePath + " has been renamed or moved to " + newPath + " you should recreate the whole element");
                }
                assetRelativePath = newPath;
                pathSet = true;
            }
        }

        public bool IsEqualToArchive(string archivePath)
        {
            if (isFolder)
            {
                return DirectoryHelpers.CompareDirectories(archivePath, AssetAbsolutePath);
            }

            return DirectoryHelpers.CompareFiles(archivePath, AssetAbsolutePath);
        }

        public ElementState GetStateForConfig(Config config, bool forceUpdate)
        {
            if (config == configForState && !forceUpdate)
                return State;
            configForState = config;
            if(config == null)
            {
                State = ElementState.Unknown;
                return State;
            }

            Inclusion inclusion = config.Uses(this);
            if (inclusion == null)
            {
                State = Exists(AssetAbsolutePath)
                    ? ElementState.Different
                    : ElementState.Ok;
            }
            else if (!Exists(AssetAbsolutePath))
            {
                State = ElementState.Missing;
            }
            else
            {
                State = IsEqualToArchive(config.GetArchiveAbsolutePath(this, inclusion.overrideArchivePath))
                    ? ElementState.Ok
                    : ElementState.Different;
            }
            return State;
        }
    }
}