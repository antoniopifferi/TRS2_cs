using ScottPlot.Plottables;
using System;
using System.Windows.Controls;

namespace TRS2
{
    public partial class DisplayPage : Page
    {
        private readonly double[] x = new double[Set.Spc.NumBins];
        private readonly double[] y = new double[Set.Spc.NumBins];
        private Scatter? points;
        private bool initialized;

        public DisplayPage()
        {
            InitializeComponent();
            Display.InitRequested += InitDisplay;
            Display.Updated += UpdatePlot;
            Display.TextRequested += AddText;
        }

        private void InitDisplay()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(InitDisplay);
                return;
            }

            if (initialized)
                return;

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = i * Set.Spc.BinWidth;
                y[i] = double.NaN;
            }

            points = PlotView.Plot.Add.ScatterPoints(x, y);
            points.Color = ScottPlot.Colors.Yellow;
            points.MarkerSize = 3;

            PlotView.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
            {
                MinorTickGenerator = new ScottPlot.TickGenerators.LogDecadeMinorTickGenerator(),
                IntegerTicksOnly = true,
                LabelFormatter = v => $"1E{v:0}"
            };

            PlotView.Plot.Axes.SetLimitsY(0, 6);
            PlotView.Plot.Axes.SetLimitsX(0, (x.Length - 1) * Set.Spc.BinWidth);

            PlotView.Plot.Title("Histogram");
            PlotView.Plot.XLabel("Time (ps)");
            PlotView.Plot.YLabel("Counts");

            PlotView.Plot.FigureBackground.Color = ScottPlot.Colors.White;
            PlotView.Plot.DataBackground.Color = ScottPlot.Colors.Black;
            PlotView.Plot.Axes.Color(ScottPlot.Colors.Black);

            PlotView.Plot.Grid.XAxisStyle.MajorLineStyle.Color = ScottPlot.Colors.White;
            PlotView.Plot.Grid.YAxisStyle.MajorLineStyle.Color = ScottPlot.Colors.White;
            PlotView.Plot.Grid.YAxisStyle.MajorLineStyle.Width = 1.5f;
            PlotView.Plot.Grid.YAxisStyle.MinorLineStyle.Color = ScottPlot.Colors.White.WithOpacity(.45);
            PlotView.Plot.Grid.YAxisStyle.MinorLineStyle.Width = 1;

            PlotView.Plot.Axes.Left.MajorTickStyle.Length = 8;
            PlotView.Plot.Axes.Left.MajorTickStyle.Width = 2;
            PlotView.Plot.Axes.Left.MinorTickStyle.Length = 4;
            PlotView.Plot.Axes.Left.MinorTickStyle.Width = 1;

            initialized = true;
            PlotView.Refresh();
        }

        private void UpdatePlot(uint[] data)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdatePlot(data));
                return;
            }

            if (!initialized)
                InitDisplay();

            lock (PlotView.Plot.Sync)
            {
                int n = Math.Min(data.Length, y.Length);

                for (int i = 0; i < n; i++)
                    y[i] = data[i] > 0 ? Math.Log10(data[i]) : double.NaN;

                for (int i = n; i < y.Length; i++)
                    y[i] = double.NaN;
            }

            PlotView.Refresh();
        }

        private void AddText(string text)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => AddText(text));
                return;
            }

            DisplayText.AppendText(text + Environment.NewLine);
            DisplayText.ScrollToEnd();
        }
    }
}