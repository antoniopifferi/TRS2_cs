using System;
using System.Threading;
using TypeData = System.UInt32; // 4 bytes


namespace TRS2
{
    public sealed class Data
    {
        private readonly TypeData[] ringData;
        private readonly TypeData[] archiveData;
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

            ringData = new uint[numAcq * numElem];
            archiveData = new uint[numSlice * numElem];
            Temp = new uint[numElem];
        }

        public Span<TypeData> Ring(int slot) =>
            ringData.AsSpan(slot * NumElem, NumElem);

        public Span<TypeData> Archive(int slice) =>
            archiveData.AsSpan(slice * NumElem, NumElem);

        public void CopyArchive(int slice)
        {
            long n = Volatile.Read(ref Consumed);

            while (n >= Volatile.Read(ref Produced))
                Thread.Yield();

            Ring((int)(n % NumAcq)).CopyTo(Archive(slice));

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
