#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MornSpreadSheet
{
    [CustomEditor(typeof(MornSpreadSheetMaster))]
    internal sealed class MornSpreadSheetMasterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var master = (MornSpreadSheetMaster)target;
            if (GUILayout.Button("URLを開く"))
            {
                master.Open();
            }

            if (GUILayout.Button("GASコードをコピー"))
            {
                CopyGASCode();
            }

            if (GUILayout.Button("シート名を取得"))
            {
                MornSpreadSheetDownloader.UpdateSheetNamesWithProgressAsync(master).Forget();
            }

            if (GUILayout.Button("シートを更新"))
            {
                MornSpreadSheetDownloader.DownloadSheetsWithProgressAsync(master).Forget();
            }

            base.OnInspectorGUI();
        }

        private void CopyGASCode()
        {
            // SpreadSheetGAS.txtファイルを検索
            var guids = AssetDatabase.FindAssets("SpreadSheetGAS t:TextAsset");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("エラー", "SpreadSheetGAS.txtファイルが見つかりません。", "OK");
                return;
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (textAsset != null)
            {
                GUIUtility.systemCopyBuffer = textAsset.text;
                EditorUtility.DisplayDialog("成功", "GASコードをクリップボードにコピーしました。\nGoogle Apps Scriptエディタに貼り付けてください。", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("エラー", "SpreadSheetGAS.txtファイルの読み込みに失敗しました。", "OK");
            }
        }
    }
}
#endif