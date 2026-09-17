using CommunityToolkit.Mvvm.ComponentModel;

namespace TRS2
{
    public static partial class Set
    {
        public static Group1Data Group1 { get; } = new();

        public static Group3Data Group3 { get; } = new();

        public partial class Group1Data : ObservableObject
        {
            [ObservableProperty]
            private int int1 = 1;

            [ObservableProperty]
            private double double1 = 1.1;

            [ObservableProperty]
            private string string1 = "Value 1";
        }

        public partial class Group3Data : ObservableObject
        {
            [ObservableProperty]
            private string string3 = "Value 3";
        }
    }
}
