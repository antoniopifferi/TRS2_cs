using System;
using System.Threading;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2
{
    public sealed class Data
    {
        private readonly TypeData[] ringData;

        // Complete contiguous storage for all archive slices.
        public readonly TypeData[] Archive;

        public readonly TypeData[] Temp;


        public readonly int NumBins; // number of bins for a single detector/histogram
        public readonly int NumPages; // number of detectors/histograms per acquisition
        public readonly int NumElem; // total number of bins for a single acquisition
        public readonly int NumAcq;   // number of acquisitions in the ring buffer
        public readonly int NumSlices;   // number of slices in the archive


        // Written only by producer / consumer respectively.
        public long Produced;
        public long Consumed;

        public Data()
        {
            NumBins = Set.Spc.NumBins;
            NumPages = Set.Spc.NumDet * Set.Spc.NumBoard;
            NumElem = NumBins * NumPages;
            NumAcq = Konst.MAX_ACQ;
            NumSlices = 1;

            ringData = new TypeData[NumAcq * NumElem];
            Archive = new TypeData[NumSlices * NumElem];
            Temp = new TypeData[NumElem];
        }

        public Span<TypeData> Ring(int slot) =>
            ringData.AsSpan(slot * NumElem, NumElem);

        // One slice within the contiguous archive.
        public Span<TypeData> Slice(int slice) =>
            Archive.AsSpan(slice * NumElem, NumElem);

        public void CopyArchive(int slice)
        {
            long n = Volatile.Read(ref Consumed);
            while (n >= Volatile.Read(ref Produced))
                Thread.Yield();
            Ring((int)(n % NumAcq)).CopyTo(Slice(slice));
            Volatile.Write(ref Consumed, n + 1);
        }

        public void CopyTemp()
        {
            long n = Volatile.Read(ref Consumed);
            while (n >= Volatile.Read(ref Produced))
                Thread.Yield();
            Ring((int)(n % NumAcq)).CopyTo(Temp);
            Volatile.Write(ref Consumed, n + 1);

        }
    }
}
