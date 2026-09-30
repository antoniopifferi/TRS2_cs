using System.Collections.Generic;
using System.Windows.Controls;

namespace TRS2
{
    public partial class StepPage : Page
    {
        public Set.Group3Data Group3 { get; } = Set.Group3;

        public IEnumerable<GroupRowDefinition> Group3Rows { get; } =
        [
            new("String3", CellKind.String, nameof(Set.Group3Data.String3))
        ];

        public StepPage()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
