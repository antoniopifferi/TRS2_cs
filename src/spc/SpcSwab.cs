using SwabianInstruments.TimeTagger;
using System;
using System.DirectoryServices;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Runtime.InteropServices;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2
{
    public class SpcSwab : Spc
    {
        public const int SWAB_MAX_CHANN = 18; // max number of Swabian channels
        public const double SWAB_REPLAY_SPEED = 1.0; // replay speed for Virtual file
        public const string SWAB_FILE_EXTENSION = "ttbin"; // file extension for Swabian Time Tagger files

        private Int64 _picoseconds=0;
        private bool _isFwRunning = false;
        private int[] _channelNumber = new int[SWAB_MAX_CHANN]; // Array to hold the channel numbers
        private TimeTaggerBase? _tagger=null; // TimeTaggerBase is the base class for both TimeTagger and TimeTaggerVirtual
        private TimeTagger? _taggerReal=null; // TimeTagger is the class for the real TimeTagger device
        private TimeTaggerVirtual? _taggerVirtual=null; // TimeTaggerVirtual is the class for the virtual TimeTagger device
        private int[] _signChannels; // Array to hold the signal channels
        private int[] _syncChannels; // Array to hold the sync channels
        private FrequencyMultiplier? _frequencyMultiplier=null;
        private int _freqMultChannel;
        private Histogram?[] _histogram = new Histogram[Set.Spc.NumDet];
        private Correlation?[] _correlation = new Correlation[Set.Spc.NumDet];
        private SynchronizedMeasurements? _syncMeas = null; // multiple syncronised measurements that starts together
        private FileWriter? _fileWriter = null;
        private Int64 _binwidth=(Int64)(Set.Spc.BinWidth+0.1);
        private int[][] _staging = []; // Staging buffer for data transfer [numDet][numBins]

        protected override void InitDev()
        {
            _signChannels = new int[Set.Spc.NumDet];
            _syncChannels = new int[1];
            int numSignChannels = 0;
            int numSyncChannels = 0;

            _staging = new int[Set.Spc.NumDet][]; // initialise the staging buffer for each detector
            
            // complete channel arrays based on Set.Swab.Setting
            for (int ic = 0; ic < SWAB_MAX_CHANN; ic++)
            {
                _channelNumber[ic] = Set.Swab.Setting[ic].Level > 0 ? ic + 1 : -(ic + 1); // Channels are 1-indexed and with sign
                if (Set.Swab.Setting[ic].Type == "SIGN")
                    _signChannels[numSignChannels++] = _channelNumber[ic];
                else if (Set.Swab.Setting[ic].Type == "SYNC")
                    _syncChannels[numSyncChannels++] = _channelNumber[ic];
            }

            // initialise Real Swabian Time Tagger
            if (Set.Swab.Type == "REAL")
            {
                _tagger = TT.createTimeTagger();
                _taggerReal = (TimeTagger)_tagger; // Cast to TimeTagger for real device
                Display.Text("Serial number: " + _taggerReal.getSerial() + "...", false);
                _taggerReal.reset();
                if (Set.Swab.FrequencyDivider > 1)
                    _taggerReal.setEventDivider(_syncChannels[0], Set.Swab.FrequencyDivider);
                if (Set.Swab.Filter)
                    if (Set.Swab.FilteredChannel == "SYNC")
                        _taggerReal.setConditionalFilter(_signChannels, _syncChannels);
                    else if (Set.Swab.FilteredChannel == "SIGN")
                        _taggerReal.setConditionalFilter(_syncChannels, _signChannels);
                for (int ic = 0; ic < SWAB_MAX_CHANN; ic++)
                    if (Set.Swab.Setting[ic].Type != "NONE")
                    {
                        _taggerReal.setTriggerLevel(_channelNumber[ic], Set.Swab.Setting[ic].Level);
                        _taggerReal.setInputDelay(_channelNumber[ic], Set.Swab.Setting[ic].Delay);
                        _taggerReal.setDeadtime(_channelNumber[ic], Set.Swab.Setting[ic].Deadtime);
                        if (Set.Swab.Setting[ic].Hysteresis > 0)
                            _taggerReal.setInputHysteresis(_channelNumber[ic], Set.Swab.Setting[ic].Hysteresis);
                    }
                _taggerReal.sync();
            }
            else if (Set.Swab.Type == "VIRTUAL")
            {
                _tagger = TT.createTimeTaggerVirtual();
                _taggerVirtual = (TimeTaggerVirtual)_tagger; // Cast to TimeTaggerVirtual for virtual device
                Display.Text("Virtual Time Tagger created...", false);
                _taggerVirtual.setReplaySpeed(SWAB_REPLAY_SPEED);
                _taggerVirtual.replay(Set.Swab.FileVirt);
            }

            if (Set.Swab.FrequencyMultiplier > 1)
            {
                _frequencyMultiplier = new FrequencyMultiplier(_tagger!, _syncChannels[0], (int)Set.Swab.FrequencyMultiplier);
                _frequencyMultiplier.start();
                _freqMultChannel = _frequencyMultiplier.getChannel();
                _syncChannels[0] = _freqMultChannel; // replace sync channel with frequency multiplied channel
            }

            // create the syncronisedmeasurements
            _syncMeas = new SynchronizedMeasurements(_tagger);

            // Create the measurements
            switch (Set.Swab.Meas)
            {
                case "HIST":
                    for (int id = 0; id < Set.Spc.NumDet; id++)
                        if (Set.Swab.Start == "SIGN")
                            _histogram[id] = new Histogram(_syncMeas.getTagger(), _syncChannels[0], _signChannels[id], _binwidth, Set.Spc.NumBins);
                        else
                            _histogram[id] = new Histogram(_syncMeas.getTagger(), _signChannels[id], _syncChannels[0], _binwidth, Set.Spc.NumBins);
                    break;
                case "CORR":
                    for (int id = 0; id < Set.Spc.NumDet; id++)
                        _correlation[id] = new Correlation(_syncMeas.getTagger(), _signChannels[id], _signChannels[id], _binwidth, Set.Spc.NumBins);
                    break;
            }

            // initialise the File writer
            if (Set.Swab.SaveTags)
            {
                string filePath = Store.DataPath; // use DataPath but replace extension with SWAB_FILE_EXTENSION
                filePath = Path.ChangeExtension(filePath, SWAB_FILE_EXTENSION);
                int[] channels = Set.Swab.Type switch
                {
                    "REAL" => _taggerReal!.getChannelList(),
                    "VIRTUAL" => _taggerVirtual!.getChannelList(),
                };
                _fileWriter = new FileWriter(_syncMeas.getTagger(), filePath, channels);
            }
        }

        protected override void CloseDev()
        {
            _frequencyMultiplier?.Dispose(); _frequencyMultiplier = null;
            _syncMeas?.Dispose(); _syncMeas = null;
            _fileWriter?.Dispose(); _fileWriter = null;
            for(int id = 0; id < Set.Spc.NumDet; id++)
            {
                _histogram[id]?.Dispose(); _histogram[id] = null;
                _correlation[id]?.Dispose(); _correlation[id] = null;
            }
            _tagger?.Dispose();
            _tagger = null;
            _taggerReal= null;
            _taggerVirtual = null;
            //if (SW->isFwRunning)
            //{
            //    ret = Swab_TimeTag_FileWriter_stop(SW->Fw, &SW->Except);
            //    if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "STOP FILE WRITE");
            //    ret = Swab_TimeTag_FileWriter_Dispose(SW->Fw, &SW->Except);
            //    if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "DISPOSE FILE WRITE");
            //}
            //switch (Set.Swab.Type)
            //{
            //    case "REAL":
            //        _taggerReal?.Dispose(); _taggerReal = null;
            //        break;
            //    case "VIRTUAL":
            //        _taggerVirtual?.Dispose(); _taggerVirtual = null;
            //}
            //_tagger?.Dispose(); _tagger = null;
        }

        protected override void StartDev()
        {
            _syncMeas?.startFor(_picoseconds);
            //switch(Set.Swab.Meas)
            //{
            //    case "HIST":
            //        for (int id = 0; id < Set.Spc.NumDet; id++)
            //            _histogram[id].startFor(_picoseconds);
            //        break;
            //    case "CORR":
            //        for (int id = 0; id < Set.Spc.NumDet; id++)
            //            _correlation[id].startFor(_picoseconds);
            //        break;
            //}
        }

        protected override void StopDev()
        {
            _syncMeas?.stop();
            //switch(Set.Swab.Meas)
            //{
            //    case "HIST":
            //        for (int id = 0; id < Set.Spc.NumDet; id++)
            //            _histogram[id].stop();
            //        break;
            //    case "CORR":
            //        for (int id = 0; id < Set.Spc.NumDet; id++)
            //            _correlation[id].stop();
            //        break;
            //}

            //if(_isFwRunning)
            //{
            //    // stop file writer if it is running
            //    // ret = Swab_TimeTag_FileWriter_stop(SW->Fw, &SW->Except);
            //    // if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "STOP FILE WRITE");
            //    _isFwRunning = false;
            //}
        }

        protected override void SetTimeDev(double seconds)
        {
            _picoseconds = (Int64)(seconds * 1e12);
        }

        protected override void WaitDev()
        {
            _syncMeas?.waitUntilFinished();
            //switch (Set.Swab.Meas)
            //{
            //    case "HIST":
            //        for (int id = 0; id < Set.Spc.NumDet; id++)
            //            _histogram[id].waitUntilFinished();
            //        break;
            //    case "CORR":
            //        for (int id = 0; id < Set.Spc.NumDet; id++)
            //            _correlation[id].waitUntilFinished();
            //        break;
            //}
        }

        protected override void GetDev()
        {
            for (int id = 0; id < Set.Spc.NumDet; id++)
            {
                _staging[id] = Set.Swab.Meas switch
                {
                    "HIST" => _histogram[id].getData(),
                    "CORR" => _correlation[id].getData(),
                    _ => []
                };
                if(Set.Swab.Reverse)
                    Array.Reverse(_staging[id]);
            }
        }

        protected override void ClearDev()
        {
            for (int id = 0; id < Set.Spc.NumDet; id++)
            {
                switch (Set.Swab.Meas)
                {
                    case "HIST":
                        _histogram[id].clear();
                        break;
                    case "CORR":
                        _correlation[id].clear();
                        break;
                }
            }
        }

        protected override void CopyDev(Span<TypeData> destination)
        {
            for (int id = 0; id < Set.Spc.NumDet; id++)
                MemoryMarshal.Cast<int, TypeData>(_staging[id].AsSpan()).CopyTo(destination.Slice(id * Set.Spc.NumBins, Set.Spc.NumBins));
        }
    }
}



        ///* GET TIME OF REAL ACQUISITION SWAB */	
        //void GetSwabElapsedTime(double* Elapsed_Time)
        //{
        //    int ret, id;
        //    __int64 elapsed_time;

        //    struct SwabS*SW = &P.Spc.Swab[0];  // NOT WORKING FOR MULTIPLE BOARDS
        //switch (SW->Meas)
        //{
        //    case SWAB_HIST:
        //        for (id = 0; id < P.Num.Det; id++)
        //            ret = Swab_TimeTag_Histogram_getCaptureDuration(SW->Hist[id], &elapsed_time, &SW->Except);
        //        break;
        //    case SWAB_CORR:
        //        for (id = 0; id < P.Num.Det; id++)
        //            //ret = Swab_TimeTag_Correlation_getCaptureDuration (SW->Corr[id], &elapsed_time, &SW->Except);
        //            ret = Swab_TimeTag_HistogramLogBins_getCaptureDuration(SW->Corr[id], &elapsed_time, &SW->Except);
        //        break;
        //}
        //if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "PAUSE");
        //*Elapsed_Time = elapsed_time / (1.0 * SWAB_S2PS);
        //	}

        ///*	START FILE WRITER SWAB */	
        //void StartFileSwab(void)
        //{
        //    int ret;
        //    int is_running;

        //    struct SwabS*SW = &P.Spc.Swab[0]; // NOT WORKING FOR MULTIPLE BOARDS     
        //int detectors[MAX_DET];
        //ssize_t numdet = P.Num.Det + 1; // number of detectors in Swab

        //if (!SW->SaveTags) return;
        //if (!P.Action.SwabNewFile) return;
        //detectors[0] = SW->DetSyncFreqMult; // NOTE: in case FreqDiv is APPLIED, this is Frequency Divided, and in case Frequency Multiplied
        //for (int id = 0; id < P.Num.Det; id++) detectors[id + 1] = SW->DetSign[id];
        ////Open the FileWrite for writing the stream (Meas already checked)
        //if (!SW->isFwRunning)
        //{
        //    strcpy(SW->FPathOut, P.File.Path); // take file name from .DAT file
        //    char* dot_position = strrchr(SW->FPathOut, '.');
        //    *dot_position = '\0';
        //    //SW->FPathOut[strlen(SW->FPathOut)-strlen(P.File.Ext)-1]='\0'; //delete also the dot for extension
        //    if (SW->SplitFile == LOOP_NONE) sprintf(SW->FPathOut, "%s.%s", SW->FPathOut, SWAB_FILEEXT);
        //    else sprintf(SW->FPathOut, "%s_%d.%s", SW->FPathOut, SW->FileIndex++, SWAB_FILEEXT);// increment index
        //    ret = Swab_TimeTag_FileWriter__Create(&SW->Fw, SW->Ttb, SW->FPathOut, detectors, numdet, &SW->Except);
        //    if (ret < 0) ErrHandler(ERR_SWAB, ret, "FILE WRITER");
        //    SW->isFwRunning = TRUE;
        //}
        //	}

