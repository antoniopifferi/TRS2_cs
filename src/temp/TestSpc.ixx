module;                              // global module fragment
#include "src/gui/AppLogger.h"
#include <QEventLoop>
#include <QTimer>
#include <format>
#include <cmath>
#include <thread>

export module TestSpc;


import Spc;
import Globals;
import Data;
import Const;

using uint32 = std::uint32_t;

// Concrete device: TestSpc (software generator mirroring TestSpc.c)
export class TestSpc : public Spc {
public:
    TestSpc() { outText(std::string("TestSpc constructor")); }
    ~TestSpc() override { /*outText("TestSpc destructor")*/; }

protected:
    double secs;

    // initialize device (allocate buffers, start acquisition thread)
    void initDev() override {
        Buffer.resize(D->NumElem);
    }

    void closeDev() override { /* nothing */ }
    void startDev() override { /* nothing */ }
    void stopDev() override { /* nothing */ }

    void setTimeDev(float seconds) override {
        secs = seconds;
    }

    void waitDev() {
        const int ms = static_cast<int>(std::max(0.0, secs) * 1000.0);
        QEventLoop loop;
        QTimer::singleShot(ms, &loop, &QEventLoop::quit);
        loop.exec();
    }

    void getDev()  {
        const int numBins = static_cast<int>(P.Bins.Num);
        const int numDet = static_cast<int>(P.Num.Det);
        for (int id = 0; id < numDet; ++id) {
            const double mus = TEST_MUS / (P.Num.Det * P.Num.Board * (1 / 0.3)) * (1 + 2 * (id + 0 * P.Num.Det));
            const double mua = TEST_MUA / (P.Num.Det * P.Num.Board * (1 / 0.3)) * (1 + 2 * (id + 0 * P.Num.Det));
            const double r = TEST_RHO;
            const double v = TEST_V;
            std::vector<double> dataD(static_cast<std::size_t>(numBins), 0.0);
            double area = 0.0;
            for (int ib = 0; ib < numBins; ib++) {
                const double t = (ib + 0.5) * P.Spc.Factor;
                dataD[ib] = (pow(t, -5.0 / 2.0) / mus) * exp(-mua * v * t) * exp(-(3.0 * r * r * mus) / (4.0 * v * t));
            }
            for (int ib = 0; ib < numBins; ib++)
                area += dataD[ib];
            for (int ib = 0; ib < numBins; ib++) {
                const double timeA = (P.Contest.Function == CONTEST_OSC ? P.Spc.TimeO : P.Spc.TimeM);
                double value = (TEST_AREA * timeA / area * dataD[ib]);
                value *= (1 - TEST_NOISE + (2.0 * TEST_NOISE * rand()) / RAND_MAX);
                Buffer[static_cast<std::size_t>(ib + id * numBins)] = static_cast<uint32>(value);
            }
        }
    }
};

