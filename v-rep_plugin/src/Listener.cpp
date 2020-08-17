
#include <vector>

#define _USE_MATH_DEFINES
#include <cmath>

#include <boost/serialization/serialization.hpp>
#include <boost/property_tree/ptree.hpp>
#include <boost/property_tree/json_parser.hpp>

#include "Listener.h"
#include "../simExtFIBR3D.h"

using boost::property_tree::ptree;
using boost::property_tree::read_json;
using boost::property_tree::write_json;

template <typename T>
std::vector<T> static as_vector(ptree const& pt, ptree::key_type const& key)
{
	std::vector<T> r;
	for (auto& item : pt.get_child(key))
		r.push_back(item.second.get_value<T>());
	return r;
}

Listener::Listener()
{
	m_last_position.assign(5, 0.0);

	//Get simulation timestep
	double dt = v_repExtGetSimulationTimeStep();

	m_trajectory = std::make_shared<Motion::Trajectory>(dt);
}

void Listener::Deserialize(std::string JSON_msg)
{
	// Ptree to store information from JSON
	ptree request;
	std::istringstream is(JSON_msg);
	read_json(is, request);

	// Storing information in a package object
	try {
		m_pack.keycode = (KEYCODE)request.get<int>("keycode");
		m_pack.bool1 = request.get<bool>("clkwise");
		m_pack.bool2 = request.get<bool>("interpol");
		m_pack.extrude = request.get<bool>("extrude");
		m_pack.text = request.get<std::string>("text");
		m_pack.int1 = request.get<int>("plane");
		m_pack.coord1 = request.get<double>("aux");
		ptree coord1 = request.get_child("pos1");
		ptree coord2 = request.get_child("pos2");
		ptree vel = request.get_child("vel");
		ptree acc = request.get_child("acc");
		m_pack.line = request.get<int>("line");

		// Storing arrays
		int i = 0, j = 0, k = 0, l = 0;
		//Convert mm to m
		for (ptree::value_type &el : coord1)
		{
			if (i < 3) {
				m_pack.vec1[i] = el.second.get<double>("");
			}
			else {
				m_pack.vec1[i] = el.second.get<double>("");
			}
			i++;
		}
		for (ptree::value_type &el : coord2)
		{
			m_pack.vec2[j] = el.second.get<double>("");
			j++;
		}
		for (ptree::value_type &el : vel)
		{
			m_pack.vel[k] = el.second.get<double>("");
			k++;
		}
		for (ptree::value_type &el : acc)
		{
			m_pack.acc[l] = el.second.get<double>("");
			l++;
		}
	}
	catch (const boost::property_tree::json_parser_error& e1) {
		std::string emsg = "Error reading current sent command: " + e1.message();

		v_repExtPQBarMsgCommunication(emsg);
	}
}

void Listener::PushAction()
{
	//TODO: Change motion control to ext_handle different axis acceleration / velocity
	double acc = std::min(m_pack.acc[0], std::min(m_pack.acc[1], m_pack.acc[2]));

	//Relative motion variables
	double rel_origin[5] = { 0.0, 0.0, 0.0, 0.0, 0.0 };
	//double rel_target[3];

	if(m_pack.keycode < 2000) //Motion commands
	{
		//PullCurrentPosition(current_position);
		double current_position[5];
		std::copy(m_last_position.data(), m_last_position.data()+5, current_position);
		//Convert current_position data to mm and deg
		current_position[0] *= 1000;
		current_position[1] *= 1000;
		current_position[2] *= 1000;
		current_position[3] *= (180 / M_PI);
		current_position[4] *= (180 / M_PI);

		switch (m_pack.keycode)
		{
		case RapidMove:
			m_trajectory->RapidTrajectory(current_position, m_pack.vec1, m_pack.vel, acc);
			break;
		case IncRapidMove:
			m_trajectory->RapidTrajectory(rel_origin, m_pack.vec1, m_pack.vel, acc);
			break;
		case LinearMove:
			m_trajectory->LinearTrajectory(current_position, m_pack.vec1, m_pack.vel, acc, m_pack.bool2);
			break;
		case IncLinearMove:
			m_trajectory->LinearTrajectory(rel_origin, m_pack.vec1, m_pack.vel, acc, m_pack.bool2);
			break;
		case ArcMoveCenter:
			m_trajectory->ArcTrajectory(current_position, m_pack.vec1, m_pack.vec2, (Motion::Plane)m_pack.int1,
				m_pack.bool1, m_pack.vel, acc, m_pack.bool2);
			break;
		case ArcMoveRadius:
			m_trajectory->ArcTrajectory(current_position, m_pack.vec1, m_pack.coord1, (Motion::Plane)m_pack.int1,
				m_pack.bool1, m_pack.vel, acc, m_pack.bool2);
			break;
		case CircleMove:
			m_trajectory->CircleTrajectory(current_position, m_pack.vec2, (Motion::Plane)m_pack.int1,
				m_pack.bool1, m_pack.vel, acc, m_pack.bool2);
			break;
		case IncArcMoveCenter:
			m_trajectory->ArcTrajectory(rel_origin, m_pack.vec1, m_pack.vec2, (Motion::Plane)m_pack.int1,
				m_pack.bool1, m_pack.vel, acc, m_pack.bool2);
			break;
		case IncArcMoveRadius:
			m_trajectory->ArcTrajectory(rel_origin, m_pack.vec1, m_pack.coord1, (Motion::Plane)m_pack.int1,
				m_pack.bool1, m_pack.vel, acc, m_pack.bool2);
			break;
		case IncCircleMove:
			m_trajectory->CircleTrajectory(rel_origin, m_pack.vec2, (Motion::Plane)m_pack.int1,
				m_pack.bool1, m_pack.vel, acc, m_pack.bool2);
			break;
		case Dwell:
			m_trajectory->Dwell(current_position, m_pack.coord1);
			break;
		}

		//Get joint trajectory 
		std::vector<std::vector<double> > joint_values;
		m_trajectory->GetTrajectory(joint_values);

		if(!joint_values.empty())
		{
			m_last_position = joint_values.back();

			// Extend extrude to the same length as the number of joint values passed, 
			// so that for each trajectory path point, the simulator thread can pop
			// one joint values set and an extrude set.
			std::pair<bool, int> extrude_pair(m_pack.extrude, m_pack.keycode);
			std::vector< std::pair<bool, int> > extrude_vector(joint_values.size(), extrude_pair);
			// Same thing happens with the line
			std::vector<int> line_vector(joint_values.size(), m_pack.line);
			// Push values to simulator queue
			v_repExtPQSetJointTrajectory(joint_values, extrude_vector, line_vector);
		}
		else {
			// If instruction leads to no new joint values, update the line.
			v_repExtPQAdvanceLine(m_pack.line);
		}
	}
}

void Listener::PullCurrentPosition(double * cjp)
{
	std::vector<double> vcp_;
	v_repExtPQGetJointPositions(vcp_);
	std::copy(vcp_.data(), vcp_.data() + 5, cjp);
}

void Listener::Interpret(std::string JSON_msg)
{
	Deserialize(JSON_msg);

	PushAction();
}
