using System;
using System.Reflection;

namespace OSG.Core
{
    /// <summary>
    /// Assembly exclusion filter
    /// </summary>
    /// <param name="assembly">The one that should be tested for exclusion</param>
    /// <returns>true if the assembly must be ignored by the AssemblyScanner</returns>
    public delegate bool AssemblyFilterOutDelegate(Assembly assembly);

    /// <summary>
    /// Process Type delegate with user data
    /// </summary>
    /// <param name="type"></param>
    /// <param name="userData"></param>
    public delegate void ProcessTypeUserDataDelegate(Type type, object userData);

    /// <summary>
    /// Process Type delegate
    /// </summary>
    /// <param name="type"></param>
    public delegate void ProcessTypeDelegate(Type type);

    internal class ScanParameters
    {
        private readonly AssemblyFilterOutDelegate m_assemblyFilterOutOut;
        private readonly ProcessTypeUserDataDelegate m_processTypeUserData;
        private readonly ProcessTypeDelegate m_processType;
        private readonly object m_userData;

        public ScanParameters(ProcessTypeUserDataDelegate processTypeUserData,
                              AssemblyFilterOutDelegate assemblyFilterOutOut,
                              object userData = null)
        {
            m_assemblyFilterOutOut = assemblyFilterOutOut;
            m_userData = userData;
            m_processTypeUserData = processTypeUserData;
            m_processType = null;
        }

        public ScanParameters(
            ProcessTypeDelegate processType,
            AssemblyFilterOutDelegate assemblyFilterOutOut)
        {
            m_processType = processType;
            m_assemblyFilterOutOut = assemblyFilterOutOut;
            m_userData = null;
            m_processTypeUserData = null;
        }

        public override string ToString()
        {
            return m_processType != null
                ? "Process method " + m_processType.Method.DeclaringType.Name + '.' + m_processType.Method.Name
                : "Process method " + m_processTypeUserData.Method.DeclaringType.Name + '.' + m_processTypeUserData.Method.Name;
        }

        public bool FilterOutAssembly(Assembly assembly)
        {
            return (m_assemblyFilterOutOut != null) && m_assemblyFilterOutOut.Invoke(assembly);
        }

        internal void Scan(Type type)
        {
            try
            {
                if (m_processType == null)
                {
                    m_processTypeUserData.Invoke(type, m_userData);
                }
                else
                {
                    m_processType.Invoke(type);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(this + " on type " + type + ":\n" + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}