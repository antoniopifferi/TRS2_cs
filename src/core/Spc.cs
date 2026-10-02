using System;
using System.Threading;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2
{
    public abstract class Spc
    {
        private readonly Data data=Run.data;

        private Thread? acquisition;
        private volatile bool running;
        protected TypeData[] Buffer = [];

        protected Spc()
        {
        }

        public void Init()
        {
            Buffer = new TypeData[Set.Spc.NumBins * Set.Spc.NumDet * Set.Spc.NumBoard];
            InitDev();
        }

        public void Start(double seconds)
        {
            SetTimeDev(seconds);

            running = true;
            acquisition = new Thread(Acquire)
            {
                IsBackground = true
            };
            acquisition.Start();
        }

        public void Stop()
        {
            running = false;

            // StopDev should also unblock WaitDev if it is waiting on hardware.
            StopDev();

            acquisition?.Join();
            acquisition = null;
        }

        public void Close()
        {
            Stop();
            CloseDev();
        }

        private void Acquire()
        {
            while (running)
            {
                // One device acquisition.
                // For TestSpc this can be empty; for MultiHarp it can start a single shot.
                StartDev();
                WaitDev();

                if (!running)
                    break;

                GetDev();

                long n = Volatile.Read(ref data.Produced);

                // Ring full: wait until the consumer frees one slot.
                while (running &&
                       n - Volatile.Read(ref data.Consumed) >= data.NumAcq)
                {
                    Thread.Yield();
                }

                if (!running)
                    break;

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


//export
//{
//    class Spc {
//    public:
//        // Factory (implemented in SpcFactory.ixx)
//        static std::unique_ptr<Spc> createSpc();

//        virtual ~Spc() = default;

//        // High-level API (device-agnostic orchestration)
//        void init() {
//            n = 0;
//            initDev();
//        }
//        void close() {
//            stop();
//            closeDev();
//        }

//        void start(float seconds) {
//            setTime(seconds);
//            acquisition = std::jthread([this](std::stop_token st)
//                {
//                    runAcquire(st);
//                });
//            startDev();
//        }

//        void stop(void) {
//            stopDev();
//            acquisition.request_stop();
//            if (acquisition.joinable()) acquisition.join();
//        }

//    protected:
//        std::jthread acquisition;
//        std::uint64_t n;
//        std::vector<TYPE_DATA> Buffer;

//        void runAcquire(std::stop_token st)
//        {
//            while (!st.stop_requested())
//            {
//                waitDev();
//                getDev();

//                if (n - D->consumed.load(std::memory_order_relaxed) >= std::uint64_t(D->NumAcq))
//                    outText("WARNING: overwriting data\n");

//                int next_acq = int(n % D->NumAcq);

//                std::memcpy(D->Ring[next_acq], Buffer.data(), D->NumElem * sizeof(TYPE_DATA));

//                // Publish completed acquisition
//                D->produced.store(n + 1, std::memory_order_release);
//                ++n;
//            }
//        }

//        void setTime(float seconds) {
//            // Keep parity with the C path that doubles the time unless we're explicitly waiting on SPC
//            if (P.Wait.Type != WAIT_SPC) seconds *= 2.0f;
//            setTimeDev(seconds);
//        }

//        // Device primitives to be implemented by subclasses
//        virtual void initDev() = 0;
//        virtual void closeDev() = 0;
//        virtual void startDev() = 0;
//        virtual void stopDev() = 0;
//        virtual void setTimeDev(float seconds) = 0;
//        virtual void waitDev() = 0;
//        virtual void getDev() = 0;
//    };

//    std::unique_ptr<Spc> spc[1];

//    void startSpc(double time) {
//        spc[0]->start(time);
//    }

//    void stopSpc() {
//        spc[0]->stop();
//    }
//}