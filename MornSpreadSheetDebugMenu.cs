using System;
using System.Collections.Generic;
using MornDebug;

namespace MornSpreadSheet
{
    internal sealed class MornSpreadSheetDebugMenu : MornDebugMenuBase
    {
        public override IEnumerable<(string key, Action action)> GetMenuItems()
        {
            yield break;
        }
    }
}