// Copyright 2006-2016 Coppelia Robotics GmbH. All rights reserved. 
// marc@coppeliarobotics.com
// www.coppeliarobotics.com
// 
// -------------------------------------------------------------------
// THIS FILE IS DISTRIBUTED "AS IS", WITHOUT ANY EXPRESS OR IMPLIED
// WARRANTY. THE USER WILL USE IT AT HIS/HER OWN RISK. THE ORIGINAL
// AUTHORS AND COPPELIA ROBOTICS GMBH WILL NOT BE LIABLE FOR DATA LOSS,
// DAMAGES, LOSS OF PROFITS OR ANY OTHER KIND OF LOSS WHILE USING OR
// MISUSING THIS SOFTWARE.
// 
// You are free to use/modify/distribute this file for whatever purpose!
// -------------------------------------------------------------------
//
// This file was automatically created for V-REP release V3.3.2 on August 29th 2016
#pragma once

#include "Printer.h"
#include "Extruder.h"

#include <string>

#ifdef _WIN32
    #define VREP_DLLEXPORT extern "C" __declspec(dllexport)
#endif /* _WIN32 */
#if defined (__linux) || defined (__APPLE__)
    #define VREP_DLLEXPORT extern "C"
#endif /* __linux || __APPLE__ */


// The 3 required entry points of the V-REP plugin:
VREP_DLLEXPORT unsigned char v_repStart(void* reservedPointer,int reservedInt);
VREP_DLLEXPORT void v_repEnd();
VREP_DLLEXPORT void* v_repMessage(int message,int* auxiliaryData,void* customData,int* replyData);

/*!
 * Guaranteeing the VRep standard API functions are always called from the VRep main thread.
 *
 * Instead of directly calling this functions from communication threads, the information is
 * passed to queues that are push/poped from the communication or VRep thread.
 *
 * To clear the plugin files, it is created objects that encapsulate information relative to
 * robots, end-effectors or surgical trajectories.
 */

 //Printer Communication Functions
float v_repExtGetSimulationTimeStep();
void v_repExtPQGetJointPositions(std::vector<double> &pos);
void v_repExtPQSetJointTrajectory(std::vector<std::vector<double> > &dpos, const std::vector< std::pair<bool, int> > &extrude, 
	const std::vector<int> &line);
void v_repExtPQAdvanceLine(int line);
void v_repExtPQStopPrinting(); //Stop communication server, clear joint queues...
void v_repExtPQBarMsgCommunication(const std::string &msg);
void v_repExtPQBoxMsgCommunication(const std::string &msg);

namespace _internal_ {
	//Printer Control
	void v_repExtGetHandles(Printer *printer, Extruder *extruder);
	bool v_repExtGetJointPositions(const std::vector<int> &jhandles, std::vector<double> &pos);
	bool v_repExtSetJointPositions(const std::vector<int> &jhandles, const std::vector<double> &dpos, const int ehandle, const bool extrude, 
		const int extrude_type);
	void v_repExtRemoveMaterial();
	void v_repExtCreateMesh();
	void v_repExtBarMsgCommunication(const std::string &msg); //Message to status bar
	void v_repExtBoxMsgCommunication(const std::string &msg); //Message dialog

	void v_repAddDrawingItem(const int paint_handle, const float *paint_point);
}
