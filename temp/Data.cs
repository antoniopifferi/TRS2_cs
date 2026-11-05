using System;
using System.Threading;
using System.Threading.Tasks; // NEW
using TypeData = System.UInt32; // 4 bytes

namespace TRS2
{
    public sealed class Data
    {
        private readonly TypeData[] ringData;
        public readonly TypeData[] Archive;
        public readonly TypeData[] Temp;

        public Memory<TypeData> Slice;

        public readonly int NumBins;
        public readonly int NumPages;
        public readonly int NumElem;
        public readonly int NumAcq;
        public readonly int NumSlices;

        public long Produced;
        public long Consumed;

        // NEW: the task that supplies ring-buffer data (written by Spc.Start/Stop).
        public Task? Producer;

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

        public Span<TypeData> Ring(int slot) => ringData.AsSpan(slot * NumElem, NumElem);

        public void CopyArchive(int slice)
        {
            Slice = Archive.AsMemory(slice * NumElem, NumElem);
            CopyNext(Slice);
        }

        public void CopyTemp()
        {
            Slice = Temp.AsMemory();
            CopyNext(Slice);
        }

        private void CopyNext(Memory<TypeData> destination)
        {
            long n = Volatile.Read(ref Consumed);

            // NEW: detect failure (or missing producer) instead of hanging.
            Workers.ThrowIfFailed();
            Task? producer = Volatile.Read(ref Producer);
            if (producer is null)
                throw new InvalidOperationException("No active SPC acquisition task.");

            while (n >= Volatile.Read(ref Produced))
            {
                // CHANGED: check on every iteration, no arbitrary '63' counter.
                Workers.ThrowIfFailed();

                if (producer.IsCompleted)
                {
                    producer.GetAwaiter().GetResult(); // Re-throw if it failed.
                    throw new InvalidOperationException(
                        "SPC acquisition ended without supplying another data buffer.");
                }

                Thread.Yield();
            }

            Workers.ThrowIfFailed(); // NEW: detect another worker's failure.
            Ring((int)(n % NumAcq)).CopyTo(destination.Span);
            Volatile.Write(ref Consumed, n + 1);
        }
    }
}
