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
        private const string PREFIX = "[MornLocalize] ";

        public static void Log(string message)
        {
            if (SHOW_LOG)
            {
                Debug.Log(PREFIX + message);
            }
        }

        public static void LogError(string message)
        {
            if (SHOW_LOG)
            {
                Debug.LogError(PREFIX + message);
            }
        }

        public static void LogWarning(string message)
        {
            if (SHOW_LOG)
            {
                Debug.LogWarning(PREFIX + message);
            }
        }
    }
}