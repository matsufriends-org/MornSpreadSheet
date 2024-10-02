namespace MornSpreadSheet
{
    public sealed class MornSpreadSheet
    {
        private readonly MornSpreadSheetCell[,] _cells;
        public readonly int RowCount;
        public readonly int ColCount;

        public MornSpreadSheet(string data)
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
            RowCount = rows.Length;
            if (RowCount == 0)
            {
                MornSpreadSheetLogger.LogError("No data found");
                return;
            }

            ColCount = rows[0].Split("\",\"").Length;
            if (ColCount == 0)
            {
                MornSpreadSheetLogger.LogError("No data found");
                return;
            }

            _cells = new MornSpreadSheetCell[RowCount, ColCount];
            for (var i = 0; i < RowCount; i++)
            {
                var cols = rows[i].Split("\",\"");
                for (var j = 0; j < ColCount; j++)
                {
                    _cells[i, j] = new MornSpreadSheetCell(cols[j]);
                }
            }

            // 先頭のダブルクォーテーションを削除
            var topValue = _cells[0, 0].AsString();
            _cells[0, 0] = new MornSpreadSheetCell(topValue.Substring(1));

            // 末尾のダブルクォーテーションを削除
            var bottomValue = _cells[RowCount - 1, ColCount - 1].AsString();
            _cells[RowCount - 1, ColCount - 1] = new MornSpreadSheetCell(bottomValue.Substring(0, bottomValue.Length - 1));
        }

        /// <summary> row と col は1始まり </summary>
        public MornSpreadSheetCell Get(int row, int col)
        {
            if (row < 1 || row > RowCount || col < 1 || col > ColCount)
            {
                MornSpreadSheetLogger.LogError($"Index out of range row: {row}, col: {col}");
                return null;
            }

            return _cells[row - 1, col - 1];
        }

        /// <summary> row は1始まり </summary>
        public MornSpreadSheetCell[] GetRow(int row)
        {
            if (row < 1 || row > RowCount)
            {
                MornSpreadSheetLogger.LogError($"Index out of range row: {row}");
                return null;
            }

            var rowCells = new MornSpreadSheetCell[ColCount];
            for (var i = 0; i < ColCount; i++)
            {
                rowCells[i] = _cells[row - 1, i];
            }

            return rowCells;
        }

        /// <summary> col は1始まり </summary>
        public MornSpreadSheetCell[] GetCol(int col)
        {
            if (col < 1 || col > ColCount)
            {
                MornSpreadSheetLogger.LogError($"Index out of range col: {col}");
                return null;
            }

            var colCells = new MornSpreadSheetCell[RowCount];
            for (var i = 0; i < RowCount; i++)
            {
                colCells[i] = _cells[i, col - 1];
            }

            return colCells;
        }
    }
}