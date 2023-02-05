// Old Skull Games
// Bernard Barthelemy
// Monday, October 16, 2017

using System;
using System.Diagnostics;
using System.Reflection;
using ScanParametersContainer = System.Collections.Generic.List<OSG.Core.ScanParameters>;

namespace OSG.Core
{
    /// <summary>
    /// Scans assemblies for types
    /// \details
    /// 
    /// AssemblyScanner.Scan calls all registered callbacks.
    /// Callbacks can be either statically (via the `AssemblyScanner.ProcessType`) 
    /// or dynamically registered (via `AssemblyScanner.Register`)
    /// 
    /// Example of use:
    /// 
    ///     private static bool FilterAssemblyOut(Assembly assembly){
    ///         return !(assembly.FullName.StartsWith("Assembly-CSharp") || assembly.FullName.StartsWith("UnityEngine"));
    ///     }
    ///         
    ///     [AssemblyScanner.ProcessType("FilterAssemblyOut")]
    ///     static void ProcessType(Type type){
    ///         if (typeof(Object).IsAssignableFrom(type)) {
    ///             AssemblyTypes.Add(type);
    ///         }
    ///     }
    /// 
    /// This will add any class derived from UnityEngine.Object that's in unity or in the current project, as the ProcessTypeAttribute's parameter is a Filtering method that rejects any assembly not  named UnityEngine or Assembly_CSharp
    /// **Note:**
    /// Don't assume *YOUR* call of AssemblyScanner.Scan will be the first one to be executed. Therefore, don't assume 
    /// you can allocate some stuff (like a list of type) *BEFORE* the scan is done.
    /// Whenever you need some stuff allocated, do it in the ProcessType function. 
    /// In the example, AssemblyTypes looks like:
    /// 
    ///     static List<Type> AssemblyTypes { get { return _assemblyTypes??(_assemblyTypes=new List<Type>());}}
    /// 
    /// </summary>
    public static class AssemblyScanner
    {
        private static ScanParametersContainer s_scanParameters;

        private static ScanParametersContainer s_autoBuildParameters;
        private static bool m_autoScanDone;
        /// <summary>
        /// Register a callback for the next Scan
        /// </summary>
        /// <param name="processTypeUserData"></param>
        /// <param name="userData"></param>
        /// <param name="assemblyFilterOut"></param>
        public static void Register(ProcessTypeUserDataDelegate processTypeUserData,
            object userData,
            AssemblyFilterOutDelegate assemblyFilterOut = null)
        {
            if (s_scanParameters == null)
            {
                s_scanParameters = new ScanParametersContainer();
            }
            s_scanParameters.Add(new ScanParameters(processTypeUserData, assemblyFilterOut, userData));
        }
        /// <summary>
        /// Register a callback for the next Scan
        /// </summary>
        /// <param name="processType"></param>
        /// <param name="assemblyFilterOut"></param>
        public static void Register(ProcessTypeDelegate processType, AssemblyFilterOutDelegate assemblyFilterOut = null)
        {
            if (s_scanParameters == null)
            {
                s_scanParameters = new ScanParametersContainer();
            }
            s_scanParameters.Add(new ScanParameters(processType, assemblyFilterOut));
        }

        private static void ProcessType(Type type)
        {

            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo methodInfo in methods)
            {
                object[] customAttributes = methodInfo.GetCustomAttributes(typeof(ProcessTypeAttribute), false);
                foreach (object customAttribute in customAttributes)
                {
                    AddToAutoBuild(type, customAttribute as ProcessTypeAttribute, methodInfo);
                }
            }
        }


        private static void AddToAutoBuild(Type type, ProcessTypeAttribute processTypeAttribute, MethodInfo methodInfo)
        {
            ProcessTypeDelegate delegateNoData = methodInfo.MakeDelegate<ProcessTypeDelegate>();
            if (delegateNoData != null)
            {
                s_autoBuildParameters.Add(new ScanParameters(delegateNoData, processTypeAttribute.AssemblyFilter(type)));
            }
        }

        private static bool scanning;


        private static event Action onScanDone;

        /// <summary>
        /// Initiate the assembly scanning.
        /// 
        /// All registered callbacks will be called.
        /// If no new callbacks are registered since the last call of Scan,
        /// nothing will happen.<br/>
        /// Scan can be safely called from one of the callbacks, 
        /// inner call will be ignored (as well as new registered callbacks)
        /// </summary>
        [Obsolete("Use Scan(Action action) override of Scan, and check that you don't use any result of this scan too early (i.e. not in the callback)")]
        public static void Scan()
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            Scan(null);
        }

        public static void Scan(Action onDone)
        {
            if(onDone!=null)
            {
                onScanDone += onDone;
            }

            if (scanning)
            {
                return;
            }
            Stopwatch watch = new Stopwatch();
            watch.Start();

            scanning = true;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (!m_autoScanDone)
            {
                m_autoScanDone = true;
                ScanParametersContainer alreadyRegistered = s_scanParameters;
                s_autoBuildParameters = new ScanParametersContainer();
                Register(ProcessType);
                ScanAssemblies(assemblies);
                s_scanParameters = s_autoBuildParameters;
                if (alreadyRegistered != null)
                {
                    s_scanParameters.AddRange(alreadyRegistered);
                }
            }
            ScanAssemblies(assemblies);
            scanning = false;
            if(onScanDone != null)
            {
                onScanDone();
                onScanDone = null;
            }
            //UnityEngine.Debug.Log("Assembly scanner took " + watch.Elapsed.TotalSeconds.ToString("0.000") + " seconds");
        }

        private static void ScanAssemblies(Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                ScanAssembly(assembly);
            }
            s_scanParameters.Clear();
        }

        private static void ScanAssembly(Assembly assembly)
        {
            Type[] assemblyTypes = null;
            try
            {
                foreach (ScanParameters parameters in s_scanParameters)
                {
                    if (!parameters.FilterOutAssembly(assembly))
                    {
                        assemblyTypes = assemblyTypes ?? GetTypes(assembly);
                        foreach (Type type in assemblyTypes)
                        {
                            parameters.Scan(type);
                        }
                    }
                }
            }
            catch (InvalidOperationException e)
            {
                UnityEngine.Debug.LogError("Assembly " + assembly.GetName());
                UnityEngine.Debug.LogException(e);
            }
        }

        private static Type[] GetTypes(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (Exception)
            {
                types = assembly.GetExportedTypes();
            }
            return types;
        }
        /// <summary>
        /// Statically registers a method for the first Scan
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class ProcessTypeAttribute : Attribute
        {
            private readonly string assemblyFilterName;
            private AssemblyFilterOutDelegate assemblyFilter;
            /// <summary>
            /// Register the method for all types in all assemblies
            /// </summary>
            public ProcessTypeAttribute()
            {
                assemblyFilterName = null;
            }
            /// <summary>
            /// Register the method for all types in assemblies
            /// that are not excluded by the method given in parameter.
            /// </summary>
            /// <param name="assemblyFilterName">A AssemblyFilterOutDelegate that returns true if the assembly must be ignored.</param>
            public ProcessTypeAttribute(string assemblyFilterName)
            {
                this.assemblyFilterName = assemblyFilterName;
            }

            internal AssemblyFilterOutDelegate AssemblyFilter(Type type)
            {
                if (assemblyFilter != null || string.IsNullOrEmpty(assemblyFilterName)) return assemblyFilter;
                MethodInfo mi = type.GetMethod(assemblyFilterName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi == null)
                {
                    return null;
                }
                assemblyFilter = mi.MakeDelegate<AssemblyFilterOutDelegate>();
                return assemblyFilter;
            }
        }

        /// <summary>
        /// Basic filter that takes only project's assembly (ignoring unity's dlls and plugins')
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool OnlyProject(Assembly assembly)
        {
            bool exclude = !assembly.FullName.StartsWith("Assembly-CSharp")
                        && !assembly.FullName.Contains("OSG");
            return exclude;
        }

        public static bool OnlyProjectAndUnity(Assembly assembly)
        {
            bool exclude = !assembly.FullName.StartsWith("Assembly-CSharp")
                && !assembly.FullName.StartsWith("Unity")
                && !assembly.FullName.Contains("OSG");
            return exclude;
        }


    }
}