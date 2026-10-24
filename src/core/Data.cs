using System;
using System.Threading;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2 {
    public sealed class Data {
        private readonly TypeData[] ringData;
        public readonly TypeData[] Archive;
        public readonly TypeData[] Temp;

        public Memory<TypeData> Slice;

        public readonly int NumBins; // number of bins for a single detector/histogram
        public readonly int NumPages; // number of detectors/histograms per acquisition
        public readonly int NumElem; // total number of bins for a single acquisition
        public readonly int NumAcq;   // number of acquisitions in the ring buffer
        public readonly int NumSlices;   // number of slices in the archive

        public long Produced;
        public long Consumed;

        public Data() {
            NumBins = Set.Spc.NumBins;
            NumPages = Set.Spc.NumDet * Set.Spc.NumBoard;
            NumElem = NumBins * NumPages;
            NumAcq = Konst.MAX_ACQ;
            NumSlices = 1;

            ringData = new TypeData[NumAcq * NumElem];
            Archive = new TypeData[NumSlices * NumElem];
            Temp = new TypeData[NumElem];
        }

        public Span<TypeData> Ring(int slot) => ringData.AsSpan(slot * NumElem, NumElem);

        public void CopyArchive(int slice) {
            Slice = Archive.AsMemory(slice * NumElem, NumElem);
            CopyNext(Slice);
        }

        public void CopyTemp() {
            Slice = Temp.AsMemory();
            CopyNext(Slice);
        }

        void CopyNext(Memory<TypeData> destination) {
            long n = Volatile.Read(ref Consumed);

            while (n >= Volatile.Read(ref Produced)) Thread.Yield();

            Ring((int)(n % NumAcq)).CopyTo(destination.Span);
            Volatile.Write(ref Consumed, n + 1);
            }
        }
}
