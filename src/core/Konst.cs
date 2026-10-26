namespace TRS2 {
    public static class Konst {
        public const string AppName = "TRS2";
        public const int MAX_LOOP = 5;
        public const int MAX_STEP = 10;
        public const int MAX_TRIM = 10;
        public const int MAX_ROI = 32;
        public const double TEST_MUS = 10.0; // cm-1
        public const double TEST_MUA = 0.1; // cm-1
        public const double TEST_RHO = 1.0; // cm
        public const double TEST_V = 0.03; // cm/ps
        public const double TEST_AREA = 1000000;
        public const double TEST_NOISE = 0.1;
        public const int RAND_MAX = 32767;
        public const string FILE_SET = "TRS2.json";
        public const int HEAD_LEN = 256;
        public const int HEAD_VER_SUB_UNO = 1;
        public const int HEAD_VER_DUE = 2;
        public const int HEAD_VER_GEN = 0;
        public const int HEAD_IS_SUB = 0;
        public const int MAX_ACQ = 512; // max number of slots for acq (full slice) in ring array
        public const int MAX_CURVE = 16; // max number of curves in plot
        }
}
