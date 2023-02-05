// Old Skull Games
// Bernard Barthelemy
// Wednesday, August 2, 2017
//#define THREAD_SAFE


using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using OSG.Core;

namespace OSG
{
 /*   /// <summary>
    /// Add this attribute to a singleton class to have it instanciated automagically
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoCreateSingletonAttribute : Attribute
    {
        public readonly string name;
        public AutoCreateSingletonAttribute(string name)
        {
            this.name = name;
        }
    }*/

    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// 
    /// As a note, this is made as MonoBehaviour because we need Coroutines.
    /// </summary>
    [Obsolete("Try not to use singletons, that's killing kitten")]
    public class AutoSingleton<T> : Singleton<T> where T : OSGMono
    {
        // ReSharper disable once StaticMemberInGenericType
        private static bool preventAutoCreationOnQuit;

        [Conditional("DEBUG")]
        static void CheckInstanceAccessConditions()
        {
            if(!preventAutoCreationOnQuit) return;
#if UNITY_EDITOR
            if(!EditorApplication.isPlaying)
            {
                preventAutoCreationOnQuit = false;
                return;
            }
#endif
            throw new InvalidOperationException("Accessing instance of " + typeof(T).Name + " while application is quiting is forbidden");
        }

        
        public static bool HasBeenInstantiated
        {
            get
            {
#if THREAD_SAFE
                lock(_lock)
#endif
                {
                    return instance;
                }
            }
        }

        private void OnApplicationQuit()
        {
            Log("Application quitting, we won't allow to create anything new from now on for " + typeof(T).Name +  " " + name);
            preventAutoCreationOnQuit = true;
#if THREAD_SAFE
            lock(_lock)
#endif
            {
                instance = null;
            }
        }

        public new static T Instance
        {
            get
            {
                CheckInstanceAccessConditions();
                return instance ? instance : Create();
            }
        }

        public static T Create()
        {
#if THREAD_SAFE
            lock (_lock)
#endif
            {
                instance = Search();
                if (!(instance || preventAutoCreationOnQuit) )
                {
                    instance = Search();
                    if (!instance)
                    {
                        instance = new GameObject(typeof(T).Name).AddComponent<T>();
                        Debug.Log("[Singleton] An instance of " + typeof(T) +
                                              " is needed in the scene, so '" + instance.name +
                                              "' was created.");
                    }

                    MakeDontDestroyOnLoadInPlayMode(instance.gameObject);
                }

                return instance;
            }
        }

        private static void MakeDontDestroyOnLoadInPlayMode(GameObject singleton)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying) 
                return;
#endif
            DontDestroyOnLoad(singleton);
            Debug.Log("[Singleton] gameobject "+singleton.name+" set to don't destroy on load");
        }
    }
    [Obsolete("PersistentSingleton is deprecated, use AutoSingleton instead", true)]
    public class PersistentSingleton<T> : OSGMono where T : OSGMono
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    // search for object of same kind
                    instance = FindObjectOfType<T>();
                    if (!instance)
                    {
                        GameObject obj = new GameObject();
                        instance = obj.AddComponent<T>();
                        obj.name = instance.GetType().ToString();
                    }
                }

                return instance;
            }
        }

        public virtual void Awake()
        {
            if (!instance)
            {
                instance = this as T;
                DontDestroyOnLoad(transform.gameObject);
            }
            else
            {
                if (this != instance)
                {
                    Destroy(gameObject);
                }
            }
        }

    }

    public class Singleton<T> : OSGMono where T : OSGMono
    {
        protected static T instance;
#if THREAD_SAFE
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly object _lock = new object();
#endif

        public static bool Exists()
        {
            return instance;
        }

        protected static T Search()
        {
            Object[] objects = FindObjectsOfType(typeof(T));
            if (objects.Length > 1)
            {
                Debug.LogError("[Singleton] Something went really wrong " +
                                           " - there should never be more than 1 singleton!" +
                                           " Reopening the scene might fix it.");

                return null;
            }

            return objects.InRange(0) ? (T) objects[0] : null;
        }

        
        public static T Instance
        {
            get
            {
#if THREAD_SAFE
                lock (_lock)
#endif
                {
                 //   #if UNITY_EDITOR
                    if(! instance)
                    {
                        instance = Search();
                    }
                   // #endif

                    return instance;
                }
            }
        }

        protected  virtual bool ShouldAutoDestroyIfInstanceExists(){
            return false;
        }
        
        protected virtual void Awake()
        {
#if THREAD_SAFE
            lock (_lock)
#endif
            {
                if (instance && instance != this)
                {
                    if (ShouldAutoDestroyIfInstanceExists())
                    {
                        Debug.Log($"Instance of {GetType()} already exists. Destroying the {GetType()} component in {gameObject.name}");
                        Destroy(this);
                        return;
                    }
                    else
                    {
                        Debug.LogError("Instance of " + GetType() 
                                                      + " already existing at " + instance.gameObject.scene.name+":"+instance.transform.HierarchyPath() 
                                                      + " ... overriding it with " + gameObject.scene.name+":"+transform.HierarchyPath());   
                    }
                }
                instance = GetComponent<T>();
            }
        }
    }
    
    
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        protected static T _instance;
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    string path = PathName();
                    // UnityEngine.Debug.LogFormat("Loading {0}'s singleton instance at '{1}'", typeof(T).Name, path);

                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    _instance = Resources.Load<T>(path);
                    watch.Stop();

                    if (!_instance)
                    {
                        Debug.LogWarningFormat("Can't load singleton at {0}. Make sure you've created it with the right name", path);
                    }
                    else
                    {
                        var singletonInstance = _instance as ScriptableObjectSingleton<T>;
                        singletonInstance.OnInstanceLoaded();
                    }

                    if (watch.ElapsedMilliseconds > 500)
                    {
                        Debug.LogWarning("Singleton '" + path + "' took " + watch.ElapsedMilliseconds + " milliseconds to load.");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Tests whether or not this type of singleton has a .asset ScriptableObject in the project Resources.
        /// </summary>
        public static bool HasAsset
        {
            get
            {
                if(_instance) return true;
                _instance = Resources.Load<T>(PathName());
                return _instance;
            }
        }

        private static string PathName()
        {
            object[] customAttributes = typeof(T).GetCustomAttributes(typeof(ScriptableSingletonFolderAttribute), false);
            if (customAttributes.Length > 0)
            {
                ScriptableSingletonFolderAttribute attribute = customAttributes[0] as ScriptableSingletonFolderAttribute;
                if (attribute != null)
                    return string.Format("{0}/{1}", attribute.folder , typeof(T).Name);
            }
            return typeof(T).Name;
        }

        public static void Refresh()
        {
            _instance = null;        
        }

        /// <summary>
        /// Called when the .asset of the ScriptableObject is created.
        /// </summary>
        public virtual void OnSingletonCreate()
        {
        }

        /// <summary>
        /// Called when the singleton's instance is loaded.
        /// </summary>
        protected virtual void OnInstanceLoaded()
        {
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScriptableSingletonFolderAttribute : Attribute
    {
        public readonly string folder;
        public ScriptableSingletonFolderAttribute(string folder)
        {
            this.folder = folder;
        }
    }
}
