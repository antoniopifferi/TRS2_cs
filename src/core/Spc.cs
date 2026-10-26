using System;
using System.Threading;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2 {
    public abstract class Spc {
        private readonly Data data = Run.data;

        public bool isTrash = false;
        private Thread? acquisition;
        private volatile bool running;
        protected TypeData[] Buffer = [];

        protected Spc() { }

        public void Init() {
            Display.Text("initialising SPC " + Set.Spc.Type + "...");
            Buffer = new TypeData[Set.Spc.NumBins * Set.Spc.NumDet * Set.Spc.NumBoard];
            InitDev();
            Display.Text("SUCCESS");
        }

        public void Start(double seconds) {
            SetTimeDev(seconds);
            StartDev();

            running = true;
            acquisition = new Thread(Acquire) {
                IsBackground = true
            };

            //if (isTrash) {
            //    Volatile.Write(ref data.Consumed, Volatile.Read(ref data.Produced));
            //    isTrash = false;
            //}
            acquisition.Start();
        }

        public void Stop() {
            running = false;

            // StopDev should also unblock WaitDev if it is waiting on hardware.
            StopDev();
            acquisition?.Join();
            acquisition = null;
            Volatile.Write(ref data.Consumed, Volatile.Read(ref data.Produced));
        }

        public void Close() {
            Stop();
            CloseDev();
        }

        public void Acquire() {
            while (running) {
                // One device acquisition.
                // For TestSpc this can be empty; for MultiHarp it can start a single shot.
                //StartDev();
                WaitDev();

                if (!running) break;

                GetDev();

                long n = Volatile.Read(ref data.Produced);

                // Ring full: wait until the consumer frees one slot.
                while (running && n - Volatile.Read(ref data.Consumed) >= data.NumAcq) {
                    Thread.Yield();
                }

                if (!running) break;

                Buffer.AsSpan().CopyTo(data.Ring((int)(n % data.NumAcq)));

                // Publish only after the slot is completely written.
                Volatile.Write(ref data.Produced, n + 1);
            }
        }

        protected abstract void InitDev();
        protected abstract void CloseDev();
        protected abstract void StartDev();
        protected abstract void StopDev();
        protected abstract void SetTimeDev(double seconds);
        protected abstract void WaitDev();
        protected abstract void GetDev();

    }
}