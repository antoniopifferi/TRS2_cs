module;                              // global module fragment (for legacy includes)
#include "src/gui/AppLogger.h"

#include <memory>
#include <vector>
#include <cmath>
#include <cstdlib>
#include <thread>

export module Spc;

import Const;
import Globals;
import Data;

export
{
    class Spc {
    public:
        // Factory (implemented in SpcFactory.ixx)
        static std::unique_ptr<Spc> createSpc();

        virtual ~Spc() = default;

        // High-level API (device-agnostic orchestration)
        void init() {
            n = 0;
            initDev();
        }
        void close() {
            stop();
            closeDev();
        }

        void start(float seconds) {
            setTime(seconds);
            acquisition = std::jthread([this](std::stop_token st)
                {
                    runAcquire(st);
                });
            startDev();
        }

        void stop(void) {
            stopDev();
            acquisition.request_stop();
            if (acquisition.joinable()) acquisition.join();
        }

    protected:
        std::jthread acquisition;
        std::uint64_t n;
        std::vector<TYPE_DATA> Buffer;

        void runAcquire(std::stop_token st)
        {
            while (!st.stop_requested())
            {
                waitDev();
                getDev();

                if (n - D->consumed.load(std::memory_order_relaxed) >= std::uint64_t(D->NumAcq))
                    outText("WARNING: overwriting data\n");

                int next_acq = int(n % D->NumAcq);

                std::memcpy(D->Ring[next_acq], Buffer.data(), D->NumElem * sizeof(TYPE_DATA));

                // Publish completed acquisition
                D->produced.store(n + 1, std::memory_order_release);
                ++n;
            }
        }

        void setTime(float seconds) {
            // Keep parity with the C path that doubles the time unless we're explicitly waiting on SPC
            if (P.Wait.Type != WAIT_SPC) seconds *= 2.0f;
            setTimeDev(seconds);
        }

        // Device primitives to be implemented by subclasses
        virtual void initDev() = 0;
        virtual void closeDev() = 0;
        virtual void startDev() = 0;
        virtual void stopDev() = 0;
        virtual void setTimeDev(float seconds) = 0;
        virtual void waitDev() = 0;
        virtual void getDev() = 0;
    };

    std::unique_ptr<Spc> spc[1];

    void startSpc(double time) {
        spc[0]->start(time);
    }

    void stopSpc() {
        spc[0]->stop();
    }
}