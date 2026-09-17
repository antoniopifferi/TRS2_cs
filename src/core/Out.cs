using CommunityToolkit.Mvvm.ComponentModel;

namespace TRS2
{
    public static partial class Out
    {
        public static Group2Data Group2 = new();

        public partial class Group2Data : ObservableObject
        {
            [ObservableProperty]
            private int int2 = 2;

            [ObservableProperty]
            private double double2 = 2.2;

            [ObservableProperty]
            private string string2 = "Value 2";
        }
    }
}
