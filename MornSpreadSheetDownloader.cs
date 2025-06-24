#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MornSpreadSheet
{
    /// <summary>エディタ専用のスプレッドシートダウンロード機能</summary>
    public static class MornSpreadSheetDownloader
    {
        [Serializable]
        private class SheetNameList
        {
            public string[] Names;
        }

        /// <summary>スプレッドシートをロード</summary>
        public async static UniTask<MornSpreadSheet> LoadSheetAsync(string sheetId, string sheetName,
            CancellationToken cancellationToken = default)
        {
            var url = $"https://docs.google.com/spreadsheets/d/{sheetId}/gviz/tq?tqx=out:csv&sheet={sheetName}";
            return await LoadSheetFromUrlAsync(sheetName, url, cancellationToken);
        }

        private async static UniTask<MornSpreadSheet> LoadSheetFromUrlAsync(string sheetName, string url,
            CancellationToken cancellationToken = default)
        {
            MornSpreadSheetGlobal.Log($"ダウンロード開始:{url}");
            using var req = UnityWebRequest.Get(url);
            await req.SendWebRequest().WithCancellation(cancellationToken);
            if (req.result == UnityWebRequest.Result.Success)
            {
                var resultText = req.downloadHandler.text;
                MornSpreadSheetGlobal.Log($"ダウンロード成功:\n{resultText}");
                if (MornSpreadSheet.TryConvert(sheetName, resultText, out var result))
                {
                    return result;
                }

                MornSpreadSheetGlobal.LogError("変換失敗");
                return null;
            }

            MornSpreadSheetGlobal.LogError($"ダウンロード失敗:{req.error}");
            return null;
        }

        /// <summary>MornSpreadSheetMasterの進捗表示付きダウンロード</summary>
        public async static UniTask DownloadSheetsWithProgressAsync(MornSpreadSheetMaster master)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await UniTask.SwitchToMainThread();
                MornSpreadSheetGlobal.Log("<size=30>タスク開始</size>");
                var sheets = new List<MornSpreadSheet>();
                var totalCount = master.SheetNames.Count;
                for (var i = 0; i < totalCount; i++)
                {
                    var sheetName = master.SheetNames[i];
                    var progress = (float)i / totalCount;
                    var cancelled = EditorUtility.DisplayCancelableProgressBar(
                        "スプレッドシートダウンロード",
                        $"ダウンロード中: {sheetName} ({i + 1}/{totalCount})",
                        progress);
                    if (cancelled)
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }

                    // シートをダウンロード
                    var sheet = await LoadSheetAsync(
                        master.SheetId,
                        sheetName,
                        cancellationTokenSource.Token);
                    if (sheet != null)
                    {
                        sheets.Add(sheet);
                    }

                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }

                // 最後のシートの進捗を100%に
                EditorUtility.DisplayCancelableProgressBar("スプレッドシートダウンロード", "完了処理中...", 1f);

                // 内部メソッドを使って更新
                master.SetSheets(sheets);
                AssetDatabase.SaveAssets();
                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    MornSpreadSheetGlobal.Log("<size=30>タスク完了</size>");
                }
            }
            catch (OperationCanceledException)
            {
                MornSpreadSheetGlobal.Log("<size=30>タスクがキャンセルされました</size>");
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("エラー", $"エラーが発生しました: {ex.Message}", "OK");
                MornSpreadSheetGlobal.LogError($"タスク中にエラーが発生しました: {ex}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                cancellationTokenSource?.Dispose();
            }
        }

        /// <summary>プログレスバー付きでシート名を更新</summary>
        public async static UniTask UpdateSheetNamesWithProgressAsync(MornSpreadSheetMaster master)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await UniTask.SwitchToMainThread();
                MornSpreadSheetGlobal.Log("<size=30>タスク開始</size>");
                var result = new List<string>();
                if (string.IsNullOrEmpty(master.GetSheetNameApiUrl))
                {
                    MornSpreadSheetGlobal.LogWarning("シート名を取得するURLが設定されていません");
                    return;
                }

                using var request = UnityWebRequest.Get(master.GetSheetNameApiUrl);
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    var cancelled = EditorUtility.DisplayCancelableProgressBar(
                        "シート名取得",
                        "シート名を取得中...",
                        operation.progress);
                    if (cancelled)
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }

                    await UniTask.Yield(cancellationTokenSource.Token);
                }

                if (!cancellationTokenSource.Token.IsCancellationRequested
                    && request.result == UnityWebRequest.Result.Success)
                {
                    var json = request.downloadHandler.text;
                    var sheetNames = JsonUtility.FromJson<SheetNameList>("{\"Names\":" + json + "}").Names;
                    result.AddRange(sheetNames.Where(sheetName => !sheetName.StartsWith("#")));

                    // シート名を設定
                    master.SetSheetNames(result);
                    AssetDatabase.SaveAssets();
                    MornSpreadSheetGlobal.Log("<size=30>タスク終了</size>");
                }
                else if (request.result != UnityWebRequest.Result.Success)
                {
                    MornSpreadSheetGlobal.LogError($"シート名の取得失敗：{request.error}");
                }
            }
            catch (OperationCanceledException)
            {
                MornSpreadSheetGlobal.Log("シート名の取得がキャンセルされました");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                cancellationTokenSource?.Dispose();
            }
        }
    }
}
#endif