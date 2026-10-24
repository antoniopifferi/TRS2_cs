using System;

namespace TRS2
{
    public static class Display
    {
        internal static event Action? InitRequested;
        internal static event Action<ReadOnlyMemory<uint>>? Updated;
        internal static event Action<string>? TextRequested;

        public static void Init()
        {
            InitRequested?.Invoke();
        }

        public static void LogPlot()
        {
            Updated?.Invoke(Run.data!.Slice);
        }

        public static void Text(string textString)
        {
            TextRequested?.Invoke(textString);
        }
    }
}