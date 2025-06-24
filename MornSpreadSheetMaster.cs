using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MornSpreadSheet
{
    [CreateAssetMenu(fileName = nameof(MornSpreadSheetMaster), menuName = "Morn/" + nameof(MornSpreadSheetMaster))]
    public sealed class MornSpreadSheetMaster : ScriptableObject
    {
        [SerializeField] private string _sheetId;
        [SerializeField] private string _getSheetNameApiUrl;
        [SerializeField] private List<string> _sheetNames;
        [SerializeField] private List<MornSpreadSheet> _sheets;
        public IEnumerable<MornSpreadSheet> Sheets => _sheets;
        public string SheetId => _sheetId;
        public IReadOnlyList<string> SheetNames => _sheetNames;
        internal string GetSheetNameApiUrl => _getSheetNameApiUrl;

        public void Open()
        {
            var url = $"https://docs.google.com/spreadsheets/d/{_sheetId}/edit";
            Application.OpenURL(url);
        }

        /// <summary>シート名リストを設定</summary>
        internal void SetSheetNames(IEnumerable<string> sheetNames)
        {
            _sheetNames.Clear();
            _sheetNames.AddRange(sheetNames);
            MornSpreadSheetGlobal.SetDirty(this);
        }

        /// <summary>シートリストを更新</summary>
        public void SetSheets(List<MornSpreadSheet> sheets)
        {
            _sheets = sheets;
            MornSpreadSheetGlobal.SetDirty(this);
        }

        public async UniTask UpdateSheetNamesWithProgressAsync()
        {
#if UNITY_EDITOR
            await MornSpreadSheetDownloader.UpdateSheetNamesWithProgressAsync(this);
#else
            await UniTask.CompletedTask;
#endif
        }

        public async UniTask DownloadSheetsWithProgressAsync()
        {
#if UNITY_EDITOR
            await MornSpreadSheetDownloader.DownloadSheetsWithProgressAsync(this);
#else
            await UniTask.CompletedTask;
#endif
        }
    }
}