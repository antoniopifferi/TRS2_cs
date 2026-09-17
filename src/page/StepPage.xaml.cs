using System.Windows.Controls;

namespace TRS2
{
    public partial class StepPage : Page
    {
        public Set.Group3Data Group3 { get; } = Set.Group3;

        public StepPage()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
