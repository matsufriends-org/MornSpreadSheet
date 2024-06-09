namespace MornSpreadSheet
{
    public sealed class MornSpreadSheetCell
    {
        private readonly string value;

        public MornSpreadSheetCell(string value)
        {
            this.value = value;
        }

        public string AsString()
        {
            return value;
        }

        public int AsInt()
        {
            return int.Parse(value);
        }

        public float AsFloat()
        {
            return float.Parse(value);
        }

        public bool AsBool()
        {
            return bool.Parse(value);
        }
    }
}