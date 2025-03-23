using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MornSpreadSheet
{
    public static class MornSpreadSheetUtil
    {
        [Serializable]
        private class SheetNameList
        {
            public string[] Names;
        }

        public async static UniTask<IEnumerable<string>> LoadSheetNamesAsync(string getSheetNameApiUrl,
            CancellationToken ct = default)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(getSheetNameApiUrl))
            {
                MornSpreadSheetGlobal.LogWarning("シート名を取得するURLが設定されていません");
                return result;
            }

            using var request = UnityWebRequest.Get(getSheetNameApiUrl);
            await request.SendWebRequest().WithCancellation(ct);
            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = request.downloadHandler.text;
                var sheetNames = JsonUtility.FromJson<SheetNameList>("{\"Names\":" + json + "}").Names;
                result.AddRange(sheetNames.Where(sheetName => !sheetName.StartsWith("#")));
            }
            else
            {
                MornSpreadSheetGlobal.LogError($"シート名の取得失敗：{request.error}");
            }

            return result;
        }

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
    }
}