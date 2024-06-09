namespace MornSpreadSheet
{
    public sealed class MornSpreadSheet
    {
        private readonly MornSpreadSheetCell[,] cells;
        public readonly int rowCount;
        public readonly int colCount;

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
            rowCount = rows.Length;
            if (rowCount == 0)
            {
                MornSpreadSheetLogger.LogError("No data found");
                return;
            }

            colCount = rows[0].Split("\",\"").Length;
            if (colCount == 0)
            {
                MornSpreadSheetLogger.LogError("No data found");
                return;
            }

            cells = new MornSpreadSheetCell[rowCount, colCount];
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
            cells[rowCount - 1, colCount - 1] = new MornSpreadSheetCell(bottomValue.Substring(0, bottomValue.Length - 1));
        }

        /// <summary> row と col は1始まり </summary>
        public MornSpreadSheetCell Get(int row, int col)
        {
            if (row < 1 || row > rowCount || col < 1 || col > colCount)
            {
                MornSpreadSheetLogger.LogError($"Index out of range row: {row}, col: {col}");
                return null;
            }

            return cells[row - 1, col - 1];
        }

        /// <summary> row は1始まり </summary>
        public MornSpreadSheetCell[] GetRow(int row)
        {
            if (row < 1 || row > rowCount)
            {
                MornSpreadSheetLogger.LogError($"Index out of range row: {row}");
                return null;
            }

            var rowCells = new MornSpreadSheetCell[colCount];
            for (var i = 0; i < colCount; i++)
            {
                rowCells[i] = cells[row - 1, i];
            }

            return rowCells;
        }

        /// <summary> col は1始まり </summary>
        public MornSpreadSheetCell[] GetCol(int col)
        {
            if (col < 1 || col > colCount)
            {
                MornSpreadSheetLogger.LogError($"Index out of range col: {col}");
                return null;
            }

            var colCells = new MornSpreadSheetCell[rowCount];
            for (var i = 0; i < rowCount; i++)
            {
                colCells[i] = cells[i, col - 1];
            }

            return colCells;
        }
    }
}