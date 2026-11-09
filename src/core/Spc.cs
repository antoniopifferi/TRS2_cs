using System;
using System.Threading;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2 {
    public abstract class Spc {
        private readonly Data data = Run.data;
        public bool isTrash = false;
        private Thread? acquisition;
        private volatile bool running;

        protected Spc() { }

        public void Init() {
            Display.Text("initialising SPC " + Set.Spc.Type + "...",false);
            InitDev();
            Display.Text("SUCCESS");
        }

        public void Start(double seconds) {
            SetTimeDev(seconds);
            StartDev();
            running = true;
            acquisition = new Thread(Acquire) { IsBackground = true };
            acquisition.Start();
        }

        public void Stop() {
            running = false;
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
                WaitDev();
                if (!running) break;

                GetDev();

                long n = Volatile.Read(ref data.Produced);
                while (running && n - Volatile.Read(ref data.Consumed) >= data.NumAcq)
                    Thread.Yield();

                if (!running) break;
                StartDev();

                CopyDev(data.Ring((int)(n % data.NumAcq)));
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
        protected abstract void CopyDev(Span<TypeData> destination);
        protected abstract void ClearDev();
    }
}