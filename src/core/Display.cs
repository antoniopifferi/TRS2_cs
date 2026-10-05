using System;

namespace TRS2
{
    public static class Display
    {
        internal static event Action? InitRequested;
        internal static event Action<uint[]>? Updated;

        public static void Init()
        {
            InitRequested?.Invoke();
        }

        public static void LogPlot(uint[] data)
        {
            Updated?.Invoke(data);
        }
    }
}
