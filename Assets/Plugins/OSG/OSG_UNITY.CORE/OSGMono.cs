
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace OSG
{
    /// <summary>
    /// Base class for our monobehaviours
    /// </summary>
    [HelpURL("https://confluence.gamehouse.com/pages/viewpage.action?pageId=18324523")]
    public abstract class OSGMono : MonoBehaviour
    {
        [Conditional("DEBUG")]
        public static void Log(string message, LogType logType= LogType.Log)
        {
            DateTime date = DateTime.Now;
            message = date.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " " + message;
            UnityEngine.Debug.unityLogger.Log(logType, message);
        }

        private Transform _transformCached;
        public Transform transformCached
        {
            get
            {
                // ?? operator or == null doesn't work well with UnityEngine.Object type
                // use boolean cast instead, it'll work with destroyed not null objects too
                if(!_transformCached)
                {
                    _transformCached = transform;
                }
                return _transformCached;
            }
        }

        public Vector3 position { get { return transformCached.position; } set { transformCached.position = value; } }
        public Vector3 localPosition { get { return transformCached.localPosition; } set { transformCached.localPosition = value; } }
        public Quaternion rotation { get { return transformCached.rotation; } set { transformCached.rotation = value; } }
        public Quaternion localRotation { get { return transformCached.localRotation; } set { transformCached.localRotation = value; } }
        public Vector3 eulerAngles { get { return transformCached.eulerAngles; } set { transformCached.eulerAngles = value; } }
        public Vector3 localScale { get { return transformCached.localScale; } set { transformCached.localScale = value; } }

        IEnumerator Start()
        {
            yield return Init();
        }

        public virtual IEnumerator Init()
        {
            yield break;
        }

        public virtual void SanityCheck(){
            
        }
    }
}