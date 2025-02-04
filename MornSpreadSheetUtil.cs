using UnityEngine;

namespace MornSpreadSheet
{
    internal static class MornSpreadSheetLogger
    {
#if DISABLE_MORN_SPREAD_SHEET_LOG
        private const bool SHOW_LOG = false;
#else
        private const bool SHOW_LOG = true;
#endif
        private static string Prefix => $"[<color=green>{nameof(MornSpreadSheet)}</color>] ";


        public static void Log(string message)
        {
            if (SHOW_LOG)
            {
                Debug.Log(Prefix + message);
            }
        }

        public static void LogError(string message)
        {
            if (SHOW_LOG)
            {
                Debug.LogError(Prefix + message);
            }
        }

        public static void LogWarning(string message)
        {
            if (SHOW_LOG)
            {
                Debug.LogWarning(Prefix + message);
            }
        }
    }
}