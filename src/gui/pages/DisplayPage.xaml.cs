using ScottPlot.Plottables;
using System.Windows.Controls;

namespace TRS2
{
    public partial class DisplayPage : Page
    {
        public Signal Signal = null!;

        public DisplayPage()
        {
            InitializeComponent();

            Signal = PlotView.Plot.Add.Signal(new double[Set.Spc.NumBins]);
            Signal.Data.Period = Set.Spc.BinWidth;

            PlotView.Plot.Axes.SetLimitsY(0, 6);
            PlotView.Plot.Title("Histogram");
            PlotView.Plot.XLabel("Time (ps)");
            PlotView.Plot.YLabel("Counts (log10)");

            Display.Updated += UpdatePlot;
        }

        public void UpdatePlot(double[] values)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(() => UpdatePlot(values));
                return;
            }

            PlotView.Plot.Clear();

            Signal = PlotView.Plot.Add.Signal(values);
            Signal.Data.Period = Set.Spc.BinWidth;

            PlotView.Plot.Axes.SetLimitsY(0, 6);
            PlotView.Plot.Title("Histogram");
            PlotView.Plot.XLabel("Time (ps)");
            PlotView.Plot.YLabel("Counts");
            PlotView.Refresh();
        }

    }
}