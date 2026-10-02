using CommunityToolkit.Mvvm.ComponentModel;

namespace TRS2
{
    public static partial class Set
    {
        // members of Set
        public static Group1Data Group1 { get; } = new();
        public static Group3Data Group3 { get; } = new();
        public static SpcData Spc { get; } = new();
        public static LoopData[] Loop { get; } = CreateArray(Konst.MAX_LOOP, i => new LoopData(i + 1));

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

        private static T[] CreateArray<T>(int length, Func<int, T> factory)
        {
            var array = new T[length];
            for (int i = 0; i < array.Length; i++) array[i] = factory(i);
            return array;
        }
    }
}
