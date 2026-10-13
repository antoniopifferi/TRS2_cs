using CommunityToolkit.Mvvm.ComponentModel;

namespace TRS2
{
    public partial class SetData
    {
        // members of Set
        public Group1Data Group1 { get; set; } = new();
        public Group3Data Group3 { get; set; } = new();
        public SpcData Spc { get; set; } = new();
        public LoopData[] Loop { get; set; } = CreateArray(Konst.MAX_LOOP, i => new LoopData(i + 1));
        public Step[] Steps { get; set; } = CreateArray(Konst.MAX_STEP, i => new Step(i + 1));
        public Trim[] Trims { get; set; } = CreateArray(Konst.MAX_TRIM, i => new Trim(i + 1));

        // definitions of classes used in Set

        public partial class Group1Data
        {
            public int Int1 { get; set; } = 1;
            public double Double1 { get; set; } = 1.1;
            public string String1 { get; set; } = "Value 1";
        }

        public partial class Group3Data
        {
            public string String3 { get; set; } = "Value 3";
        }

        public partial class SpcData
        {
            public string Type { get; set; } = "TEST";
            public string Wait { get; set; } = "SPC";
            public double TimeMeas { get; set; } = 1.0;
            public double TimeOscill { get; set; } = 1.0;
            public int NumBins { get; set; } = 4096;
            public double BinWidth { get; set; } = 1.0;
            public int NumDet { get; set; } = 1;
            public int NumBoard { get; set; } = 1;
        }

        public partial class LoopData : ObservableObject
        {
            public LoopData(int number){Number = number;} public int Number { get; }

            public int Home { get; set; }
            [ObservableProperty] private int first; // need to be lowercase for [ObservableProperty] to work
            [ObservableProperty] private int last; // need to be lowercase for [ObservableProperty] to work
            [ObservableProperty] private int delta = 1; // need to be lowercase for [ObservableProperty] to work
            public int Num => Math.Abs(Last - First) / Delta + 1;
            public int Actual { get; set; }
            public string FileBreak { get; set; } = string.Empty;
            public bool Break { get; set; }
            public bool Invert { get; set; }
            public string Cont { get; set; } = "NONE";

            // Needed for visualization.
            partial void OnFirstChanged(int value) => OnPropertyChanged(nameof(Num));
            partial void OnLastChanged(int value) => OnPropertyChanged(nameof(Num));
            partial void OnDeltaChanged(int value) => OnPropertyChanged(nameof(Num));
        }

        public partial class Step
        {
            public Step(int number) { Number = number; }

            public int Number { get; }
            public string Type { get; set; } = "NONE";
            public int Com { get; set; }
            public int Axis { get; set; } = 1;
            public string Mode { get; set; } = "SINGLE";
            public string Sign { get; set; } = "POS";
            public int Loop { get; set; } = -1;
            public string Hold { get; set; } = "OFF";
            public string Lcd { get; set; } = "OFF";
            public string FileName { get; set; } = string.Empty;
            public int MinHw { get; set; } = int.MinValue;
            public int MaxHw { get; set; } = int.MaxValue;
            public int FreqMin { get; set; }
            public int FreqMax { get; set; }
            public int Freq { get; set; }
            public int FreqDelta { get; set; }
            public double Factor { get; set; } = 1.0;
            public bool Sort { get; set; }
        }

        public partial class Trim
        {
            public Trim(int number) { Number = number; }

            public int Number { get; }
            public int Step { get; set; } = -1;
            public int Loop { get; set; } = -1;
            public string Type { get; set; } = "CONT";
            public string Wait { get; set; } = "SPC";
            public string Target { get; set; } = "AREA";
            public int LowCps { get; set; }
            public int GoalCps { get; set; }
            public int HighCps { get; set; }
            public string Sign { get; set; } = "HIGHER";
            public int Roi { get; set; } = -1;
            public double Fraction { get; set; }
            public double Time { get; set; }
            public double Delta { get; set; }
            public int Num { get; set; }
            public string Scan { get; set; } = "RANGE";
            public string OnBreak { get; set; } = "BEFORE";
            public bool Display { get; set; }
            public bool Status { get; set; }
            public bool Prompt { get; set; }
            public string FileName { get; set; } = string.Empty;
        }

        private static T[] CreateArray<T>(int length, Func<int, T> factory)
        {
            var array = new T[length];
            for (int i = 0; i < array.Length; i++) array[i] = factory(i);
            return array;
        }
    }
}
