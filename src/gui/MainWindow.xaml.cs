using System.Windows;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;


namespace TRS2
{
    public partial class MainWindow : Window
    {
        public ParmPage ParmPage;
        public StepPage StepPage;
        public TrimPage TrimPage;
        public DisplayPage DisplayPage;

        public MainWindow()
        {
            InitializeComponent();

            ParmPage = new();
            StepPage = new();
            TrimPage = new();
            DisplayPage = new();

            // IMPORTANT: Connect pages to the global Set object
            ParmPage.DataContext = Set;
            StepPage.DataContext = Set;
            TrimPage.DataContext = Set;

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

        private void OpenTrimPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(TrimPage);
        }

        private void OpenDisplayPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(DisplayPage);
        }

        private void SaveSet(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(Konst.FILE_SET,JsonSerializer.Serialize(Set,new JsonSerializerOptions { WriteIndented = true }));
        }

        private void LoadSet(object sender, RoutedEventArgs e)
        {
            Set = JsonSerializer.Deserialize<SetData>(File.ReadAllText(Konst.FILE_SET)) ?? throw new InvalidDataException("Invalid settings file.");

            ParmPage.DataContext = Set;
            StepPage.DataContext = Set;
            TrimPage.DataContext = Set;
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