using ScottPlot.Plottables;
using System;
using System.Windows.Controls;

namespace TRS2
{
    public partial class DisplayPage : Page
    {
        private readonly double[] x = new double[Set.Spc.NumBins];
        private readonly double[][] y = new double[Konst.MAX_CURVE][];
        private readonly Scatter?[] points = new Scatter?[Konst.MAX_CURVE];

        private readonly ScottPlot.Color[] colors = [
            ScottPlot.Colors.Yellow,
            ScottPlot.Colors.Red,
            ScottPlot.Colors.Blue,
            ScottPlot.Colors.Green,
            ScottPlot.Colors.Orange,
            ScottPlot.Colors.Purple,
            ScottPlot.Colors.Cyan,
            ScottPlot.Colors.Magenta,
            ScottPlot.Colors.Lime,
            ScottPlot.Colors.Gold,
            ScottPlot.Colors.Pink,
            ScottPlot.Colors.Brown,
            ScottPlot.Colors.Teal,
            ScottPlot.Colors.Violet,
            ScottPlot.Colors.Coral,
            ScottPlot.Colors.SkyBlue
        ];

        private int numPages;
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

            if (initialized) return;

            numPages = Math.Min(Run.data!.NumPages, Konst.MAX_CURVE);

            for (int i = 0; i < x.Length; i++) x[i] = i * Set.Spc.BinWidth;

            for (int page = 0; page < numPages; page++)
            {
                y[page] = new double[x.Length];

                for (int i = 0; i < y[page].Length; i++) y[page][i] = double.NaN;

                points[page] = PlotView.Plot.Add.ScatterPoints(x, y[page]);
                points[page]!.Color = colors[page];
                points[page]!.MarkerSize = 3;
            }

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

        private void UpdatePlot(ReadOnlyMemory<uint> data)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdatePlot(data));
                return;
            }

            if (!initialized) InitDisplay();

            ReadOnlySpan<uint> values = data.Span;

            lock (PlotView.Plot.Sync)
            {
                for (int page = 0; page < numPages; page++)
                {
                    double[] pageY = y[page]!;
                    int offset = page * x.Length;

                    int count = offset < values.Length ? Math.Min(x.Length, values.Length - offset) : 0;

                    for (int i = 0; i < count; i++)
                    {
                        uint value = values[offset + i];
                        pageY[i] = value > 0 ? Math.Log10(value) : double.NaN;
                    }

                    for (int i = count; i < pageY.Length; i++) pageY[i] = double.NaN;
                }
            }

            PlotView.Refresh();
        }

        private void AddText(string text, bool appendNewLine = true)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => AddText(text, appendNewLine));
                return;
            }
            DisplayText.AppendText(appendNewLine ? text + Environment.NewLine : text);
            DisplayText.ScrollToEnd();
        }

        private void Continue(object sender, System.Windows.RoutedEventArgs e)
        {
            Set.Display.Continue = true;
        }
    }
}