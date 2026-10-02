using System;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Diagnostics;

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
            public static bool saveData;
        }

        private static void decideAction()
        {
            bool[] firstloop = new bool[Konst.MAX_LOOP];
            bool[] newloop   = new bool[Konst.MAX_LOOP];
            bool[] lastloop  = new bool[Konst.MAX_LOOP];
    
            for (int iL = 0; iL < Konst.MAX_LOOP; iL++)
            {
                firstloop[iL] = (Set.Loop[iL].Actual == 0);
                lastloop[iL] = (Set.Loop[iL].Actual == Set.Loop[iL].Num - 1);
                if (iL == 0) newloop[iL] = false; else newloop[iL] = firstloop[iL - 1];
            }
            Action.Oscill = false;
            Action.saveData = lastloop[4];
            Action.startSpc = firstloop[4];
            Action.stopSpc = lastloop[4];
            Action.copyArchive = true;
            Action.copyTemp = true;
            Action.displayPlot = true;
        }

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
            //MessageBox.Show("Running", "TRS2", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Measure()
        {
            MessageBox.Show($"Loop4First=: {Set.Loop[4].First}", "TRS2", MessageBoxButton.OK, MessageBoxImage.Information);

            // GUI
            //P.Num.Board = 1;
            //P.Num.Det = 1;
            //P.Frame.Num = 1;
            //P.Contest.Function = CONTEST_OSC;
            //P.Action.Oscill = false;
            //P.Command.Abort = false;
            //P.Meas.Plot = 1;
            //P.Meas.Save = P.Loop[4].Num;

            //GUI->readAll();
            //GUI->displayPanel("Output");

            // initData();
            int numElem = Set.Spc.NumBoard * Set.Spc.NumDet * Set.Spc.NumBins;
            int NumAcq = 512;
            data = new Data(numElem,Set.Loop[4].Num,NumAcq);

            // initSpc();
            spc = Set.Spc.Type switch
            {
                "TEST" => new SpcTest(),
                "NONE" => null // No SPC device selected, do nothing
            };
            spc?.Init();



            //initSteps();
            //initSpc();
            //InitDataFile();

            //Oscilloscope();

            int loop = 0;
            //bool status = false;

            while (loop < Set.Loop[0].Num * Set.Loop[1].Num * Set.Loop[2].Num * Set.Loop[3].Num * Set.Loop[4].Num)
            //while (!P.Command.Abort && loop < Set.Loop[0].Num * Set.Loop[1].Num * Set.Loop[2].Num * Set.Loop[3].Num * Set.Loop[4].Num)
                {
                loopGet(loop);
                decideAction();
                //    if (P.Action.Oscill) Oscilloscope();
                //    if (P.Action.moveStep) moveStep();
                if (Action.startSpc) spc?.Start(Set.Spc.TimeMeas);
                if (Action.copyArchive) data.CopyArchive(loop);
                if (Action.copyTemp) data.CopyTemp();

                // print on screen the values in the array data.Temp in columns of 10 values per row
                for (int i = 0; i < data!.Temp.Length; i++)
                    {
                    Debug.Write($"{data.Temp[i],10}");
                    if ((i + 1) % 10 == 0 || i == data.Temp.Length - 1)
                        {
                        Debug.WriteLine(string.Empty);
                        }
                    }

                //    if (P.Action.displayPlot) displayPlot(loop);
                //    if (P.Action.saveData) saveData();

                // MessageBox showing the current loop number and data.Temp[100] value
                MessageBox.Show($"Current loop: {loop}, data.Temp[100]: {data.Temp[100]}", "TRS2", MessageBoxButton.OK, MessageBoxImage.Information);

                //MessageBox.Show("Running", "TRS2", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Action.stopSpc) spc.Stop();
                loop++;
            }

            //// Close file if needed
            //// stop all thread before closing Data

            //spc[0]->close();
            //CloseDataFile();
            //closeData();

            //GUI->displayPanel("Parm");

        }
    }
}
