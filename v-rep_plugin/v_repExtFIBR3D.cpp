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
#include "v_repExtFIBR3D.h"
#include "Server.h"
#include "string_concurrent_queue.h"

#include "vrep/include/scriptFunctionData.h"
#include "vrep/include/v_repLib.h"
#include "vrep/include/v_repConst.h"
#include "vrep/include/v_repTypes.h"

//PCL
//#include <pcl/io/pcd_io.h>
//#include <pcl/point_types.h>
//#include <pcl/filters/voxel_grid.h>

//#define _LOGGING_ 1

//LOGGING
#ifdef _LOGGING_
#include "spdlog/spdlog.h"
#include "spdlog/sinks/stdout_color_sinks.h" //support for stdout logging
#include "spdlog/sinks/basic_file_sink.h" // support for basic file logging
#include "spdlog/sinks/rotating_file_sink.h" // support for rotating file logging
#endif // _LOGGING_

#include <boost/date_time.hpp>

#include <algorithm>
#include <vector>
#include <iostream>

#define _USE_MATH_DEFINES
#include <math.h>

#ifdef _WIN32
    #ifdef QT_COMPIL
        #include <direct.h>
    #else
        #include <shlwapi.h>
        #pragma comment(lib, "Shlwapi.lib")
    #endif
#endif /* _WIN32 */
#if defined (__linux) || defined (__APPLE__)
    #include <unistd.h>
#endif /* __linux || __APPLE__ */

#ifdef __APPLE__
#define _stricmp strcmp
#endif

#define CONCAT(x,y,z) x y z
#define strConCat(x,y,z)    CONCAT(x,y,z)

#define PLUGIN_NAME "FIBR3D"
#define PLUGIN_VERSION 1

#define MESH_CREATION 0
#define OCTREE_COLLISION 1

LIBRARY vrepLib; // the V-REP library that we will dynamically load and bind

#ifdef _LOGGING_
using namespace spdlog;
#endif // _LOGGING_
using namespace std::chrono;
using namespace std;

//Component Initialization Variables
bool plugin_loaded = false;
bool first_extrude = true;
int previous_extrude_type = 0;

//Handle gcode line changes
int gcode_line = 0;
//Handle collision line changes
tuple<int, bool> info_line = make_tuple<int,bool>(0, false);

//Object Handlers
unique_ptr<Printer> ptr_printer;
unique_ptr<Extruder> ptr_extruder;
unique_ptr<CommunicationManager> ptr_cm;

system_clock::time_point now, last;

// Get current system time
#ifdef _LOGGING_
boost::posix_time::ptime timeLocal = boost::posix_time::second_clock::local_time();
string date = "logs/fibr3d_" + boost::posix_time::to_iso_string(timeLocal) + "_.txt";

auto mlogger = spdlog::basic_logger_mt("FIBR3Dlogger", date);
#endif // _LOGGING_

//Messages
StringConcurrentQueue msgBar_scq;
StringConcurrentQueue msgBox_scq;

/// LOAD PLUGIN FUNCTION

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_init: Launch this plugin
// --------------------------------------------------------------------------------------
#define LUA_INIT_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_init" // the name of the new Lua command

const int inArgs_INIT[] = {
	2,
	sim_script_arg_string,0,
	sim_script_arg_int32,0,
};

void LUA_INIT_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, inArgs_INIT, inArgs_INIT[0], LUA_INIT_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded) {
			ptr_printer.reset();
			ptr_extruder.reset();
			ptr_cm.reset();

			plugin_loaded = false;
		}
		plugin_loaded = true;
		
		vector<CScriptFunctionDataItem>* inData = D.getInDataPtr();
		string pname = inData->at(0).stringData[0];
		int pdofs = inData->at(1).int32Data[0];

		ptr_printer = make_unique<Printer>(pname, pdofs);
		ptr_extruder = make_unique<Extruder>("Extruder", "DrawBoard");
		ptr_cm = make_unique<CommunicationManager>();
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_change_material_shape: Changes deposited material shape
// --------------------------------------------------------------------------------------
#define LUA_CHANGE_MATERIAL_SHAPE_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_change_material_shape" // the name of the new Lua command

const int inArgs_CHANGE_MATERIAL_SHAPE[] = {
	1,
	sim_script_arg_int32,0,
};

void LUA_CHANGE_MATERIAL_SHAPE_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, inArgs_CHANGE_MATERIAL_SHAPE, inArgs_CHANGE_MATERIAL_SHAPE[0], LUA_CHANGE_MATERIAL_SHAPE_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			std::vector<CScriptFunctionDataItem>* inData = D.getInDataPtr();

			ptr_extruder->SetFilamentType(inData->at(0).int32Data[0]);
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_change_material_color: Changes deposited material color
// --------------------------------------------------------------------------------------
#define LUA_CHANGE_MATERIAL_COLOR_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_change_material_color" // the name of the new Lua command

const int inArgs_CHANGE_MATERIAL_COLOR[] = {
	1,
	sim_script_arg_int32,0,
};

void LUA_CHANGE_MATERIAL_COLOR_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, inArgs_CHANGE_MATERIAL_COLOR, inArgs_CHANGE_MATERIAL_COLOR[0], LUA_CHANGE_MATERIAL_COLOR_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			std::vector<CScriptFunctionDataItem>* inData = D.getInDataPtr();

			ptr_extruder->SetFilamentColor(inData->at(0).int32Data[0]);	
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_change_material_size: Changes deposited material size
// --------------------------------------------------------------------------------------
#define LUA_CHANGE_MATERIAL_SIZE_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_change_material_size" // the name of the new Lua command

const int inArgs_CHANGE_MATERIAL_SIZE[] = {
	1,
	sim_script_arg_float,0,
};

void LUA_CHANGE_MATERIAL_SIZE_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, inArgs_CHANGE_MATERIAL_SIZE, inArgs_CHANGE_MATERIAL_SIZE[0], LUA_CHANGE_MATERIAL_SIZE_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			std::vector<CScriptFunctionDataItem>* inData = D.getInDataPtr();

			ptr_extruder->SetFilamentSize(inData->at(0).floatData[0]);
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_change_filament_resolution: Changes the number of elements printed in the filament
// --------------------------------------------------------------------------------------
#define LUA_CHANGE_FILAMENT_RESOLUTION_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_change_filament_resolution" // the name of the new Lua command

const int inArgs_CHANGE_FILAMENT_RESOLUTION[] = {
	1,
	sim_script_arg_float,0,
};

void LUA_CHANGE_FILAMENT_RESOLUTION_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, inArgs_CHANGE_FILAMENT_RESOLUTION, inArgs_CHANGE_FILAMENT_RESOLUTION[0], LUA_CHANGE_FILAMENT_RESOLUTION_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			std::vector<CScriptFunctionDataItem>* inData = D.getInDataPtr();

			ptr_extruder->SetFilamentResolution(inData->at(0).floatData[0]);
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_remove_material: Cleans deposited material
// --------------------------------------------------------------------------------------
#define LUA_REMOVE_MATERIAL_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_remove_material" // the name of the new Lua command

void LUA_REMOVE_MATERIAL_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, nullptr, 0, LUA_REMOVE_MATERIAL_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			_internal_::v_repExtRemoveMaterial();
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_create_mesh: Attempts to create a mesh from the printed material
// --------------------------------------------------------------------------------------
#define LUA_CREATE_MESH_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_create_mesh" // the name of the new Lua command

void LUA_CREATE_MESH_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, nullptr, 0, LUA_CREATE_MESH_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			_internal_::v_repExtCreateMesh();
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_set_collision_handles: Set collision handles
// --------------------------------------------------------------------------------------
#define LUA_SET_COLLISION_HANDLES_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_set_collision_handles" // the name of the new Lua command

const int inArgs_SET_COLLISION_HANDLES[] = {
	1,
	sim_script_arg_int32 | sim_script_arg_table,0,
};

void LUA_SET_COLLISION_HANDLES_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, inArgs_SET_COLLISION_HANDLES, inArgs_SET_COLLISION_HANDLES[0], LUA_SET_COLLISION_HANDLES_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			std::vector<CScriptFunctionDataItem>* inData = D.getInDataPtr();
			size_t num_collisions = inData->at(0).int32Data.size();
			int* collision_handles = &inData->at(0).int32Data[0];

			ptr_printer->collision_handles = std::vector<int>(collision_handles, collision_handles + num_collisions);
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_enable_collisions: Enables/Disables collision checking
// --------------------------------------------------------------------------------------
#define LUA_ENABLE_COLLISIONS_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_enable_collisions" // the name of the new Lua command

const int inArgs_ENABLE_COLLISIONS[] = {
	1,
	sim_script_arg_bool,0,
};

void LUA_ENABLE_COLLISIONS_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, inArgs_ENABLE_COLLISIONS, inArgs_ENABLE_COLLISIONS[0], LUA_ENABLE_COLLISIONS_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded)
		{
			std::vector<CScriptFunctionDataItem>* inData = D.getInDataPtr();

			ptr_printer->collisions_enabled = inData->at(0).boolData[0];
		}
	}
	D.writeDataToStack(p->stackID);
}

// --------------------------------------------------------------------------------------
// simExtSurgRobotControl_cleanup: Cleanup this plugin
// --------------------------------------------------------------------------------------
#define LUA_CLEANUP_FIBR3D_CONTROL_COMMAND "simExtFIBR3DControl_cleanup" // the name of the new Lua command

void LUA_CLEANUP_FIBR3D_CONTROL_CALLBACK(SScriptCallBack* p)
{
	CScriptFunctionData D;
	if (D.readDataFromStack(p->stackID, nullptr, 0, LUA_CLEANUP_FIBR3D_CONTROL_COMMAND))
	{
		if (plugin_loaded) {
			ptr_printer.reset(nullptr);
			ptr_extruder.reset(nullptr);
			ptr_cm.reset(nullptr);

			plugin_loaded = false;
		}
	}
	D.writeDataToStack(p->stackID);
}

////////////////////////////////////////////////////////////////////////////////////////
/// PRINTER PLUGIN INTERFACE

float v_repExtGetSimulationTimeStep()
{
	float dt = simGetSimulationTimeStep();
	if (dt == -1.0f)
	{
		dt = 0.1f;
	}

	return dt;
}

void v_repExtPQGetJointPositions(std::vector<double>& pos)
{
	if (ptr_printer->ready)
	{
		pos = ptr_printer->get_cur_pos();
	}
}

void v_repExtPQSetJointTrajectory(std::vector<std::vector<double> > & dpos, const std::vector< std::pair<bool, int> > &extrude, const std::vector<int> &line)
{
	if (ptr_printer->ready)
	{
		// Push target positions and extrude property
		for (size_t i = 0; i < dpos.size(); ++i)
		{
			ptr_printer->target_jpos.push_front(dpos[i]);
			//push paint and type
			ptr_extruder->paint.push_front(extrude[i]);

			//added line control
			ptr_printer->line.push_front(line[i]);
		}
	}
}

void v_repExtPQAdvanceLine(int line)
{
	if (ptr_printer->ready)
	{
		//added line control
		ptr_printer->line.push_front(line);
	}
}

void v_repExtPQStopPrinting()
{
	//Stop server communication
	ptr_cm->StopServer();

	//Clear joint position / velocity queues
	ptr_printer->target_jpos.clear();
	ptr_extruder->paint.clear();

	v_repExtPQBoxMsgCommunication("Printing process halted. Communications terminated.");
}

void v_repExtPQBarMsgCommunication(const std::string & msg)
{
	if (ptr_printer->ready)
	{
		msgBar_scq.push_back(msg);
	}
}

void v_repExtPQBoxMsgCommunication(const std::string & msg)
{
	if (ptr_printer->ready)
	{
		msgBox_scq.push_back(msg);
	}
}

////////////////////////////////////////////////////////////////////////////////////////
/// PRINTER INTERNAL

namespace _internal_ {

	void v_repExtGetHandles(Printer * printer, Extruder * extruder)
	{
		//Get robot handles
		int phandle = simGetObjectHandle(printer->name.c_str());
		if (phandle != -1)
		{
			printer->handle = phandle;
			//Get robot joint handles
			int bit_options, num_objects;
			bit_options = 1; //bit0 set (1): exclude the tree base from the returned array
			int *ret = simGetObjectsInTree(phandle, sim_object_joint_type, bit_options, &num_objects);
			if (ret != nullptr)
			{
				printer->joint_handles.resize(printer->dofs);
				// Common 3 DoFs
				printer->joint_handles[0] = simGetObjectHandle("AxisX_joint");
				printer->joint_handles[1] = simGetObjectHandle("AxisY_joint");
				printer->joint_handles[2] = simGetObjectHandle("AxisZ_joint");
				if (printer->dofs == 5) {
					printer->joint_handles[3] = simGetObjectHandle("AxisB_joint");
					printer->joint_handles[4] = simGetObjectHandle("AxisC_joint");
				}
				printer->ready = true;
			}
			else {
				msgBox_scq.push_back("Could not retrieve the printer joint handles.");
				return;
			}
			
			////Get collision handle
			//int coll_res = simGetCollisionHandle(printer->collision_name.c_str());
			//if(simIsHandleValid(coll_res, sim_appobj_collision_type))
			//{
			//	printer->collision_handle = coll_res;
			//}
			//else
			//{
			//	msgBox_scq.push_back("Could not retrieve the collision handle.");
			//}
		}
		else {
			msgBox_scq.push_back("Could not retrieve the printer handle.");
			return;
		}

		//Get extruder handle
		int ejhandle = simGetObjectHandle(extruder->ext_name.c_str());
		int bhandle = simGetObjectHandle(extruder->bed_name.c_str());
		int fohandle = simGetObjectHandle(extruder->octree_name.c_str());
		if (ejhandle != -1 && bhandle != -1)
		{
			float *mat = new float[12];
			std::vector<float> vmat;
			extruder->ext_handle = ejhandle;
			extruder->bed_handle = bhandle;
			extruder->octree_handle = fohandle;
			if (printer->dofs == 3) {
				extruder->last_printer_point[3] = 0.0;
				extruder->last_printer_point[4] = 0.0;
			}
			simGetObjectMatrix(ejhandle, -1, mat);
			//Filament placement
			mat[11] -= extruder->GetFilamentSize() * 2.0;
			vmat.assign(mat, mat + 12);
			extruder->SetInitialTransformation(vmat);
		}
		else
		{
			msgBox_scq.push_back("Could not retrieve the extruder or the bed handle.");
			return;
		}
	}

	bool v_repExtGetJointPositions(const std::vector<int>& joint_handles, std::vector<double>& pos)
	{
		//! Commented because the pos comes with an assigned size and if the printer only
		//! has 3 DoF it only assigns the first 3 elements.
		//pos.resize(joint_handles.size()); 
		for (size_t i = 0; i < joint_handles.size(); ++i)
		{
			simFloat jpos;
			if (simGetJointPosition(joint_handles[i], &jpos) != -1)
			{
				pos[i] = static_cast<double>(jpos);
			}
			else
			{
				msgBox_scq.push_back("Could not retrieve current joint positions.");
				return false;
			}
		}
		return true;
	}

	bool v_repExtGetJointVelocities(const std::vector<int>& joint_handles, std::vector<double>& vel)
	{
		//! Commented because the pos comes with an assigned size and if the printer only
		//! has 3 DoF it only assigns the first 3 elements.
		//pos.resize(joint_handles.size()); 
		for (size_t i = 0; i < joint_handles.size(); ++i)
		{
			simFloat jvel;
			if (simGetObjectFloatParameter(joint_handles[i], sim_jointfloatparam_velocity, &jvel) != -1)
			{
				vel[i] = static_cast<double>(jvel);
			}
			else
			{
				msgBox_scq.push_back("Could not retrieve current joint velocities.");
				return false;
			}
		}
		return true;
	}

	bool v_repExtSetJointPositions(const std::vector<int>& joint_handles, const std::vector<double>& dpos, const int ehandle, const bool extrude,
		const int extrude_type)
	{
		//! Commented because the plugin can either operate with 3 or a 5 DoF
		//if (joint_handles.size() != dpos.size())
		//{
		//	msgBox_scq.push_back("Number of joint handles specified do not match target joint positions.");
		//	return false;
		//}

		//Set joint positions
		for (size_t i = 0; i < joint_handles.size(); ++i)
		{
			if (simSetJointPosition(joint_handles[i], dpos[i]) == -1)
			{
				msgBox_scq.push_back("Could not set desired joint position for a passive joint.");
				return false;
			}
		}

		//Set extruder
		if (extrude)
		{
			std::vector<double> cpos(5, 0.0);
			std::vector<double> bpos(5, 0.0); 

			float last_pt[3] = { 0.0, 0.0, 0.0 }; //Last printed point
			float dvec[3] = { 0.0, 0.0, 0.0 };	//distance vector between points
			float last_bc[2] = { 0.0, 0.0 }; //Last bc coordinates

			float int_pt[3] = { 0.0, 0.0, 0.0 }; //intermediate point
			float draw_pt[3] = { 0.0, 0.0, 0.0 };
			float draw_pt_6[6] = { 0.0, 0.0, 0.0, 1.0, 0.0, 0.0 };

			std::vector<float> paint_color;
			float paint_size;
			ptr_extruder->PaintInfo(extrude_type, paint_color, paint_size);

			//If new extrude type, and not already first extrude
			if (extrude_type != previous_extrude_type) {
				//Required to reset paint, and adjust to new color and size
				first_extrude = true;
			}

			//Store previous type in memory to catch when it changes
			previous_extrude_type = extrude_type;

			float resolution = paint_size * ptr_extruder->GetFilamentResolution();

			//Get current joint positions
			v_repExtGetJointPositions(joint_handles, cpos);
			bpos.assign(ptr_extruder->last_printer_point, ptr_extruder->last_printer_point+5);

			//We are only interested in drawing on the extrusion bed - d_o
			if (first_extrude)
			{
				//sim_drawing_cyclic = if the drawing object is full, then the first items are overwritten
				int mode = ptr_extruder->drawobj_mode + sim_drawing_cyclic;
				
				size_t it_paint = simAddDrawingObject(mode, paint_size, resolution, ptr_extruder->bed_handle, ptr_extruder->buffer_size, &paint_color[0], NULL, NULL, NULL);
				ptr_extruder->paint_handle.push_back(it_paint);

				//Saved last printed point
				copy(cpos.begin(), cpos.end(), ptr_extruder->last_printer_point);

				//Convert to world coordinates and draw
				ptr_extruder->CalculateWorldPoint(cpos, draw_pt);
				switch (ptr_extruder->drawobj_mode) {
				case 6: // cubes
					copy(draw_pt, draw_pt + 3, draw_pt_6);
					v_repAddDrawingItem(ptr_extruder->paint_handle.back(), draw_pt_6);
					break;
				case 7: // spheres
				default:
					v_repAddDrawingItem(ptr_extruder->paint_handle.back(), draw_pt);
				}
				
				first_extrude = false;
			}

			copy(ptr_extruder->last_printer_point, ptr_extruder->last_printer_point + 3, last_pt);
			copy(ptr_extruder->last_printer_point + 3, ptr_extruder->last_printer_point + 5, last_bc);

			dvec[0] = cpos[0] - last_pt[0];
			dvec[1] = cpos[1] - last_pt[1];
			dvec[2] = cpos[2] - last_pt[2];

			//norm of distance vector 
			float ddist = sqrt(pow(dvec[0], 2) + pow(dvec[1], 2) + pow(dvec[2], 2));
			float bcdist = (float)ptr_extruder->CalculateRealDistancePoints(bpos,cpos);

			// XYZ motion > 0
			if (ddist > resolution)
			{
				//Number of spheres in segment
				//Determines the number of spheres required to cover the distance from the previous point to the new one.
				int n_sph = static_cast<int>(ceil(ddist / resolution));

				for (size_t i = 0; i < n_sph; ++i)
				{
					int_pt[0] = last_pt[0] + i * (dvec[0] / n_sph);
					int_pt[1] = last_pt[1] + i * (dvec[1] / n_sph);
					int_pt[2] = last_pt[2] + i * (dvec[2] / n_sph);

					//Convert to world coordinates and draw
					ptr_extruder->CalculateWorldPoint(int_pt, draw_pt);
					switch (ptr_extruder->drawobj_mode) {
					case 6: // cubes
						copy(draw_pt, draw_pt + 3, draw_pt_6);
						v_repAddDrawingItem(ptr_extruder->paint_handle.back(), draw_pt_6);
						break;
					case 7: // spheres
					default:
						v_repAddDrawingItem(ptr_extruder->paint_handle.back(), draw_pt);
					}
				}
				//Saved last printed point
				copy(cpos.begin(), cpos.end(), ptr_extruder->last_printer_point);
			}
			// No XYZ motion
			else if(bcdist > (resolution/100.0)) {
				// Draw 1
				//Saved last printed point
				copy(cpos.begin(), cpos.end(), ptr_extruder->last_printer_point);

				//Convert to world coordinates and draw
				ptr_extruder->CalculateWorldPoint(cpos, draw_pt);
				switch (ptr_extruder->drawobj_mode) {
				case 6: // cubes
					copy(draw_pt, draw_pt + 3, draw_pt_6);
					v_repAddDrawingItem(ptr_extruder->paint_handle.back(), draw_pt_6);
					break;
				case 7: // spheres
				default:
					v_repAddDrawingItem(ptr_extruder->paint_handle.back(), draw_pt);
				}
			}
		}
		else
		{
			first_extrude = true;
		}

		return true;
	}

	void v_repExtRemoveMaterial()
	{
		if (ptr_extruder)
		{
			//Remove existing Drawing objects
			for_each(ptr_extruder->paint_handle.begin(), ptr_extruder->paint_handle.end(), 
				[&](int &h) { simRemoveDrawingObject(h); });

			//Set First Extrude to true to redraw Drawing object
			first_extrude = true;
		
			if (OCTREE_COLLISION)
			{
				simRemoveVoxelsFromOctree(ptr_extruder->octree_handle, 1, NULL, 0, NULL);
			}
		}
	}

	void v_repExtCreateMesh() 
	{	/*
		if (MESH_CREATION && ptr_extruder)
		{
			int mode = ptr_extruder->drawobj_mode + sim_drawing_cyclic;
			double size = ptr_extruder->GetFilamentSize();
			size_t n_pts = ptr_extruder->paint_items.size();
			size_t n_pts2 = ptr_extruder->paint_items2.size();

			// Initialize non-ordered point cloud
			pcl::PointCloud<pcl::PointXYZ>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZ>);
			cloud->width = n_pts;
			cloud->height = 1;
			cloud->is_dense = false;
			cloud->points.resize(n_pts);

			// Initialize non-ordered point cloud
			pcl::PointCloud<pcl::PointXYZ>::Ptr cloud2(new pcl::PointCloud<pcl::PointXYZ>);
			cloud2->width = n_pts2;
			cloud2->height = 1;
			cloud2->is_dense = false;
			cloud2->points.resize(n_pts2);

			for (size_t i = 0; i < n_pts; ++i)
			{
				std::vector<float> pt = ptr_extruder->paint_items.at(i);
				cloud->points[i].x = pt[0];
				cloud->points[i].y = pt[1];
				cloud->points[i].z = pt[2];
			}

			for (size_t i = 0; i < n_pts2; ++i)
			{
				std::vector<float> pt2 = ptr_extruder->paint_items2.at(i);
				cloud2->points[i].x = pt2[0];
				cloud2->points[i].y = pt2[1];
				cloud2->points[i].z = pt2[2];
			}

			pcl::io::savePCDFileASCII("C://Work//PCL//GreedyProjection//test_pcd.pcd", *cloud);
			std::cerr << "Saved " << cloud->points.size() << " data points to test_pcd.pcd." << std::endl;

			pcl::io::savePCDFileASCII("C://Work//PCL//GreedyProjection//test_pcd2.pcd", *cloud2);
			std::cerr << "Saved " << cloud2->points.size() << " data points to test_pcd.pcd." << std::endl;

			// Filter
			pcl::PointCloud<pcl::PointXYZ>::Ptr cloud_filtered(new pcl::PointCloud<pcl::PointXYZ>);
			pcl::VoxelGrid<pcl::PointXYZ> sor;
			sor.setInputCloud(cloud2);
			sor.setLeafSize(size, size, size);
			sor.filter(*cloud_filtered);

			// Remove previous
			v_repExtRemoveMaterial();
			
			// Draw filtered
			int it_paint = simAddDrawingObject(mode, size, 0, ptr_extruder->bed_handle, ptr_extruder->buffer_size, ptr_extruder->GetFilamentColor(), NULL, NULL, NULL);
			ptr_extruder->paint_handle.push_back(it_paint);
			
			//Draw the object in the simulator
			float point[6] = {0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f};
			std::for_each(cloud_filtered->begin(), cloud_filtered->end(), [&](pcl::PointXYZ &p) 
				{ 
					point[0] = p.x;
					point[1] = p.y;
					point[2] = p.z;
					simAddDrawingObjectItem(it_paint, point);
				});

			cout << "Decimated cloud with " << cloud->size() << " -> " << cloud_filtered->size() << " pts.\n";
		}
		*/
	}

	void v_repExtBarMsgCommunication(const string &message)
	{
		simAddStatusbarMessage(message.c_str());
	}

	void v_repExtBoxMsgCommunication(const string &message)
	{
		simDisplayDialog("FIBR3D message",
			message.c_str(),
			sim_dlgstyle_ok,
			NULL, //no input
			NULL, //title default colors
			NULL, //title default colors
			NULL); //custom uiHandle
	}

	void v_repAddDrawingItem(const int paint_handle, const float *paint_point)
	{
		//Add object to paint_items VectorConcurrentQueue
		std::vector<float> 
			pt(paint_point, paint_point + 3),
			otpt(paint_point, paint_point + 3);
		ptr_extruder->paint_items.push_back(pt);

		//Draw the object in the simulator
		simAddDrawingObjectItem(paint_handle, paint_point);

		if (OCTREE_COLLISION)
		{
			//otpt[2] -= ptr_extruder->octree_leaf_size / 2;
			//Check and add point to octree
			simInt res = simCheckOctreePointOccupancy(ptr_extruder->octree_handle, 0, &otpt[0], 1, NULL, NULL, NULL);
			if (res == 0) {
				simInsertVoxelsIntoOctree(ptr_extruder->octree_handle, 0, &otpt[0], 1, NULL, NULL, NULL);
			}
		}
	}

}

// This is the plugin start routine (called just once, just after the plugin was loaded):
VREP_DLLEXPORT unsigned char v_repStart(void* reservedPointer,int reservedInt)
{
    // Dynamically load and bind V-REP functions:
    // ******************************************
    // 1. Figure out this plugin's directory:
    char curDirAndFile[1024];
#ifdef _WIN32
    #ifdef QT_COMPIL
        _getcwd(curDirAndFile, sizeof(curDirAndFile));
    #else
        GetModuleFileName(NULL,curDirAndFile,1023);
        PathRemoveFileSpec(curDirAndFile);
    #endif
#elif defined (__linux) || defined (__APPLE__)
    getcwd(curDirAndFile, sizeof(curDirAndFile));
#endif

    std::string currentDirAndPath(curDirAndFile);
    // 2. Append the V-REP library's name:
    std::string temp(currentDirAndPath);
#ifdef _WIN32
    temp+="\\v_rep.dll";
#elif defined (__linux)
    temp+="/libv_rep.so";
#elif defined (__APPLE__)
    temp+="/libv_rep.dylib";
#endif /* __linux || __APPLE__ */
    // 3. Load the V-REP library:
    vrepLib=loadVrepLibrary(temp.c_str());
    if (vrepLib==NULL)
    {
        std::cout << "Error, could not find or correctly load the V-REP library. Cannot start '" << PLUGIN_NAME << "'.\n";
        return(0); // Means error, V-REP will unload this plugin
    }
    if (getVrepProcAddresses(vrepLib)==0)
    {
        std::cout << "Error, could not find all required functions in the V-REP library. Cannot start '" << PLUGIN_NAME << "'.\n";
        unloadVrepLibrary(vrepLib);
        return(0); // Means error, V-REP will unload this plugin
    }
    // ******************************************

    // Check the version of V-REP:
    // ******************************************
    int vrepVer;
    simGetIntegerParameter(sim_intparam_program_version,&vrepVer);
    if (vrepVer<30200) // if V-REP version is smaller than 3.02.00
    {
        std::cout << "Sorry, your V-REP copy is somewhat old. Cannot start '" << PLUGIN_NAME << "'.\n";
        unloadVrepLibrary(vrepLib);
        return(0); // Means error, V-REP will unload this plugin
    }
    // ******************************************

    std::vector<int> inArgs;
	
	// Register the new Lua command "simExtSurgRobotControl_functions":
	simRegisterScriptCallbackFunction(strConCat(LUA_INIT_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_INIT_FIBR3D_CONTROL_COMMAND, "(string printerName, number printerDoFs)"), LUA_INIT_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_CHANGE_MATERIAL_SHAPE_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_CHANGE_MATERIAL_SHAPE_FIBR3D_CONTROL_COMMAND, "(number shapeID)"), LUA_CHANGE_MATERIAL_SHAPE_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_CHANGE_MATERIAL_COLOR_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_CHANGE_MATERIAL_COLOR_FIBR3D_CONTROL_COMMAND, "(number colorID)"), LUA_CHANGE_MATERIAL_COLOR_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_CHANGE_MATERIAL_SIZE_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_CHANGE_MATERIAL_SIZE_FIBR3D_CONTROL_COMMAND, "(number size)"), LUA_CHANGE_MATERIAL_SIZE_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_CHANGE_FILAMENT_RESOLUTION_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_CHANGE_FILAMENT_RESOLUTION_FIBR3D_CONTROL_COMMAND, "(number resolution)"), LUA_CHANGE_FILAMENT_RESOLUTION_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_REMOVE_MATERIAL_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_REMOVE_MATERIAL_FIBR3D_CONTROL_COMMAND, "()"), LUA_REMOVE_MATERIAL_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_CREATE_MESH_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_CREATE_MESH_FIBR3D_CONTROL_COMMAND, "()"), LUA_CREATE_MESH_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_SET_COLLISION_HANDLES_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_SET_COLLISION_HANDLES_FIBR3D_CONTROL_COMMAND, "(table collisionHandles)"), LUA_SET_COLLISION_HANDLES_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_ENABLE_COLLISIONS_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_ENABLE_COLLISIONS_FIBR3D_CONTROL_COMMAND, "(bool enableCollisions)"), LUA_ENABLE_COLLISIONS_FIBR3D_CONTROL_CALLBACK);
	simRegisterScriptCallbackFunction(strConCat(LUA_CLEANUP_FIBR3D_CONTROL_COMMAND, "@", PLUGIN_NAME), strConCat("void", LUA_CLEANUP_FIBR3D_CONTROL_COMMAND, "()"), LUA_CLEANUP_FIBR3D_CONTROL_CALLBACK);

    return(PLUGIN_VERSION); // initialization went fine, we return the version number of this plugin (can be queried with simGetModuleName)
}

// This is the plugin end routine (called just once, when V-REP is ending, i.e. releasing this plugin):
VREP_DLLEXPORT void v_repEnd()
{
    unloadVrepLibrary(vrepLib); // release the library
}

// This is the plugin messaging routine (i.e. V-REP calls this function very often, with various messages):
VREP_DLLEXPORT void* v_repMessage(int message,int* auxiliaryData,void* customData,int* replyData)
{ // This is called quite often. Just watch out for messages/events you want to handle
    // Keep following 5 lines at the beginning and unchanged:
    static bool refreshDlgFlag=true;
    int errorModeSaved;
    void* retVal=nullptr;

    simGetIntegerParameter(sim_intparam_error_report_mode,&errorModeSaved);
    simSetIntegerParameter(sim_intparam_error_report_mode,sim_api_errormessage_ignore);

    // Here we can intercept many messages from V-REP (actually callbacks). Only the most important messages are listed here.
    // For a complete list of messages that you can intercept/react with, search for "sim_message_eventcallback"-type constants
    // in the V-REP user manual.

    if (message==sim_message_eventcallback_refreshdialogs)
    {
        refreshDlgFlag=true; // V-REP dialogs were refreshed. Maybe a good idea to refresh this plugin's dialog too
    }

    if (message==sim_message_eventcallback_menuitemselected)
    { // A custom menu bar entry was selected..
      // here you could make a plugin's main dialog visible/invisible
    }

    if (message==sim_message_eventcallback_instancepass)
    {   // This message is sent each time the scene was rendered (well, shortly after) (very often)
        // It is important to always correctly react to events in V-REP. This message is the most convenient way to do so:
        int flags=auxiliaryData[0];
        bool sceneContentChanged=((flags&(1+2+4+8+16+32+64+256))!=0); // object erased, created, model or scene loaded, und/redo called, instance switched, or object scaled since last sim_message_eventcallback_instancepass message
        bool sceneLoaded=((flags&8)!=0);

        if (sceneLoaded)
        { // React to a scene load here!!
        }

        if (sceneContentChanged)
        { // we actualize plugin objects for changes in the scene
            refreshDlgFlag=true; // always a good idea to trigger a refresh of this plugin's dialog here
        }
    }

    if (message==sim_message_eventcallback_mainscriptabouttobecalled)
    { // The main script is about to be run (only called while a simulation is running (and not paused!))
    }

	if (message == sim_message_eventcallback_simulationabouttostart)
	{ // Simulation is about to start
		if ((customData == NULL) || (std::string("FIBR3D").compare((char*)customData) == 0)) // is the command also meant for this plugin?
		{
			if (plugin_loaded)
			{
				//Get Printer and Extruder Handles
				_internal_::v_repExtGetHandles(ptr_printer.get(), ptr_extruder.get());

				simAddStatusbarMessage("Waiting for connection from Parser...");
				ptr_cm->LaunchServer();

#ifdef _LOGGING_
				//Logger initialization
				mlogger->info("LAUNCHED PLUGIN");
				mlogger->info("TIME, LINE, X, Y, Z, B, C, vX, vY, vZ, vB, vC");
				set_level(spdlog::level::info);
				//flush_every(std::chrono::seconds(1));
				set_pattern("%v");

				now = system_clock::now();
				last = now;
#endif // _LOGGING_

				
			}
		}
    }

    if (message==sim_message_eventcallback_simulationended)
    { // Simulation just ended
		if ((customData == NULL) || (std::string("FIBR3D").compare((char*)customData) == 0)) // is the command also meant for this plugin?
		{
			if (plugin_loaded)
			{
				//If queues not empty, empty
				while (!ptr_printer->target_jpos.empty()) {
					ptr_printer->target_jpos.pop_back();
				}

				//If queues not empty, empty
				while (!ptr_extruder->paint.empty()) {
					ptr_extruder->paint.pop_back();
				}

				//Clean deposited material
				_internal_::v_repExtRemoveMaterial();

				//Reset actuator positions
				std::vector<double> jpos(ptr_printer->dofs, 0.0);
				_internal_::v_repExtSetJointPositions(ptr_printer->joint_handles, jpos, ptr_extruder->ext_handle, false, 0);

				simAddStatusbarMessage("Close TCP/IP connection with Printer parser.");
				ptr_cm->StopServer();

#ifdef _LOGGING_
				mlogger->flush();
#endif // _LOGGING_
			}
		}
    }

    if (message==sim_message_eventcallback_moduleopen)
    {
        if ( (customData==NULL)||(std::string("FIBR3D").compare((char*)customData)==0) ) // is the command also meant for this plugin?
        {

        }
    }

    if (message==sim_message_eventcallback_modulehandle)
    {
		if ((customData == NULL) || (std::string("FIBR3D").compare((char*)customData) == 0)) // is the command also meant for this plugin?
		{
			if (plugin_loaded)
			{
				//Robot Real
				if (ptr_printer->ready)
				{				
					//Read Joints
					std::vector<double> jpos(ptr_printer->dofs, 0.0), jvel(ptr_printer->dofs, 0.0);
					if (_internal_::v_repExtGetJointPositions(ptr_printer->joint_handles, jpos))
					{
						ptr_printer->set_cur_pos(jpos);

#ifdef _LOGGING_
						_internal_::v_repExtGetJointVelocities(ptr_printer->joint_handles, jvel);

						now = system_clock::now();
						int elapsed = static_cast<int>(duration_cast<milliseconds> (now - last).count());
						//last = now;

						//current line too
						// Pos: m -> 1000 mm, Ori: rad -> 180/M_PI deg
						// Vel: m/s -> 60000 mm/min, AVel: rad/s -> 60 * 180/M_PI deg/min
						mlogger->info("{:6d}, {:2d}, {:03.3f}, {:03.3f}, {:03.3f}, {:03.3f}, {:03.3f}, {:03.3f}, {:03.3f}, {:03.3f}, {:03.3f}, {:03.3f} ",
							elapsed,
							ptr_printer->line.back(),
							jpos[0] * 1000, jpos[1] * 1000, jpos[2] * 1000, 
							jpos[3] * 180 / M_PI, jpos[4] * 180 / M_PI,
							jvel[0] * 60000, jvel[1] * 60000,  jvel[2] * 60000,  
							jvel[3] * 60 * 180 / M_PI, jvel[4] * 60 * 180 / M_PI);
#endif // _LOGGING_
					}

					//Set Joints
					if (!ptr_printer->target_jpos.empty())
					{
						//Printing should have its own funcion with jpos!
						std::pair<bool, int> current_paint = ptr_extruder->paint.back();
						if (_internal_::v_repExtSetJointPositions(ptr_printer->joint_handles, ptr_printer->target_jpos.back(), 
							ptr_extruder->ext_handle, current_paint.first, current_paint.second))
						{
							if (!ptr_printer->target_jpos.empty()) 
							{
								ptr_printer->target_jpos.pop_back();
							}
							if (!ptr_extruder->paint.empty())
							{
								ptr_extruder->paint.pop_back();
							}
							//was here
							//if (!ptr_printer->line.empty())
						}
						else
						{
							ptr_printer->target_jpos.clear();
							ptr_extruder->paint.clear();
							ptr_printer->line.clear();
						}
					}
				}

				//Check current line, updated when there are also no joints to move
				if (!ptr_printer->line.empty())
				{
					//Current line collision		
					//int res = simReadCollision(ptr_printer->collision_handle);
					//bool colliding = (res == 1);
					//int res = simHandleCollision(sim_handle_all);
					//bool colliding = (res > 0);
					bool colliding = false; // set to true by default, collision detection selected in vrep gui
					int number_collisions = 0;
					if (ptr_printer->collisions_enabled) {
						for_each(ptr_printer->collision_handles.begin(), ptr_printer->collision_handles.end(), [&](const int &handle) {
							int res = simHandleCollision(handle);
							number_collisions += (res != -1 ? res : 0);
						});
						colliding = (number_collisions > 0);
					}


					//Changed current line
					int current_line = ptr_printer->line.back();

					//Broadcast if new line or
					//			if same line and no collision before and collision now
					if (std::get<0>(info_line) != current_line ||
						(std::get<0>(info_line) == current_line && !std::get<1>(info_line) && colliding))
					{
						std::get<0>(info_line) = current_line;
						std::get<1>(info_line) = colliding;

						//trigger event
						ptr_cm->get_server()->broadcast(current_line, colliding);
					}
					ptr_printer->line.pop_back();
				}

				//Error messages
				if (!msgBar_scq.empty()) //Status bar messages
				{
					_internal_::v_repExtBarMsgCommunication(msgBar_scq.consume_back());
				}

				if (!msgBox_scq.empty()) //Message box errors
				{
					_internal_::v_repExtBoxMsgCommunication(msgBox_scq.consume_back());
				}
			}
		}
    }

    if (message==sim_message_eventcallback_moduleclose)
    {
    }

    if (message==sim_message_eventcallback_instanceswitch)
    { // We switched to a different scene. Such a switch can only happen while simulation is not running
    }

    if (message==sim_message_eventcallback_broadcast)
    { // Here we have a plugin that is broadcasting data (the broadcaster will also receive this data!)
    }

    if (message==sim_message_eventcallback_scenesave)
    { // The scene is about to be saved. If required do some processing here (e.g. add custom scene data to be serialized with the scene)
    }

    // You can add many more messages to handle here

    if ((message==sim_message_eventcallback_guipass)&&refreshDlgFlag)
    { // handle refresh of the plugin's dialogs
        // ...
        refreshDlgFlag=false;
    }

    // Keep following unchanged:
    simSetIntegerParameter(sim_intparam_error_report_mode,errorModeSaved); // restore previous settings
    return(retVal);
}
