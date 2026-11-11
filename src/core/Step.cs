using System;
using System.Threading;

namespace TRS2 {
    public abstract class Step {
        protected readonly int iS;
        private readonly object io = new();
        private readonly AutoResetEvent command = new(false);
        private readonly ManualResetEventSlim finished = new(true);
        private Thread? worker;
        private volatile bool moving;
        private volatile bool stopping;
        private volatile bool closing;
        private Exception? error;

        public bool Moving => moving;
        public long Goal { get; private set; }
        public long Delta { get; private set; }
        public double FreqActual { get; private set; }
        public long Actual { get { lock (io) return TellPosDev(); } }

        //public static int NumSteps;

        protected Step(int index) { iS = index; }

        public void Init() {
            Display.Text($"initialising STEP {iS + 1} ({Set.Step[iS].Type})...", false);
            int loop = Set.Step[iS].Loop;
            if (loop >= 0 && loop < Set.Loop.Length)
                Delta = (long)(Set.Step[iS].Factor * Set.Loop[loop].Delta);

            lock (io) InitDev();
            if (Set.Step[iS].Mode == "CONT" && loop >= 0 && loop < Set.Loop.Length && Set.Spc.TimeMeas > 0)
                SetVel((double)Delta / (Set.Spc.TimeMeas * Set.Loop[loop].Num));
            else
                SetVel(Set.Step[iS].Freq);

            worker = new Thread(Work) { IsBackground = true };
            worker.Start();
            Display.Text("SUCCESS");
        }

        public void Move(long goal, bool wait = true) {
            Wait();  // only one movement at a time for this step
            goal = Math.Clamp(goal, Set.Step[iS].MinHw, Set.Step[iS].MaxHw);
            if (Actual == goal) return;

            Goal = goal;
            error = null;
            stopping = false;
            moving = true;
            finished.Reset();
            command.Set();
            Run.MarkSpcTrash();
            if (wait) Wait();
        }

        public void Wait() {
            finished.Wait();
            if (error != null) throw new InvalidOperationException($"STEP {iS + 1} movement failed", error);
        }

        public void Stop() {
            if (!moving) return;
            stopping = true;
            lock (io) StopDev();
            finished.Wait();
        }

        public void Close() {
            Stop();
            closing = true;
            command.Set();
            worker?.Join();
            lock (io) CloseDev();
            command.Dispose();
            finished.Dispose();
        }

        public void SetVel(double freq) {
            FreqActual = Math.Abs(freq);
            lock (io) SetVelDev(FreqActual);
        }

        public long CalcGoal() {
            int loop = Set.Step[iS].Loop;
            if (loop < 0 || loop >= Set.Loop.Length) return 0;
            var l = Set.Loop[loop];
            int index = Math.Clamp(l.Actual, 0, l.Num - 1);
            return (long)(Set.Step[iS].Factor * (l.First + (long)index * l.Delta));
        }

        private void Work() {
            while (true) {
                command.WaitOne();
                if (closing) return;
                try {
                    lock (io) {
                        if (!stopping) MoveDev(Goal);
                    }
                    while (!stopping && Actual != Goal)
                        Thread.Sleep(20);
                }
                catch (Exception ex) { error = ex; }
                finally {
                    moving = false;
                    finished.Set();
                }
            }
        }

        protected abstract void MoveDev(long goal);
        protected abstract long TellPosDev();
        protected virtual void InitDev() { }
        protected virtual void CloseDev() { }
        protected virtual void SetVelDev(double freq) { }
        protected virtual void StopDev() { }
    }
}
