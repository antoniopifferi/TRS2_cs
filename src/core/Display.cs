namespace TRS2
{
    public static class Display
    {
        public static double[] Values = [];
        public static event Action<double[]>? Updated;

        public static void Init()
        {
            Values = new double[Set.Spc.NumBins];
        }

        public static void Plot(uint[] data)
        {
            Values = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
                if (data[i] > 0)
                    Values[i] = Math.Log10((double)data[i]);
                else
                    Values[i] = -0.1;

            Updated?.Invoke(Values);
        }
    }
}