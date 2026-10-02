namespace TRS2
{
    public static class Set
    {
        public static Group1Data Group1 { get; } = new();
        public static Group3Data Group3 { get; } = new();
        public static SpcData Spc { get; } = new();

        // The array and every LoopData element are created once at startup.
        public static LoopData[] Loop { get; } = CreateLoops();

        private static LoopData[] CreateLoops()
        {
            var loops = new LoopData[Konst.MAX_LOOP];

            for (int i = 0; i < loops.Length; i++)
                loops[i] = new LoopData(i + 1);

            return loops;
        }

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

        public class SpcData
        {
            public int Int1 { get; set; } = 1;
            public string Type { get; set; } = "NONE";
            public string Wait { get; set; } = "SPC";
            public double TimeMeas { get; set; } = 1.0;
            public double TimeOscill { get; set; } = 1.0;
            public int NumBins { get; set; } = 4096;
            public double BinWidth { get; set; } = 10.0;
            public int NumDet { get; set; } = 1;
            public int NumBoard { get; set; } = 1;
        }

        public class LoopData
        {
            public LoopData(int number)
            {
                Number = number;
            }

            // Used only as a convenient row identifier in a DataGrid.
            public int Number { get; }

            public int Home { get; set; }
            public int First { get; set; }
            public int Last { get; set; }
            public int Delta { get; set; } = 1;
            public int Num { get; set; } = 1;
            public int Actual { get; set; }
            public string FileBreak { get; set; } = string.Empty;
            public bool Break { get; set; }
            public bool Invert { get; set; }
            public string Cont { get; set; } = "NONE";
        }
    }
}
