﻿using System;

using G1ANT.Language;
using System.Text;

namespace G1ANT.Addon.Ocr.AbbyyFineReader
{
    [Structure(Name = "abbycell")]

    public class AbbyCellStructure : StructureTyped<CustomCell>
    {
        private const string TopIndex = "top";
        private const string BottomIndex = "bottom";
        private const string LeftIndex = "left";
        private const string BaseLineIndex = "right";
        private const string TextIndex = "text";
        

        public AbbyCellStructure(CustomCell cell) : this(cell, null, null)
        {
            Value = cell;
        }
        public AbbyCellStructure(object value, string format = null, AbstractScripter scripter = null)
            : base(value, format, scripter)
        {
            Indexes.Add(TopIndex);
            Indexes.Add(BottomIndex);
            Indexes.Add(LeftIndex);
            Indexes.Add(BaseLineIndex);
            Indexes.Add(TextIndex);
        }


        public override string ToString(string format = "")
        {
            return Value.ToString();
        }

        public override Structure Get(string index = null)
        {
            switch (index)
            {
                case null:
                    return this;
                case "top":
                    return new IntegerStructure(Value.Top);
                case "bottom":
                    return new IntegerStructure(Value.Bottom);
                case "left":
                    return new IntegerStructure(Value.Left);
                case "right":
                    return new IntegerStructure(Value.Right);
                case "baseline":
                    return new IntegerStructure(Value.BaseLine);
                case "text":
                    return new TextStructure(Value.Text);
            }
            throw new ArgumentOutOfRangeException($"There is no value under index = {index}");
        }

        public override void Set(Structure value, string index = null)
        {
            throw new NotSupportedException("This value is read-only");
        }
    }
}
