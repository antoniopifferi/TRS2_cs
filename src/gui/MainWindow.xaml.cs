using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TRS2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private void RunOscilloscope(object sender, RoutedEventArgs e)
        {
            Out.Group2.Int2 = Set.Group1.Int1;
        }

        private void RunMeasure(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Measure started.", "TRS2", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
