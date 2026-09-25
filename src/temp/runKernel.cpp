#include <iostream>
#include <memory>
#include <vector>

#include "src/run/runKernel.h"
#include "src/gui/AppLogger.h"
#include "src/gui/trs2.h"
#include <cstdint>

import Const;
import Globals;
import Step;
import StepFactory;
import StepArd;
import StepMicro;
import Spc;
import TestSpc;
import SpcFactory;
import Data;
import Save;
import Display;

void loopGet(int loop) {
    int l = loop;
    for (int iL = 0; iL < MAX_LOOP; iL++) {
        if (P.Loop[iL].Num > 0) {
            P.Loop[iL].Actual = l % P.Loop[iL].Num;
            l /= P.Loop[iL].Num;
        } else {
            P.Loop[iL].Actual = 0;
        }
    }
}

void Oscilloscope(void) {
    P.Command.Abort = false;
    spc[0]->start(P.Spc.TimeO);
    while (!P.Command.Abort) {
            D->copyTemp();
            std::vector<double> t(static_cast<size_t>(P.Bins.Num));
            for (int ib = 0; ib < P.Bins.Num; ++ib)
                t[static_cast<size_t>(ib)] = (static_cast<double>(ib) + 0.5) * P.Spc.Factor;
            GUI->displayPlot(t, D->Temp);
            QCoreApplication::processEvents();
        }
    spc[0]->stop();
	P.Command.Abort = false;
}

void decideAction(void) {
	// create bool vectors called firstloop[MAX_LOOP], newloop[MAX_LOOP], lastloop[MAX_LOOP] for the first, new and last loop index depending on loop .Actual and loop.Num
    bool firstloop[MAX_LOOP] = { false }, newloop[MAX_LOOP] = { false }, lastloop[MAX_LOOP] = { false };
    for (int iL = 0; iL < MAX_LOOP; iL++) {
        firstloop[iL] = (P.Loop[iL].Actual == 0);
        lastloop[iL] = (P.Loop[iL].Actual == P.Loop[iL].Num - 1);
        if (iL == 0) newloop[iL] = 0; else newloop[iL] = firstloop[iL - 1];
    }
	P.Action.Oscill = false;
	P.Action.saveData = lastloop[4];
	P.Action.startSpc = firstloop[4];
	P.Action.stopSpc = lastloop[4];
    P.Action.copyArchive = true;
    P.Action.displayPlot = true;
}

void initLoop(void) {
    for(int iL=0;iL<MAX_LOOP;iL++) P.Loop[iL].Num=(P.Loop[iL].Last-P.Loop[iL].First)/P.Loop[iL].Delta+1;
}

void initSteps(void) {
    for (int iS = 0; iS < MAX_STEP; ++iS) {
        steps[iS] = createStep(iS);
        if(steps[iS]) steps[iS]->initStep();
        if(steps[iS]) steps[iS]->initPos();
    }
}

void initSpc(void) {
    spc[0] = createSpc();
    if (spc[0]) spc[0]->init();
}   

void moveStep(void) {
    for (auto& s : steps) {
        if (s) s->moveStep(&s->actual, s->calcGoal(), P.Step[s->iS].Mode != "MULTI", false);
    }
}

void copyArchive(int loop) {
    int actualSlice = P.Loop[4].Actual;
    D->copyArchive(actualSlice);
}

void runKernel(void) {

    // GUI
    P.Num.Board = 1;
	P.Num.Det = 1;
	P.Frame.Num = 1;
    P.Contest.Function = CONTEST_OSC;
	P.Action.Oscill = false;
	P.Command.Abort = false;
	P.Meas.Plot = 1;
	P.Meas.Save = P.Loop[4].Num;

    GUI->readAll();
    GUI->displayPanel("Output");

	initLoop(); 
    initData();
    initSteps();
	initSpc();
    InitDataFile();

    //Oscilloscope();

	int loop = 0;
    bool status = false;

    while (!P.Command.Abort && loop<P.Loop[0].Num*P.Loop[1].Num * P.Loop[2].Num * P.Loop[3].Num * P.Loop[4].Num) {
        loopGet(loop);
        decideAction();
        if (P.Action.Oscill) Oscilloscope();
        if (P.Action.moveStep) moveStep();
        if (P.Action.startSpc) startSpc(P.Spc.TimeM);
		if (P.Action.copyArchive) copyArchive(loop);
        if (P.Action.displayPlot) displayPlot(loop);
        if (P.Action.saveData) saveData();

        QCoreApplication::processEvents();

		loop++;
    }

    // Close file if needed
	// stop all thread before closing Data

    spc[0]->close();
    CloseDataFile();
    closeData();

    GUI->displayPanel("Parm");
}