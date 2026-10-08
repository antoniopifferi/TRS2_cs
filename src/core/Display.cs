using System;

namespace TRS2
{
    public static class Display
    {
        internal static event Action? InitRequested;
        internal static event Action<uint[]>? Updated;
        internal static event Action<string>? TextRequested;

        public static void Init()
        {
            InitRequested?.Invoke();
        }

        public static void LogPlot(uint[] data)
        {
            Updated?.Invoke(data);
        }

        public static void Text(string TextString)
        {
            TextRequested?.Invoke(TextString);
        }
    }
}