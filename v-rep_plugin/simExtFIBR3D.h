#pragma once

#include "Printer.h"
#include "Extruder.h"

#include <string>

#ifdef _WIN32
    #define SIM_DLLEXPORT extern "C" __declspec(dllexport)
#else
    #define SIM_DLLEXPORT extern "C"
#endif


// The 3 required entry points of the CoppelisSim plugin:
SIM_DLLEXPORT unsigned char simStart(void* reservedPointer,int reservedInt);
SIM_DLLEXPORT void simEnd();
SIM_DLLEXPORT void* simMessage(int message,int* auxiliaryData,void* customData,int* replyData);

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
