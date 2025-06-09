using System;
using System.Collections.Generic;
using UnityEngine;

namespace MornSpreadSheet
{
    [Serializable]
    public class MornSpreadSheetRow
    {
        [SerializeField] private List<MornSpreadSheetCell> _cells = new();
        public int CellCount => _cells.Count;

        public void AddCell(MornSpreadSheetCell cell)
        {
            _cells.Add(cell);
        }

        public MornSpreadSheetCell GetCell(int colIdx)
        {
            TryGetCell(colIdx, out var cell);
            return cell;
        }
        
        public bool TryGetCell(int colIdx, out MornSpreadSheetCell cell)
        {
            if (colIdx < 1 || colIdx > _cells.Count)
            {
                MornSpreadSheetGlobal.LogError($"範囲外です。{colIdx}{_cells.Count}列");
                cell = default(MornSpreadSheetCell);
                return false;
            }

            cell = _cells[colIdx - 1];
            return true;
        }

        public IEnumerable<MornSpreadSheetCell> GetCells()
        {
            return _cells;
        }

        public string AsString()
        {
            return string.Join(",", _cells.ConvertAll(cell => cell.AsString()));
        }
    }
}