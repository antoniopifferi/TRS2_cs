using System.Collections.Generic;
using System.Windows.Controls;

namespace TRS2
{
    public partial class ParmPage : Page
    {
        public Set.Group1Data Group1 { get; } = Set.Group1;

        public Out.Group2Data Group2 { get; } = Out.Group2;

        public Set.LoopData[] Loop { get; } = Set.Loop;

        public IEnumerable<GroupRowDefinition> Group1Rows { get; } =
        [
            new("Int1", CellKind.Int, nameof(Set.Group1Data.Int1)),
            new("Double1", CellKind.Double, nameof(Set.Group1Data.Double1)),
            new("String1", CellKind.String, nameof(Set.Group1Data.String1))
        ];

        public IEnumerable<GroupRowDefinition> Group2Rows { get; } =
        [
            new("Int2", CellKind.Int, nameof(Out.Group2Data.Int2)),
            new("Double2", CellKind.Double, nameof(Out.Group2Data.Double2)),
            new("String2", CellKind.String, nameof(Out.Group2Data.String2))
        ];

        public IEnumerable<GroupRowDefinition> LoopRows { get; } =
        [
            new("Home", CellKind.Int, nameof(Set.LoopData.Home)),
            new("First", CellKind.Int, nameof(Set.LoopData.First)),
            new("Last", CellKind.Int, nameof(Set.LoopData.Last)),
            new("Delta", CellKind.Int, nameof(Set.LoopData.Delta)),
            new("Num", CellKind.Int, nameof(Set.LoopData.Num)),
            new("File break", CellKind.String, nameof(Set.LoopData.FileBreak)),
            new("Break", CellKind.Check, nameof(Set.LoopData.Break)),
            new("Invert", CellKind.Check, nameof(Set.LoopData.Invert)),
            new("Cont", CellKind.String, nameof(Set.LoopData.Cont))
        ];

        public ParmPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CheckCell_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
