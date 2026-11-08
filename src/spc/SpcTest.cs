using System;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2 {
    public class SpcTest : Spc {
        private double _seconds;
        private TypeData[] _staging = [];
        private double[] _dataD = [];

        protected override void InitDev() {
            System.Diagnostics.Debug.WriteLine($"TEST NumDet={Set.Spc.NumDet}, NumBoard={Set.Spc.NumBoard}, NumPages={Run.data!.NumPages}");
            _staging = new TypeData[Set.Spc.NumBins * Set.Spc.NumDet * Set.Spc.NumBoard];
            _dataD = new double[Set.Spc.NumBins];
        }
        protected override void CloseDev() {
            // Cleanup logic for the test SPC device
        }
        protected override void StartDev() {
            // Start acquisition logic for the test SPC device
        }
        protected override void StopDev() {
            // Stop acquisition logic for the test SPC device
        }
        protected override void SetTimeDev(double seconds) {
            // Set acquisition time logic for the test SPC device
            _seconds = seconds;
        }
        protected override void WaitDev() {
            // wait for _seconds seconds
            System.Threading.Thread.Sleep((int)(_seconds * 1000));
        }
        protected override void GetDev() {
            int numBins = Set.Spc.NumBins;
            int numDet = Set.Spc.NumDet;
            int numBoard = Set.Spc.NumBoard;
            double binWidth = Set.Spc.BinWidth;

            for (int ibd = 0; ibd < numBoard; ibd++) {
                for (int id = 0; id < numDet; id++) {
                    double mus = Konst.TEST_MUS / (numBoard * (1 / 0.3)) * (1 + 2 * (id + ibd * numDet));
                    double mua = Konst.TEST_MUA / (numDet * (1 / 0.3)) * (1 + 2 * (id + ibd * numDet));
                    double r = Konst.TEST_RHO;
                    double v = Konst.TEST_V;
                    double area = 0.0;

                    for (int ib = 0; ib < numBins; ib++) {
                        double t = (ib + 0.5) * binWidth;
                        _dataD[ib] = Math.Pow(t, -5.0 / 2.0) / mus * Math.Exp(-mua * v * t) * Math.Exp(-(3.0 * r * r * mus) / (4.0 * v * t));
                        area += _dataD[ib];
                    }

                    int offset = ibd * numDet * numBins + id * numBins;
                    for (int ib = 0; ib < numBins; ib++) {
                        double value = Konst.TEST_AREA * _seconds / area * _dataD[ib];
                        value *= 1 - Konst.TEST_NOISE + Konst.TEST_NOISE * (2 * Random.Shared.NextDouble() - 1);
                        _staging[offset + ib] = (TypeData)value;
                    }
                }
            }
        }

        protected override void CopyDev(Span<TypeData> destination) {
            _staging.AsSpan().CopyTo(destination);
        }

        protected override void ClearDev()
        {
            _staging.AsSpan().Clear();
        }
    }
}
