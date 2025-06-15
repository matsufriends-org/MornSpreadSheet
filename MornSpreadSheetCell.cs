using System;
using UnityEngine;

namespace MornSpreadSheet
{
    [Serializable]
    public struct MornSpreadSheetCell
    {
        [SerializeField] private string _value;

        public MornSpreadSheetCell(string value)
        {
            // ParseCsvメソッドで既にエスケープ処理済みなので、そのまま保存
            // Trimは維持（前後の空白を削除）
            _value = value?.Trim() ?? string.Empty;
        }

        public string AsString()
        {
            return _value;
        }

        public int AsInt()
        {
            return int.Parse(_value);
        }

        public float AsFloat()
        {
            return float.Parse(_value);
        }

        public bool AsBool()
        {
            return bool.Parse(_value);
        }
    }
}