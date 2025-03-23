using MornGlobal;

namespace MornSpreadSheet
{
    internal sealed class MornSpreadSheetGlobal : MornGlobalPureBase<MornSpreadSheetGlobal>
    {
        protected override string ModuleName => nameof(MornSpreadSheet);

        internal static void Log(string message)
        {
            I.LogInternal(message);
        }

        internal static void LogWarning(string message)
        {
            I.LogWarningInternal(message);
        }

        internal static void LogError(string message)
        {
            I.LogErrorInternal(message);
        }
    }
}