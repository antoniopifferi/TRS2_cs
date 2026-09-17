using System.Windows.Controls;

namespace TRS2
{
    public partial class ParmPage : Page
    {
        public Set.Group1Data Group1 { get; } = Set.Group1;

        public Out.Group2Data Group2 { get; } = Out.Group2;

        public ParmPage()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
