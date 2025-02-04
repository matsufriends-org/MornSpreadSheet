namespace MornSpreadSheet
{
    public sealed class MornSpreadSheetCell
    {
        private readonly string _value;

        public MornSpreadSheetCell(string value)
        {
            _value = value.Trim();
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