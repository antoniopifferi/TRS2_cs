module;

#include <atomic>
#include <cstdint>
#include <cstring>
#include <thread>
#include <vector>
#include <mutex>
#include <memory>

export module Data;

import Globals;
import Const;

export
{
    using TYPE_DATA = std::uint32_t;

    class Data
    {
    public:

        int NumElem=P.Num.Board*P.Num.Det*P.Bins.Num;
        int NumAcq=128;
        int NumSlice=P.Loop[4].Num;

        std::vector<TYPE_DATA*> Ring;
        std::vector<TYPE_DATA*> Archive;
        std::vector<TYPE_DATA> Temp;

        std::vector<TYPE_DATA> RingData;
        std::vector<TYPE_DATA> ArchiveData;

        std::atomic<std::uint64_t> produced{ 0 };
        std::atomic<std::uint64_t> consumed{ 0 };

        Data(void)
        {
            // allocate contiguous storage
            RingData.resize(static_cast<size_t>(NumAcq) * static_cast<size_t>(NumElem));
            ArchiveData.resize(static_cast<size_t>(NumSlice) * static_cast<size_t>(NumElem));
            Temp.resize(static_cast<size_t>(NumElem));

            // allocate pointer tables
            Ring.resize(static_cast<size_t>(NumAcq));
            Archive.resize(static_cast<size_t>(NumSlice));

            // set pointers into the contiguous storage
            for (int i = 0; i < NumAcq; ++i)
                Ring[static_cast<size_t>(i)] = RingData.data() + static_cast<size_t>(i) * static_cast<size_t>(NumElem);

            for (int i = 0; i < NumSlice; ++i)
                Archive[static_cast<size_t>(i)] = ArchiveData.data() + static_cast<size_t>(i) * static_cast<size_t>(NumElem);
        }

        // Copy the next unread acquisition from the Ring buffer to the specified Archive slice.
        void copyArchive(int target_slice)
        {
            std::uint64_t n = consumed.load(std::memory_order_acquire);
            while (n >= produced.load(std::memory_order_acquire)) // Wait until an acquisition is available
                std::this_thread::yield();
            int first_acq = static_cast<int>(n % NumAcq);
            std::memcpy(Archive[target_slice], Ring[first_acq], NumElem * sizeof(TYPE_DATA));
            consumed.store(n + 1, std::memory_order_release); // This acquisition has now been consumed
        }

        // Copy the next unread acquisition from the Ring buffer to Temp
        void copyTemp()
        {
            std::uint64_t n = consumed.load(std::memory_order_acquire);
            while (n >= produced.load(std::memory_order_acquire)) // Wait until an acquisition is available
                std::this_thread::yield();
            int first_acq = static_cast<int>(n % NumAcq);
            std::memcpy(Temp.data(), Ring[first_acq], NumElem * sizeof(TYPE_DATA));
            consumed.store(n + 1, std::memory_order_release); // This acquisition has now been consumed
        }
    };

    // --------------------------------------------------------
    // Create Data
    // -------------------------------------------------------

    // Global unique pointer to Data instance (exported via the module export block)
    inline std::unique_ptr<Data> D;

    inline Data* initData(void)
    {
        D = std::make_unique<Data>();
        return D.get();
    }

    inline void closeData(void)
    {
        D.reset(); // free the Data instance
    }

} // End of export
