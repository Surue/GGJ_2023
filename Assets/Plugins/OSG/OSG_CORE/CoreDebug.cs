// Old Skull Games
// Pierre Planeau
// Monday, May 21, 2018


namespace OSG.Core
{
    public static class CoreDebug
    {
        // Automatically initialized to UnityEngine.Debug.Log in OSG DebugUtils!
        private static System.Action<string> LogFunction;
        private static System.Action<string> LogWarningFunction;
        private static System.Action<string> LogErrorFunction;

        /// <summary>
        /// Log function that can be used in the OSG.Core namespace.
        /// </summary>
        public static void Log(string logMsg)
        {
            LogFunction?.Invoke(logMsg);
        }

        /// <summary>
        /// LogWarning function that can be used in the OSG.Core namespace.
        /// </summary>
        public static void LogWarning(string logMsg)
        {
            LogWarningFunction?.Invoke(logMsg);
        }

        /// <summary>
        /// LogError function that can be used in the OSG.Core namespace.
        /// </summary>
        public static void LogError(string logMsg)
        {
            LogErrorFunction?.Invoke(logMsg);
        }

        public static void SetLogFunction(System.Action<string> logFunction)
        {
            LogFunction = logFunction;
        }

        public static void SetLogWarningFunction(System.Action<string> logFunction)
        {
            LogWarningFunction = logFunction;
        }

        public static void SetLogErrorFunction(System.Action<string> logFunction)
        {
            LogErrorFunction = logFunction;
        }
    }
}
