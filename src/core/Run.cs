using System;
using System.Diagnostics.Metrics;
using System.Windows;

namespace TRS2
{
	public static class Run
	{
        private static Data? data;

        private static class Action
        {
            public static bool Oscill;
        }


        private static void initLoop()
        {
            for (int iL = 0; iL < Konst.MAX_LOOP; iL++) Set.Loop[iL].Num = (Set.Loop[iL].Last - Set.Loop[iL].First) / Set.Loop[iL].Delta + 1;
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
            MessageBox.Show("Running", "TRS2", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Measure()
        {
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

            initLoop();
            data= new Data(Set.Spc.NumBins, 128, 512);
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
                //    decideAction();
                //    if (P.Action.Oscill) Oscilloscope();
                //    if (P.Action.moveStep) moveStep();
                //    if (P.Action.startSpc) startSpc(P.Spc.TimeM);
                //    if (P.Action.copyArchive) copyArchive(loop);
                //    if (P.Action.displayPlot) displayPlot(loop);
                //    if (P.Action.saveData) saveData();

                //    QCoreApplication::processEvents();
                MessageBox.Show("Running", "TRS2", MessageBoxButton.OK, MessageBoxImage.Information);


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
