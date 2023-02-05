using System;
using UnityEngine;

namespace OSG
{
    [Serializable]
    public class FolderContent
    {
        public string[] names;
        public string path;

        public int Count => names?.Length ?? 0;

        public T Load<T>(int index) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path+'/'+ names[index]);
        }
    }

}