using SwabianInstruments.TimeTagger;
using System;
using System.DirectoryServices;
using System.Timers;
using TypeData = System.UInt32; // 4 bytes

namespace TRS2
{
    public class SpcSwab : Spc
    {
        public const int SWAB_MAX_CHANN = 18; // max number of Swabian channels
        public const double SWAB_REPLAY_SPEED = 1.0; // replay speed for Virtual file


        private double _seconds;
        private TimeTaggerBase? _tagger=null; // TimeTaggerBase is the base class for both TimeTagger and TimeTaggerVirtual
        private TimeTagger? _taggerReal=null; // TimeTagger is the class for the real TimeTagger device
        private TimeTaggerVirtual? _taggerVirtual=null; // TimeTaggerVirtual is the class for the virtual TimeTagger device
        private int[] _signChannels = new int[SWAB_MAX_CHANN]; // Array to hold the signal channels
        private int[] _syncChannels = new int[SWAB_MAX_CHANN]; // Array to hold the sync channels
        private int _numSignChannels=0; // Number of signal channels
        private int _numSyncChannels=0; // Number of sync channels
        private FrequencyMultiplier? _frequencyMultiplier=null;
        private int _freqMultChannel;

        protected override void InitDev()
        {
            // complete channel arrays based on Set.Swab.Setting
            for (int ic = 0; ic < SWAB_MAX_CHANN; ic++)
                if (Set.Swab.Setting[ic].Type == "SIGN")
                    _signChannels[_numSignChannels++] = ic;
                else if (Set.Swab.Setting[ic].Type == "SYNC")
                    _syncChannels[_numSyncChannels++] = ic;
            Array.Resize(ref _signChannels, _numSignChannels);
            Array.Resize(ref _syncChannels, _numSyncChannels);

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
                for(int ic=0; ic<SWAB_MAX_CHANN; ic++)
                    if (Set.Swab.Setting[ic].Type != "NONE")
                    {
                        _taggerReal.setTriggerLevel(ic + 1, Set.Swab.Setting[ic].Level);
                        _taggerReal.setInputDelay(ic + 1, Set.Swab.Setting[ic].Delay);
                        _taggerReal.setDeadtime(ic + 1, Set.Swab.Setting[ic].Deadtime);
                        if (Set.Swab.Setting[ic].Hysteresis > 0)
                            _taggerReal.setInputHysteresis(ic + 1, Set.Swab.Setting[ic].Hysteresis);
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

            if(Set.Swab.FrequencyMultiplier > 1)
            {
                _frequencyMultiplier = new FrequencyMultiplier(_tagger!, _syncChannels[0], (int)Set.Swab.FrequencyMultiplier);
                _frequencyMultiplier.start();
                _freqMultChannel=_frequencyMultiplier.getChannel();
                _syncChannels[0] = _freqMultChannel; // replace sync channel with frequency multiplied channel
            }

            //        SW->Binwidth=(__int64) (P.Spc.Factor+0.5);
            //	sprintf(SW->FPathVirt, "%s.%s", SW->FNameVirt, SWAB_FILEEXT);
            //        P.Spc.Swab[0].FileIndex=0; // when splittingt file, initialise first to 0

            //        // Initialise
            //        SW->isFwRunning=FALSE;


            ////Create the measurements
            //switch (SW->Meas)
            //{
            //    case SWAB_HIST:
            //        for (id = 0; id < P.Num.Det; id++)
            //            if (SW->Start == SWAB_SIGN)
            //                ret = Swab_TimeTag_Histogram__Create(&SW->Hist[id], SW->Ttb, SW->DetSyncFreqMult, SW->DetSign[id], SW->Binwidth, P.Chann.Num, &SW->Except); // Invert Start & Stop (Start is SIGN)
            //            else
            //                ret = Swab_TimeTag_Histogram__Create(&SW->Hist[id], SW->Ttb, SW->DetSign[id], SW->DetSyncFreqMult, SW->Binwidth, P.Chann.Num, &SW->Except);
            //        break;
            //    case SWAB_CORR:
            //        for (id = 0; id < P.Num.Det; id++)
            //            //ret=Swab_TimeTag_HistogramLogBins__Create(&SW->Corr[id],SW->Ttb,SW->DetSign[id],SW->DetSign[id],0.000001,0.001,P.Chann.Num,&SW->Except);
            //            ret = Swab_TimeTag_Correlation__Create(&SW->Corr[id], SW->Ttb, SW->DetSyncFreqMult, SW->DetSign[id], SW->Binwidth, P.Chann.Num, &SW->Except);
            //        break;
            //}
            //if (ret < 0) ErrHandler(ERR_SWAB, ret, "CREATE MEAS");
            //Passed();
            //	} 

            ///* CLOSE SWAB */	
            //void CloseSwab(void)
            //{
            //    int ret;

            //    struct SwabS*SW = P.Spc.Swab;
            //if (SW->isFwRunning)
            //{
            //    ret = Swab_TimeTag_FileWriter_stop(SW->Fw, &SW->Except);
            //    if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "STOP FILE WRITE");
            //    ret = Swab_TimeTag_FileWriter_Dispose(SW->Fw, &SW->Except);
            //    if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "DISPOSE FILE WRITE");
            //}
            //switch (SW->Type)
            //{
            //    case SWAB_REAL:
            //        ret = Swab_TimeTag_TimeTagger_Dispose(SW->Ttr, &SW->Except);
            //        if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "DISPOSE REAL");
            //        ret = CDotNetDiscardHandle(SW->Ttr);
            //        if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "DISCARD .NET HANDLE");
            //        break;
            //    case SWAB_VIRTUAL:
            //        ret = Swab_TimeTag_TimeTaggerVirtual_stop(SW->Ttv, &SW->Except);
            //        if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "STOP REPLAY VIRTUAL");
            //        ret = Swab_TimeTag_TimeTaggerVirtual_Dispose(SW->Ttv, &SW->Except);
            //        if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "DISPOSE VIRTUAL");
            //        ret = CDotNetDiscardHandle(SW->Ttv);
            //        if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "DISCARD .NET HANDLE");
            //        break;
            //}
            //ret = Close_SwabianInstruments_TimeTagger();
            //if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "CLOSE TIME TAGGER");
            //	} 

        }

                ///* TRANSFER DATA FROM SWAB */	
                //void GetDataSwab(void)
                //{
                //    int ret, id, ic;
                //    ssize_t arraylen;
                //    int* pData;

                //    struct SwabS*SW = &P.Spc.Swab[0];  // NOT WORKINNG FOR MULTIPLE BOARDS

                //for (int ib = 0; ib < P.Num.Board; ib++) // NOT WORKINNG FOR MULTIPLE BOARDS      
                //    for (id = 0; id < P.Num.Det; id++)
                //    {
                //        switch (SW->Meas)
                //        {
                //            case SWAB_HIST:
                //                ret = Swab_TimeTag_Histogram_getData(SW->Hist[id], &pData, &arraylen, &SW->Except);
                //                break;
                //            case SWAB_CORR:
                //                ret = Swab_TimeTag_HistogramLogBins_getData(SW->Corr[id], &pData, &arraylen, &SW->Except);
                //                //ret = Swab_TimeTag_HistogramLogBins_getDataNormalizedG2(SW->Corr[id], &pData, &arraylen, &SW->Except);
                //                //ret=Swab_TimeTag_Correlation_getData(SW->Corr[id],&pData,&arraylen,&SW->Except);
                //                break;
                //        }
                //        if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "GET DATA");
                //        if (SW->Meas == SWAB_HIST)
                //            for (ic = 0; ic < arraylen; ic++)
                //                if (SW->Reverse)  // REVERSE DATA
                //                    D.Buffer[ib][ic + id * P.Chann.Num] = (T_DATA)pData[arraylen - 1 - ic];
                //                else
                //                    D.Buffer[ib][ic + id * P.Chann.Num] = (T_DATA)pData[ic];
                //        if (SW->Meas == SWAB_CORR)
                //            for (ic = 0; ic < arraylen; ic++)
                //                D.Buffer[ib][ic + id * P.Chann.Num] = (T_DATA)pData[ic];
                //        //D.Buffer[ib][ic+id*P.Chann.Num]=(T_DATA) (pow(10,5*pData[ic]));
                //        CDotNetFreeMemory(pData); // free memory of the allocated array
                //    }
                //	}



                ///* CLEAR SWAB */	
                //void ClearSwab(void)
                //{
                //    int ret, id;

                //    struct SwabS*SW = &P.Spc.Swab[0]; // NOT WORKINNG FOR MULTIPLE BOARDS      ;
                //switch (SW->Meas)
                //{
                //    case SWAB_HIST:
                //        for (id = 0; id < P.Num.Det; id++)
                //            ret = Swab_TimeTag_Histogram_clear(SW->Hist[id], &SW->Except);
                //        break;
                //    case SWAB_CORR:
                //        for (id = 0; id < P.Num.Det; id++)
                //            //ret = Swab_TimeTag_Correlation_clear (SW->Corr[id], &SW->Except);
                //            ret = Swab_TimeTag_HistogramLogBins_clear(SW->Corr[id], &SW->Except);
                //        break;
                //}
                //if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "CLEAR");
                //	}


                ///* PAUSE SWAB */	
                //void PauseSwab(int Board)
                //{
                //    int ret, id;

                //    struct SwabS*SW = &P.Spc.Swab[Board];
                //switch (SW->Meas)
                //{
                //    case SWAB_HIST:
                //        for (id = 0; id < P.Num.Det; id++)
                //            ret = Swab_TimeTag_Histogram_stop(SW->Hist[id], &SW->Except);
                //        break;
                //    case SWAB_CORR:
                //        for (id = 0; id < P.Num.Det; id++)
                //            //ret = Swab_TimeTag_Correlation_stop (SW->Corr[id], &SW->Except);
                //            ret = Swab_TimeTag_HistogramLogBins_stop(SW->Corr[id], &SW->Except);
                //        break;
                //}
                //if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "PAUSE");
                //	}


                ///* START SWAB */	
                //void StartSwab(int Board)
                //{
                //    int ret, id;

                //    struct SwabS*SW = &P.Spc.Swab[Board];

                //switch (SW->Meas)
                //{
                //    case SWAB_HIST:
                //        for (id = 0; id < P.Num.Det; id++)
                //            ret = Swab_TimeTag_Histogram_startFor(SW->Hist[id], SW->TimeSW, 0, &SW->Except);
                //        break;
                //    case SWAB_CORR:
                //        for (id = 0; id < P.Num.Det; id++)
                //            //ret = Swab_TimeTag_Correlation_startFor (SW->Corr[id], SW->TimeSW, 0, &SW->Except);
                //            ret = Swab_TimeTag_HistogramLogBins_startFor(SW->Corr[id], SW->TimeSW, 0, &SW->Except);
                //        break;
                //}
                //if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "PAUSE");
                //	}


                ///* TIME SWAB */	
                //void TimeSwab(int Board, double Time)
                //{

                //    struct SwabS*SW = &P.Spc.Swab[Board];
                //SW->TimeSW = (__int64)(Time * SWAB_S2PS + 0.5);
                //	}


                ///* STOP SWAB */	
                //void StopSwab(int Board)
                //{
                //    int ret, id;

                //    struct SwabS*SW = &P.Spc.Swab[Board];
                //switch (SW->Meas)
                //{
                //    case SWAB_HIST:
                //        for (id = 0; id < P.Num.Det; id++)
                //            ret = Swab_TimeTag_Histogram_stop(SW->Hist[id], &SW->Except);
                //        break;
                //    case SWAB_CORR:
                //        for (id = 0; id < P.Num.Det; id++)
                //            //ret = Swab_TimeTag_Correlation_stop (SW->Corr[id], &SW->Except);
                //            ret = Swab_TimeTag_HistogramLogBins_stop(SW->Corr[id], &SW->Except);
                //        break;
                //}
                //if (SW->isFwRunning)
                //{
                //    ret = Swab_TimeTag_FileWriter_stop(SW->Fw, &SW->Except);
                //    SW->isFwRunning = FALSE;
                //}

                //if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "STOP");
                //	}


                ///* WAIT SWAB */	
                //void WaitSwab(int Board)
                //{
                //    int ret, id;
                //    int is_running;

                //    struct SwabS*SW = &P.Spc.Swab[Board];
                ////do{
                //switch (SW->Meas)
                //{
                //    case SWAB_HIST:
                //        for (id = 0; id < P.Num.Det; id++)
                //            //ret = Swab_TimeTag_Histogram_isRunning (SW->Hist[id], &is_running, &SW->Except);
                //            ret = Swab_TimeTag_Histogram_waitUntilFinished(SW->Hist[id], -1, &is_running, &SW->Except);
                //        break;
                //    case SWAB_CORR:
                //        for (id = 0; id < P.Num.Det; id++)
                //            //ret = Swab_TimeTag_Correlation_isRunning (SW->Corr[id], &is_running, &SW->Except);
                //            ret = Swab_TimeTag_HistogramLogBins_waitUntilFinished(SW->Corr[id], -1, &is_running, &SW->Except);
                //        break;
                //}
                //if (ret < 0) ErrHandler(ERR_SWAB, (short)ret, "WAIT SWAB");
                //	//	}
                //	//while(is_running);
                //	}

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

        protected override void CloseDev()
        {
            _frequencyMultiplier?.Dispose();
            _frequencyMultiplier=null;
            // Cleanup logic for the test SPC device
        }
        protected override void StartDev()
        {
            // Start acquisition logic for the test SPC device
        }
        protected override void StopDev()
        {
            // Stop acquisition logic for the test SPC device
        }
        protected override void SetTimeDev(double seconds)
        {
            // Set acquisition time logic for the test SPC device
            _seconds = seconds;
        }
        protected override void WaitDev()
        {
            // wait for _seconds seconds
            System.Threading.Thread.Sleep((int)(_seconds * 1000));
        }
        protected override void GetDev()
        {
            int numBins = Set.Spc.NumBins;
            int numDet = Set.Spc.NumDet;
            int numBoard = Set.Spc.NumBoard;
            double binWidth = Set.Spc.BinWidth;
            double[] dataD = new double[numBins];

            for (int ibd = 0; ibd < numBoard; ++ibd)
            {
                for (int id = 0; id < numDet; ++id)
                {
                    double mus = Konst.TEST_MUS / (numBoard * (1 / 0.3)) * (1 + (2 * (id + (ibd * numDet))));
                    double mua = Konst.TEST_MUA / (numDet * (1 / 0.3)) * (1 + (2 * (id + (ibd * numDet))));
                    double r = Konst.TEST_RHO;
                    double v = Konst.TEST_V;
                    double area = 0.0;
                    for (int ib = 0; ib < numBins; ib++)
                    {
                        double t = (ib + 0.5) * binWidth;
                        dataD[ib] = Math.Pow(t, -5.0 / 2.0) / mus * Math.Exp(-mua * v * t) * Math.Exp(-(3.0 * r * r * mus) / (4.0 * v * t));
                    }
                    for (int ib = 0; ib < numBins; ib++)
                    {
                        area += dataD[ib];
                    }
                    for (int ib = 0; ib < numBins; ib++)
                    {
                        double timeA = _seconds;
                        double value = Konst.TEST_AREA * timeA / area * dataD[ib];
                        value *= 1 - Konst.TEST_NOISE + Konst.TEST_NOISE * (2.0 * Random.Shared.NextDouble() - 1.0);
                        Buffer[ib + (id * numBins) + (ibd * numDet * numBins)] = (TypeData)value;
                    }
                }
            }
        }
    }
}