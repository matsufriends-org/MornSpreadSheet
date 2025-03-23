using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornSpreadSheet
{
    [Serializable]
    public sealed class MornSpreadSheet
    {
        [SerializeField] private string _sheetName;
        [SerializeField] private List<MornSpreadSheetRow> _rows;
        public int RowCount => _rows.Count;
        public int ColCount => _rows.Count > 0 ? _rows[0].CellCount : 0;

        public static bool TryConvert(string sheetName, string data, out MornSpreadSheet result)
        {
            /*
             * Memo
             * 1セルは "~" と記述される
             * 1行は "~","~","~" と記述される
             * 次の行がある場合は、末尾に"\n"が付与される
             *
             * セルの中に " や \n が含まれる場合もあるため
             * 隣接するセルのダブルクォーテーションを含む
             * "," や "\n" でsplitを行う
             */
            var rows = data.Split("\"\n\"");
            var rowCount = rows.Length;
            if (rowCount == 0)
            {
                MornSpreadSheetGlobal.LogError("データがありません。");
                result = null;
                return false;
            }

            var colCount = rows[0].Split("\",\"").Length;
            if (colCount == 0)
            {
                MornSpreadSheetGlobal.LogError("データがありません。");
                result = null;
                return false;
            }

            var cells = new MornSpreadSheetCell[rowCount, colCount];
            for (var i = 0; i < rowCount; i++)
            {
                var cols = rows[i].Split("\",\"");
                for (var j = 0; j < colCount; j++)
                {
                    cells[i, j] = new MornSpreadSheetCell(cols[j]);
                }
            }

            // 先頭のダブルクォーテーションを削除
            var topValue = cells[0, 0].AsString();
            cells[0, 0] = new MornSpreadSheetCell(topValue.Substring(1));

            // 末尾のダブルクォーテーションを削除
            var bottomValue = cells[rowCount - 1, colCount - 1].AsString();
            cells[rowCount - 1, colCount - 1] =
                new MornSpreadSheetCell(bottomValue.Substring(0, bottomValue.Length - 1));

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