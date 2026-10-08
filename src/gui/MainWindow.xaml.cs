using System.Windows;
using System.Threading.Tasks;


namespace TRS2
{
    public partial class MainWindow : Window
    {
        public ParmPage ParmPage;
        public StepPage StepPage;
        public DisplayPage DisplayPage;

        public MainWindow()
        {
            InitializeComponent();

            ParmPage = new();
            StepPage = new();
            DisplayPage = new();

            MainFrame.Navigate(ParmPage);
        }

        private void OpenParmPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(ParmPage);
        }

        private void OpenStepPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(StepPage);
        }

        private void OpenDisplayPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(DisplayPage);
        }

        private void RunOscilloscope(object sender, RoutedEventArgs e)
        {
            Out.Group2.Int2 = Set.Group1.Int1;
            Run.Oscill();
        }

        private async void RunMeasure(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(DisplayPage);
            await Task.Run(Run.Measure);
        }
    }
}