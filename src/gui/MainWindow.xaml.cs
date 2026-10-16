using System.Windows;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Microsoft.Win32;


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
            LoadSetFile(Konst.FILE_SET);

            // IMPORTANT: Connect pages to the global Set object
            ParmPage.DataContext = Set;
            StepPage.DataContext = Set;
            TrimPage.DataContext = Set;

            MainFrame.Navigate(ParmPage);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveSetFile(Konst.FILE_SET);
            base.OnClosing(e);
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
            SaveFileDialog Dialog = new() { Filter = "JSON files (*.json)|*.json", DefaultExt = ".json", AddExtension = true };
            if (Dialog.ShowDialog() != true)
            {
                MessageBox.Show("No File Selected");
                return;
            }

            SaveSetFile(Dialog.FileName);
        }

        private void SaveSetFile(string FileName)
        {
            File.WriteAllText(FileName, JsonSerializer.Serialize(Set, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void LoadSet(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Dialog = new() { Filter = "JSON files (*.json)|*.json" };
            if (Dialog.ShowDialog() != true || !File.Exists(Dialog.FileName))
            {
                MessageBox.Show("No File Selected");
                return;
            }

            LoadSetFile(Dialog.FileName);
        }

        private void LoadSetFile(string FileName)
        {
            if (!File.Exists(FileName)) return;
            Set = JsonSerializer.Deserialize<SetData>(File.ReadAllText(FileName)) ?? throw new InvalidDataException("Invalid settings file.");
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