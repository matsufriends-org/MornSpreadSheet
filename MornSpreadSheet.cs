using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MornSpreadSheet
{
    [Serializable]
    public sealed class MornSpreadSheet
    {
        [SerializeField] private string _sheetName;
        [SerializeField] private List<MornSpreadSheetRow> _rows;
        public string SheetName => _sheetName;
        public int RowCount => _rows.Count;
        public int ColCount => _rows.Count > 0 ? _rows[0].CellCount : 0;

        public static bool TryConvert(string sheetName, string data, out MornSpreadSheet result)
        {
            /*
             * RFC 4180準拠のCSV解析
             * - フィールドはカンマで区切られる
             * - フィールドはダブルクォーテーションで囲まれることがある
             * - フィールド内のダブルクォーテーションは""でエスケープされる
             * - フィールド内に改行、カンマ、ダブルクォーテーションが含まれる場合は、
             *   フィールド全体をダブルクォーテーションで囲む必要がある
             */
            var parsedRows = ParseCsv(data);
            var rowCount = parsedRows.Count;
            if (rowCount == 0)
            {
                MornSpreadSheetGlobal.LogError("データがありません。");
                result = null;
                return false;
            }

            var colCount = parsedRows[0].Count;
            if (colCount == 0)
            {
                MornSpreadSheetGlobal.LogError("データがありません。");
                result = null;
                return false;
            }
            
            var cells = new MornSpreadSheetCell[rowCount, colCount];
            for (var i = 0; i < rowCount; i++)
            {
                for (var j = 0; j < colCount; j++)
                {
                    cells[i, j] = new MornSpreadSheetCell(parsedRows[i][j]);
                }
            }

            // 1行目の#以降は無視する
            for (var i = 0; i < colCount; i++)
            {
                var value = cells[0, i].AsString();
                var sharpIndex = value.IndexOf("#", StringComparison.Ordinal);
                if (sharpIndex > 0)
                {
                    var clampedValue = value[..sharpIndex];
                    clampedValue = clampedValue.Trim();
                    cells[0, i] = new MornSpreadSheetCell(clampedValue);
                }
            }
            
            // 1行目が#で始まる/空欄の場合はコメント行として無視する
            var ignoreColHashSet = new HashSet<int>();
            for (var i = 0; i < colCount; i++)
            {
                if (cells[0, i].AsString().StartsWith("#"))
                {
                    ignoreColHashSet.Add(i);
                }
                
                if (string.IsNullOrEmpty(cells[0, i].AsString()))
                {
                    ignoreColHashSet.Add(i);
                }
            }

            // 1列目が#で始まる場合はコメント列として無視する
            var ignoreRowHashSet = new HashSet<int>();
            for (var i = 0; i < rowCount; i++)
            {
                if (cells[i, 0].AsString().StartsWith("#"))
                {
                    ignoreRowHashSet.Add(i);
                }
            }
            
            // 全ての列が空白の場合は無視する
            for (var i = 0; i < rowCount; i++)
            {
                var allEmpty = true;
                for (var j = 0; j < colCount; j++)
                {
                    if (ignoreColHashSet.Contains(j))
                    {
                        continue;
                    }
                    
                    if (!string.IsNullOrWhiteSpace(cells[i, j].AsString()))
                    {
                        allEmpty = false;
                        break;
                    }
                }

                if (allEmpty)
                {
                    ignoreRowHashSet.Add(i);
                }
            }

            // 無視する行と列を除外する
            var newCells =
                new MornSpreadSheetCell[rowCount - ignoreRowHashSet.Count, colCount - ignoreColHashSet.Count];
            var newRow = 0;
            for (var i = 0; i < rowCount; i++)
            {
                if (ignoreRowHashSet.Contains(i))
                {
                    continue;
                }

                var newCol = 0;
                for (var j = 0; j < colCount; j++)
                {
                    if (ignoreColHashSet.Contains(j))
                    {
                        continue;
                    }

                    newCells[newRow, newCol] = cells[i, j];
                    newCol++;
                }

                newRow++;
            }

            cells = newCells;
            rowCount = cells.GetLength(0);
            colCount = cells.GetLength(1);
            result = new MornSpreadSheet
            {
                _sheetName = sheetName,
                _rows = new List<MornSpreadSheetRow>(rowCount),
            };
            for (var i = 0; i < rowCount; i++)
            {
                var row = new MornSpreadSheetRow();
                for (var j = 0; j < colCount; j++)
                {
                    row.AddCell(cells[i, j]);
                }

                result._rows.Add(row);
            }

            return true;
        }

        /// <summary>
        /// RFC 4180準拠のCSV解析
        /// </summary>
        private static List<List<string>> ParseCsv(string csvData)
        {
            var rows = new List<List<string>>();
            var currentRow = new List<string>();
            var currentField = new StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < csvData.Length)
            {
                var c = csvData[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // 次の文字も確認
                        if (i + 1 < csvData.Length && csvData[i + 1] == '"')
                        {
                            // エスケープされたダブルクォーテーション
                            currentField.Append('"');
                            i += 2;
                            continue;
                        }
                        else
                        {
                            // フィールドの終了
                            inQuotes = false;
                            i++;
                            continue;
                        }
                    }
                    else
                    {
                        // クォート内の通常の文字（改行も含む）
                        currentField.Append(c);
                        i++;
                        continue;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        // フィールドの開始
                        inQuotes = true;
                        i++;
                        continue;
                    }
                    else if (c == ',')
                    {
                        // フィールドの終了
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                        i++;
                        continue;
                    }
                    else if (c == '\n' || (c == '\r' && i + 1 < csvData.Length && csvData[i + 1] == '\n'))
                    {
                        // 行の終了
                        currentRow.Add(currentField.ToString());
                        rows.Add(new List<string>(currentRow));
                        currentRow.Clear();
                        currentField.Clear();
                        
                        if (c == '\r')
                        {
                            i += 2; // \r\nをスキップ
                        }
                        else
                        {
                            i++;
                        }
                        continue;
                    }
                    else if (c == '\r')
                    {
                        // 単独の\r（古いMac形式）
                        currentRow.Add(currentField.ToString());
                        rows.Add(new List<string>(currentRow));
                        currentRow.Clear();
                        currentField.Clear();
                        i++;
                        continue;
                    }
                    else
                    {
                        // 通常の文字
                        currentField.Append(c);
                        i++;
                        continue;
                    }
                }
            }

            // 最後のフィールドと行を追加
            if (currentField.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentField.ToString());
            }
            if (currentRow.Count > 0)
            {
                rows.Add(currentRow);
            }

            return rows;
        }

        public void Open(string sheetId)
        {
            var url = $"https://docs.google.com/spreadsheets/d/{sheetId}/edit#gid={_sheetName}";
            Application.OpenURL(url);
        }

        /// <summary> row と col は1始まり </summary>
        public MornSpreadSheetCell Get(int rowIdx, int colIdx)
        {
            if (rowIdx < 1 || rowIdx > _rows.Count)
            {
                MornSpreadSheetGlobal.LogError($"範囲外です。{rowIdx}/{_rows.Count}行");
                return default(MornSpreadSheetCell);
            }

            var row = _rows[rowIdx - 1];
            return row.GetCell(colIdx);
        }

        /// <summary> row は1始まり </summary>
        public MornSpreadSheetRow GetRow(int rowIdx)
        {
            if (rowIdx < 1 || rowIdx > _rows.Count)
            {
                MornSpreadSheetGlobal.LogError($"範囲外です。{rowIdx}/{_rows.Count}行");
                return null;
            }

            return _rows[rowIdx - 1];
        }

        /// <summary> row は1始まり </summary>
        public IEnumerable<MornSpreadSheetRow> GetRows()
        {
            foreach (var row in _rows)
            {
                yield return row;
            }
        }
    }
}