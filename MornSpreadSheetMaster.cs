using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MornEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MornSpreadSheet
{
    [CreateAssetMenu(fileName = nameof(MornSpreadSheetMaster), menuName = "Morn/" + nameof(MornSpreadSheetMaster))]
    public sealed class MornSpreadSheetMaster : ScriptableObject
    {
        [SerializeField] private string _sheetId;
        [SerializeField] private string _getSheetNameApiUrl;
        [SerializeField] private List<string> _sheetNames;
        [SerializeField] private List<MornSpreadSheet> _sheets;
        internal bool IsLoading;
        public IEnumerable<MornSpreadSheet> Sheets => _sheets;

        public void Open()
        {
            var url = $"https://docs.google.com/spreadsheets/d/{_sheetId}/edit";
            Application.OpenURL(url);
        }

        public async UniTask UpdateSheetNamesAsync(CancellationToken ct = default)
        {
            if (IsLoading)
            {
                MornSpreadSheetGlobal.LogWarning("すでにタスクを実行中です");
                return;
            }

            IsLoading = true;
            MornSpreadSheetGlobal.Log("<size=30>タスク開始</size>");
            var sheetNames = await MornSpreadSheetUtil.LoadSheetNamesAsync(_getSheetNameApiUrl, ct);
            _sheetNames.Clear();
            _sheetNames.AddRange(sheetNames);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            MornSpreadSheetGlobal.Log("<size=30>タスク終了</size>");
            IsLoading = false;
        }

        public async UniTask DownloadSheetAsync(CancellationToken ct = default)
        {
            if (IsLoading)
            {
                MornSpreadSheetGlobal.LogWarning("すでにタスクを実行中です");
                return;
            }

            IsLoading = true;
            MornSpreadSheetGlobal.Log("<size=30>タスク開始</size>");
            _sheets.Clear();
            foreach (var sheetName in _sheetNames)
            {
                var sheet = await MornSpreadSheetUtil.LoadSheetAsync(_sheetId, sheetName, ct);
                if (sheet != null)
                {
                    _sheets.Add(sheet);
                }
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            MornSpreadSheetGlobal.Log("<size=30>タスク終了</size>");
            IsLoading = false;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(MornSpreadSheetMaster))]
    public sealed class MornSpreadSheetMasterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var master = (MornSpreadSheetMaster)target;
            MornEditorUtil.Draw(
                new MornEditorUtil.MornEditorOption
                {
                    IsEnabled = true,
                    IsBox = true,
                    IsIndent = true,
                    Color = master.IsLoading ? Color.red : null,
                    Header = "ステータス: " + (master.IsLoading ? "タスク実行中" : "待機"),
                },
                () =>
                {
                    if (GUILayout.Button("URLを開く"))
                    {
                        master.Open();
                    }

                    MornEditorUtil.Draw(
                        new MornEditorUtil.MornEditorOption
                        {
                            IsEnabled = !master.IsLoading
                        },
                        () =>
                        {
                            if (GUILayout.Button("シート名を取得"))
                            {
                                master.UpdateSheetNamesAsync().Forget();
                            }

                            if (GUILayout.Button("シートを更新"))
                            {
                                master.DownloadSheetAsync().Forget();
                            }
                        });
                    MornEditorUtil.Draw(
                        new MornEditorUtil.MornEditorOption
                        {
                            IsEnabled = master.IsLoading
                        },
                        () =>
                        {
                            if (GUILayout.Button("強制解除"))
                            {
                                master.IsLoading = false;
                            }
                        });
                });
            base.OnInspectorGUI();
        }
    }
#endif
}