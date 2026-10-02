using System.Windows;

namespace TRS2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new ParmPage());
        }

        private void OpenParmPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ParmPage());
        }

        private void OpenStepPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StepPage());
        }

        private void OpenDisplayPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DisplayPage());
        }

        private void RunOscilloscope(object sender, RoutedEventArgs e)
        {
            Out.Group2.Int2 = Set.Group1.Int1;
            Run.Oscill();
        }

        private void RunMeasure(object sender, RoutedEventArgs e)
        {
            Run.Measure();
        }
    }
}
