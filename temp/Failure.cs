using System;

namespace TRS2
{
    // NEW: device-independent error display, like the original C error handler.
    public static class Failure
    {
        public static void Error(string device, string function, string message)
        {
            Display.Text($"ERROR [{device}] {function}: {message}");
        }

        public static void Error(string device, string function, Exception ex)
        {
            Error(device, function, ex.Message);
        }

        public static void Warning(string device, string function, string message)
        {
            Display.Text($"WARNING [{device}] {function}: {message}");
        }

        // NEW: cleanup is best-effort; a failing Close must not skip the next Close.
        // The ONLY extra catch is here, in general infrastructure, not device code.
        public static void CloseSafely(string device, Action close)
        {
            try
            {
                close();
            }
            catch (Exception ex)
            {
                Error(device, "Close", ex);
            }
        }
    }
}
