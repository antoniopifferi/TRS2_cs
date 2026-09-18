using System.Collections.Generic;

namespace TRS2
{
    public static class Set
    {
        public static Group1Data Group1 = new();

        public static Group3Data Group3 = new();

        public static LoopData[] Loop = new LoopData[Konst.MAX_LOOP];

        public class Group1Data
        {
            public int Int1 { get; set; } = 1;

            public double Double1 { get; set; } = 1.1;

            public string String1 { get; set; } = "Value 1";
        }

        public class Group3Data
        {
            public string String3 { get; set; } = "Value 3";
        }

        public class LoopData
        {
            public int Home { get; set; }
            public int First { get; set; }
            public int Last { get; set; }
            public int Delta { get; set; } = 1;
            public int Num { get; set; } = 1;
            public string FileBreak { get; set; } = string.Empty;
            public bool Break { get; set; }
            public bool Invert { get; set; }
            public string Cont { get; set; } = "NONE";
        }
    }
}
