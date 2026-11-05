using System;
using System.Threading;
using System.Threading.Tasks; // NEW
using TypeData = System.UInt32; // 4 bytes

namespace TRS2
{
    public abstract class Spc
    {
        private readonly Data data = Run.data!;

        public bool isTrash = false;
        private Task? acquisition;        // CHANGED: was Thread?
        private volatile bool running;
        private bool initAttempted;       // NEW: allow cleanup after partial initialization
        private bool startAttempted;      // NEW: allow cleanup after partial start
        protected TypeData[] Buffer = [];

        protected Spc() { }

        public void Init()
        {
            Display.Text("initialising SPC " + Set.Spc.Type + "...", false);
            Buffer = new TypeData[Set.Spc.NumBins * Set.Spc.NumDet * Set.Spc.NumBoard];

            initAttempted = true;         // NEW
            InitDev();
            Display.Text("SUCCESS");
        }

        public void Start(double seconds)
        {
            if (startAttempted)          // NEW
                throw new InvalidOperationException("SPC is already started.");

            SetTimeDev(seconds);
            startAttempted = true;        // NEW: StopDev can undo a partially completed StartDev
            StartDev();

            running = true;
            acquisition = Workers.StartLongRunning(Acquire);  // CHANGED
            Volatile.Write(ref data.Producer, acquisition);      // NEW
        }

        public void Stop()
        {
            if (!startAttempted && acquisition is null)
                return;                  // NEW: safe after a previous Stop

            running = false;

            try
            {
                // Unblock WaitDev if the worker is waiting on the device.
                StopDev();
            }
            finally
            {
                try
                {
                    // CHANGED: join and rethrow any acquisition Task exception.
                    acquisition?.GetAwaiter().GetResult();
                }
                finally
                {
                    // NEW: always reset generic state, including after errors.
                    acquisition = null;
                    Volatile.Write(ref data.Producer, null);
                    startAttempted = false;
                    Volatile.Write(ref data.Consumed, Volatile.Read(ref data.Produced));
                }
            }
        }

        public void Close()
        {
            try
            {
                Stop();
            }
            finally
            {
                // NEW: attempt device cleanup even if Stop() throws.
                if (initAttempted)
                {
                    initAttempted = false;
                    CloseDev();
                }
            }
        }

        // UNCHANGED: the acquisition loop itself contains no try/catch.
        public void Acquire()
        {
            while (running)
            {
                WaitDev();

                if (!running) break;

                GetDev();

                long n = Volatile.Read(ref data.Produced);

                while (running && n - Volatile.Read(ref data.Consumed) >= data.NumAcq)
                {
                    Thread.Yield();
                }

                if (!running) break;

                Buffer.AsSpan().CopyTo(data.Ring((int)(n % data.NumAcq)));
                Volatile.Write(ref data.Produced, n + 1);
            }
        }

        // UNCHANGED: device-specific implementations need no exception handlers.
        protected abstract void InitDev();
        protected abstract void CloseDev();
        protected abstract void StartDev();
        protected abstract void StopDev();
        protected abstract void SetTimeDev(double seconds);
        protected abstract void WaitDev();
        protected abstract void GetDev();
    }
}
