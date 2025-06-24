using System.Runtime.CompilerServices;
using MornGlobal;
using UnityEngine;

[assembly: InternalsVisibleTo("MornSpreadSheet.Editor")]
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
        
        internal static void SetDirty(Object target)
        {
            I.SetDirtyInternal(target);
        }
    }
}