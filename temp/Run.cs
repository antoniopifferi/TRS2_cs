using System;

namespace TRS2
{
    public static class Run
    {
        public static Data? data;
        private static Spc? spc;

        private static class Action
        {
            public static bool Oscill;
            public static bool moveStep;
            public static bool startSpc;
            public static bool stopSpc;
            public static bool copyArchive;
            public static bool copyTemp;
            public static bool displayPlot;
            public static bool Save;
        }

        // UNCHANGED
        private static void decideAction()
        {
            bool[] firstloop = new bool[Konst.MAX_LOOP];
            bool[] newloop = new bool[Konst.MAX_LOOP];
            bool[] lastloop = new bool[Konst.MAX_LOOP];
            for (int iL = 0; iL < Konst.MAX_LOOP; iL++)
            {
                firstloop[iL] = (Set.Loop[iL].Actual == 0);
                lastloop[iL] = (Set.Loop[iL].Actual == Set.Loop[iL].Num - 1);
                if (iL == 0) newloop[iL] = false;
                else newloop[iL] = firstloop[iL - 1];
            }
            Action.Oscill = false;
            Action.Save = Set.Store.Save;
            Action.startSpc = firstloop[0] && firstloop[1] && firstloop[2] && firstloop[3] && firstloop[4];
            Action.stopSpc = lastloop[0] && lastloop[1] && lastloop[2] && lastloop[3] && lastloop[4];
            Action.copyArchive = true;
            //Action.copyTemp = true;
            Action.displayPlot = true;
        }

        // UNCHANGED
        private static void loopGet(int loop)
        {
            int l = loop;
            for (int iL = 0; iL < Konst.MAX_LOOP; iL++)
            {
                if (Set.Loop[iL].Num > 0)
                {
                    Set.Loop[iL].Actual = l % Set.Loop[iL].Num;
                    l /= Set.Loop[iL].Num;
                }
                else
                {
                    Set.Loop[iL].Actual = 0;
                }
            }
        }

        public static void Oscill()
        {
            // NEW: avoid an infinite wait when called before device initialization.
            if (spc is null || data is null)
                throw new InvalidOperationException("Oscilloscope requires initialized SPC and Data.");

            Display.Text("Oscilloscope, press CONTINUE to go on");
            spc.Start(Set.Spc.TimeOscill);

            while (!Set.Display.Continue)
            {
                Workers.ThrowIfFailed(); // NEW
                data.CopyTemp();
                Display.LogPlot();
            }

            spc.Stop();
            Set.Display.Continue = false;
            spc.isTrash = true;
        }

        public static void Measure()
        {
            bool storeInitialized = false; // NEW

            // CHANGED: exactly one try/catch/finally in Measure().
            try
            {
                Workers.Reset(); // NEW
                data = new Data();

                spc = Set.Spc.Type switch
                {
                    "TEST" => new SpcTest(),
                    "SWAB" => new SpcSwab(),
                    "NONE" => null
                };
                spc?.Init();

                Display.Init();
                Store.Init();
                storeInitialized = true;

                // Original measurement workflow, unchanged.
                Oscill();
                int loop = 0;

                Display.Text("Measurement started");
                while (loop < Set.Loop[0].Num * Set.Loop[1].Num *
                              Set.Loop[2].Num * Set.Loop[3].Num * Set.Loop[4].Num)
                {
                    Workers.ThrowIfFailed(); // NEW: includes future Step/Trim tasks.

                    Display.Text("Loop " + loop + " started");
                    loopGet(loop);
                    decideAction();

                    if (Action.Oscill) Oscill();
                    if (Action.startSpc) spc?.Start(Set.Spc.TimeMeas);
                    if (Action.copyArchive) data.CopyArchive(0);
                    if (Action.displayPlot) Display.LogPlot();

                    // Future:
                    // Analysis.Process(data.Slice);
                    // Feedback.Process(data.Slice);

                    if (Action.Save) Store.Save();
                    if (Action.stopSpc) spc?.Stop();

                    loop++;
                }

                Workers.ThrowIfFailed(); // NEW: verify before reporting success.
                Display.Text("Measurement finished");
            }
            catch (Exception ex)
            {
                // NEW: single central error-reporting catch for Measure().
                Failure.Error("TRS2", "Measure", ex);
            }
            finally
            {
                // CHANGED: cleanup on success or failure; keep devices independent.
                Failure.CloseSafely("SPC", () => spc?.Close());
                if (storeInitialized)
                    Failure.CloseSafely("STORE", Store.Close);

                spc = null;
                Set.Display.Continue = false;
            }
        }
    }
}
