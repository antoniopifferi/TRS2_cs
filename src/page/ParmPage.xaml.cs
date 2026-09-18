using System.Collections.Generic;
using System.Windows.Controls;

namespace TRS2
{
    public partial class ParmPage : Page
    {
        public Set.Group1Data Group1 { get; } = Set.Group1;

        public Out.Group2Data Group2 { get; } = Out.Group2;

        public Set.LoopData[] Loop { get; } = Set.Loop;

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
    }
}