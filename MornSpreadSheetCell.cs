﻿using System;
using UnityEngine;

namespace MornSpreadSheet
{
    [Serializable]
    public struct MornSpreadSheetCell
    {
        [SerializeField] private string _value;

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