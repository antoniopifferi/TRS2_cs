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

        public int NumElem { get; }
        public int NumSlice { get; }
        public int NumAcq { get; }

        // Written only by producer / consumer respectively.
        public long Produced;
        public long Consumed;

        public Data(int numElem, int numSlice, int numAcq)
        {
            NumElem = numElem;
            NumSlice = numSlice;
            NumAcq = numAcq;

            ringData = new TypeData[numAcq * numElem];
            Archive = new TypeData[numSlice * numElem];
            Temp = new TypeData[numElem];
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
        }
    }
}
