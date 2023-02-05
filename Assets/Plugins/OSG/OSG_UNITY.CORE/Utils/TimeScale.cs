//#define DEBUG_TIMEPAUSE

using System;
using System.Diagnostics;
using UnityEngine;

#if DEBUG_TIMEPAUSE
using System.Reflection;
using System.Text;
using System.Collections.Generic;
#endif

namespace OSG
{
    public static class TimeScale
    {
        [Conditional("DEBUG_TIMEPAUSE")]
        private static void AddHistory()
        {
#if DEBUG_TIMEPAUSE
        if (history == null)
        {
            history = new Queue<string>();
        }

        StackTrace trace = new StackTrace();
        var frame0 = trace.GetFrame(1);
        var frame1 = trace.GetFrame(2);

        MethodBase method = frame1.GetMethod();
        
        string name = method.ReflectedType == null ? " ?" : method.ReflectedType.Name;
        history.Enqueue(frame0.GetMethod().Name + " " + method.Name + " " + name + " count " + pauseCount + " scale " + Time.timeScale);

        if (history.Count > 10)
        {
            history.Dequeue();
        }
#endif
        }
#if DEBUG_TIMEPAUSE
    private static Queue<string> history;
    public static bool hasHistory = true;
    public static string GetHistory()
    {
        StringBuilder b = new StringBuilder();
        foreach (string s in history)
        {
            b.AppendLine(s);
        }
        return b.ToString();
    }
#else
        public static bool hasHistory = false;
        public static string GetHistory()
        {
            return String.Empty;
        }
#endif

        private static int pauseCount;
        private static float previousTimeScale;
        private static bool lerpStarted;

        public static void StartLerp()
        {
            AddHistory();

            if (lerpStarted)
            {
                throw new Exception("Already lerping "  + GetHistory());
            }

            if (pauseCount != 0)
                throw new Exception("Can't lerp in pause " + GetHistory());
        
            lerpStarted = true;
            SaveTimeScale();
        }

        public static void Lerp(float t)
        {
            if (!lerpStarted)
                throw new Exception("Lerp not started " + GetHistory());
            Set(Mathf.Lerp(0, previousTimeScale, t));
        }

        public static void EndLerp()
        {
            AddHistory();
            if (!lerpStarted)
            {
                throw new Exception("Wasn't Lerping! " + GetHistory());
            }
        
            lerpStarted = false;
            RestoreTimeScale();
        }

        public static void Pause()
        {
            AddHistory();
            if (pauseCount == 0)
            {
                SaveTimeScale();
                Set(0);
            }
            ++pauseCount;
        }

        private static void Set(float t)
        {
            Time.timeScale = t;
        }

        public static void Force(float scale)
        {
            if (scale <= 0)
            {
                AddHistory();
                throw new Exception("scale should not be forced to 0, use Pause "  + GetHistory());
            }
            Set(scale);
        }

        public static void Play()
        {
            AddHistory();
            --pauseCount;
            if (pauseCount == 0)
            {
                RestoreTimeScale();
            }
            if (pauseCount < 0)
                pauseCount = 0;
        }

        public static void Reset()
        {
            AddHistory();
            pauseCount = 0;
            previousTimeScale = 1;
            Set(1.0f);
        }

        private static void SaveTimeScale()
        {
            previousTimeScale = Time.timeScale > 0 ? Time.timeScale : 1;
        }

        private static void RestoreTimeScale()
        {
            Set(previousTimeScale > 0 ? previousTimeScale : 1);
        }

#region CHEATS

        [CheatFunction(CheatCategory.Time, false)]
        private static void SpeedUpTime()
        {
            if (Time.timeScale >= 1)
                Time.timeScale += 1;
            else
            {
                Time.timeScale += 0.1f;
            }

            CheatMessages.Append(ShowTimeScale());
        }

        private static string ShowTimeScale()
        {
            return ("SetTimeScale(x" + Time.timeScale.ToString("0.0") + ")").InColor(Color.red);
        }

        [CheatFunction(CheatCategory.Time, true, CustomMenuDisplay = "ShowTimeScale")]
        private static void SetTimeScale(float scale)
        {
            if (scale < 0.1f) scale = 0.1f;
            Time.timeScale = scale;
            ShowTimeScale();
        }

        [CheatFunction(CheatCategory.Time, false)]
        private static void SlowDownTime()
        {
            if (Time.timeScale > 1)
                Time.timeScale -= 1;
            else if (Time.timeScale > 0.1f)
                Time.timeScale -= 0.1f;

            CheatMessages.Append(ShowTimeScale());
        }


        [CheatFunction(CheatCategory.Time, false)]
        private static void PauseTime()
        {
            Pause();
            CheatMessages.Append(ShowTimeScale());
        }

        [CheatFunction(CheatCategory.Time, false)]
        private static void ResetTime()
        {
            Reset();
            CheatMessages.Append(ShowTimeScale());
        }

        [CheatFunction(CheatCategory.Time, true)]
        private static void ShowTimeHistory()
        {
            CheatMessages.Append(GetHistory());
        }

        #endregion
    }
}
