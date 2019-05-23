#pragma once

#include "Trajectory.h"

#include <deque>
#include <string>

enum KEYCODE {
	RapidMove = 1000,
	IncRapidMove = 1002,
	LinearMove = 1020,
	IncLinearMove = 1022,
	ArcMoveCenter = 1040,
	ArcMoveRadius = 1042,
	CircleMove = 1044,
	IncArcMoveCenter = 1046,
	IncArcMoveRadius = 1048,
	IncCircleMove = 1050,
	Dwell = 1080
};

struct package
{
	KEYCODE keycode;
	bool bool1;
	bool bool2;
	bool extrude;

	std::string text;

	double coord1;

	double vec1[5];
	double vec2[5];
	
	int int1;

	double vel[3]; //current, target and next velocity
	double acc[3]; //3-axis acceleration values

	int line;
};

class Listener
{
public:
	Listener();

	void Interpret(std::string JSON_msg);

private:
	void Deserialize(std::string JSON_msg);
	void PushAction();

	void PullCurrentPosition(double *cjp);

private:
	package m_pack;
	std::vector<double> m_last_position;

	std::shared_ptr<Motion::Trajectory> m_trajectory;

	//FIFO - push back, pop top
	//std::deque<std::vector<double> > m_joint_queue;
};