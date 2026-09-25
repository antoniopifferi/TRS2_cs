using System;
using System.Threading;
using TypeData = System.UInt32; // 4 bytes; UInt16 = 2 bytes

namespace TRS2
{

    public class Data(int numElem, int numSlice, int numAcq)
    {
        TypeData[] RingData = new TypeData[numAcq * numElem];
        TypeData[] ArchiveData = new TypeData[numSlice * numElem];
        TypeData[] Temp = new TypeData[numElem];
        public long Produced=0, Consumed=0, Dropped=0;

        public Span<TypeData> Ring(int slot) =>
            RingData.AsSpan(slot * numElem, numElem);

        public Span<TypeData> Archive(int slice) =>
            ArchiveData.AsSpan(slice * numElem, numElem);

        public bool CopyArchive(int slice)
        {
            long n = Volatile.Read(ref Consumed);
            if (n == Volatile.Read(ref Produced)) return false;

            Ring((int)(n % numAcq)).CopyTo(Archive(slice));
            Volatile.Write(ref Consumed, n + 1);
            return true;
        }

        public bool CopyTemp()
        {
            long n = Volatile.Read(ref Consumed);
            if (n == Volatile.Read(ref Produced)) return false;

            Ring((int)(n % numAcq)).CopyTo(Temp);
            Volatile.Write(ref Consumed, n + 1);
            return true;
        }
    }
}