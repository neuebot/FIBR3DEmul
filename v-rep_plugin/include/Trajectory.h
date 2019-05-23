#pragma once

#include "Path.h"
#include "VelocityProfile.h"

#include <memory>

//! Trajectory Class
// Generates Robot trajectories for the robot based on a specified path and velocity profile.
//
// Path: defines the geometric shape of the trajectory.
// VelocityProfile: defines the timing law on how the path is followed.
//
// Based on the input commands the Trajectory class outputs an array of [x,y,z] actuator coordinates
// sampled at a specified 'dt'. This 'dt' should be coordinated with the simulation cycle.

namespace Motion {

	class Trajectory
	{
	public:
		Trajectory(double dt);

		void RapidTrajectory(double start[5], double finish[5], double vel[3], double acc);
		void LinearTrajectory(double start[5], double finish[5], double vel[3], double acc, bool interpolation);
		void ArcTrajectory(double start[5], double finish[5], double center[3], Plane plane, bool clkwise, double vel[3], double acc, bool interpolation);
		void ArcTrajectory(double start[5], double finish[5], double radius, Plane plane, bool clkwise, double vel[3], double acc, bool interpolation);
		void CircleTrajectory(double start[5], double center[5], Plane plane, bool clkwise, double vel[3], double acc, bool interpolation);
		void Dwell(double start[5], double time);

		void GetTrajectory(std::vector<std::vector<double> > &joints);

	private:
		std::vector<double> linspace(double a, double b, size_t N);
		void vector_split(double in[5], double out1[3], double out2[2]);

	private:
		//sampling time
		double m_dt; 

		std::unique_ptr<Path> m_path;
		std::unique_ptr<VelocityProfile> m_vel_prof;

		//vector of vector of 5 joint double values
		std::vector<std::vector<double> > m_joints;
	};

}

