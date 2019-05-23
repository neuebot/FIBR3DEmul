#include "Trajectory.h"

#include <algorithm>

#define _USE_MATH_DEFINES
#include <math.h>

using namespace std;

static double TOLERANCE = 0.00001;

template <typename T> T sgn_tol(T val) {
	return (T(TOLERANCE) < val) - (val < -T(TOLERANCE));
}

template <typename T> bool abs_compare(T a, T b)
{
	return (std::abs(a) < std::abs(b));
}

namespace Motion {

	Trajectory::Trajectory(double dt) : 
		m_dt(dt)
	{
		m_path = make_unique<Path>();
		m_vel_prof = make_unique<VelocityProfile>(dt);
	}

	void Trajectory::RapidTrajectory(double start[5], double finish[5], double vel[3], double acc)
	{
		//Since this function combines the path / vel profile part.
		vel[0] = 0.0;
		vel[2] = 0.0;

		//Split path into XYZ and BC
		std::vector<double> dist(5);
		for (size_t i = 0; i < 4; ++i) {
			dist[i] = finish[i] - start[i];
		}

		double 
			xi = start[4], 
			xf = finish[4];
		//C (cyclic)
		//Since the joint is cyclic, it turns beyond 360, but it still receives joint angles within -180 and 180.
		//To avoid weird turns, we check the number of turns from the input value and apply it to the final value.
		int turns = std::floor(xi / 360.0);
		xf += turns * 360;
		double corC = xf - xi;
		//Since C is cyclic, and the default behavior is to run the shortest distance.
		//E.G If the joint goes from -170 to 170, it should do -20 instead of +340					
		double invC = corC - sgn_tol(corC) * 360;// *2 * M_PI;
		dist[4] = (std::abs(invC) > std::abs(corC) ? corC : invC);

		//Finding maximum length
		auto max_it = std::max_element(dist.begin(), dist.end(), abs_compare<double>);
		size_t index = std::distance(dist.begin(), max_it);
		double max_len = std::abs(dist[index]);

		//maximum acceleration allowed with signals
		double a[3];
		a[0] = acc;
		a[1] = 0.0;
		a[2] = -acc;

		//distance, time of each segment
		double
			ds[3] = { 0.0, 0.0, 0.0 },
			ts[3] = { 0.0, 0.0, 0.0 };

		//check what ramps are needed
		if (abs(vel[0] - vel[1]) > TOLERANCE) //start ramp
		{
			ts[0] = abs(vel[1] - vel[0]) / acc;
			ds[0] = vel[0] * ts[0] + 0.5 * a[0] * pow(ts[0], 2);
		}
		if (abs(vel[1] - vel[2]) > TOLERANCE) //end ramp
		{
			ts[2] = abs(vel[2] - vel[1]) / acc;
			ds[2] = vel[1] * ts[2] + 0.5 * a[2] * pow(ts[2], 2);
		}

		//If distance covered in ramps is greater than total distance
		// In this case the actuators do not reach the target velocity,
		// instead the actuators reach an intermediate velocity that 
		// allows it to go from current to the needed velocity and cover
		// the path, respecting the accelerations imposed.
		if (max_len >= ds[0] + ds[2])
		{
			//constant velocity segment length
			ds[1] = max_len - (ds[0] + ds[2]);
			//check time needed at constant speed
			ts[1] = ds[1] / vel[1];
		}
		else
		{
			if (ts[2] < TOLERANCE)
			{
				ts[0] = (2 * max_len) / (vel[2] + vel[0]);
				a[0] = (vel[2] - vel[0]) / ts[0];
				ds[0] = vel[0] * ts[0] + 0.5 * a[0] * pow(ts[0], 2);
				ds[1] = 0.0;
				ds[2] = 0.0;
				ts[1] = 0.0;
				ts[2] = 0.0;
			}
			else if (ts[0] < TOLERANCE)
			{
				ts[2] = (2 * max_len) / (vel[2] + vel[0]);
				a[2] = (vel[2] - vel[0]) / ts[2];
				ds[2] = vel[0] * ts[2] + 0.5 * a[2] * pow(ts[2], 2);
				ds[0] = 0.0;
				ds[1] = 0.0;
				ts[0] = 0.0;
				ts[1] = 0.0;
			}
			else
			{
				//intermediate point in path space
				double p_i = ((pow(vel[2], 2) - pow(vel[0], 2)) - 2 * a[2] * max_len) / (2 * a[0] - 2 * a[2]);
				//polynomial solving coefficients
				double as = 0.5*a[0];
				double bs = vel[0];
				double cs = -p_i;

				double r1, r2;
				if (m_vel_prof->quadroots(as, bs, cs, r1, r2))
				{
					//Use fastest solution
					// total time1 = ts + tf = ts + (vf - vi)/af = ts + (vf - (vs + as*ts))/af
					double
						t1 = r1 + (vel[2] - (vel[0] + a[0] * r1)) / a[2],
						t2 = r2 + (vel[2] - (vel[0] + a[0] * r2)) / a[2];

					double sol = 0.0;
					if (t1 > 0 && t2 > 0)
						sol = (t1 <= t2 ? r1 : r2);
					else if (t1 > 0)
						sol = r1;
					else if (t2 > 0)
						sol = r2;
					else
						throw runtime_error("No real roots found when computing Ramp Velocity Profile found");


					vel[1] = vel[0] + a[0] * sol;

					//New times
					ts[0] = sol;
					ts[1] = 0.0;
					ts[2] = (vel[2] - vel[1]) / a[2];

					//New distances
					ds[0] = vel[0] * ts[0] + 0.5 * a[0] * pow(ts[0], 2);
					ds[1] = 0.0;
					ds[2] = vel[1] * ts[2] + 0.5 * a[2] * pow(ts[2], 2);
				}
				else
				{
					throw runtime_error("No real roots found when computing Ramp Velocity Profile found");
				}
			}
		}
		//total time of all segments 
		double tt = ts[0] + ts[1] + ts[2];
		//number of trajectory samples
		int num_samples = ceil(tt / m_dt);

		if (num_samples > 0)
		{
			//Initialize vector of joint coordinates

			m_joints.assign(num_samples, vector<double>(5, 0.0));
			for (size_t j = 0; j < 5; ++j) {
				
				double ratio = dist[j] / max_len;
				double velj[3] = { vel[0] * ratio, vel[1] * ratio , vel[2] * ratio };
				double accj[3] = { a[0] * ratio, a[1] * ratio , a[2] * ratio };


				for (size_t i = 0; i < num_samples-1; i++)
				{
					//Sample time
					//Starts at instant 1 instead of 0
					//Stops at final time but always reaches reference position
					double ti = (i+1) * m_dt;
					if (ti < ts[0])
					{
						m_joints[i][j] = start[j] + (velj[0] * ti + 0.5 * accj[0] * pow(ti, 2));
						m_joints[i][j] = (j < 3 ? m_joints[i][j] / 1000 : m_joints[i][j] * (M_PI / 180));
					}
					else if (ti >= ts[0] && ti < (ts[1] + ts[0]))
					{
						m_joints[i][j] = start[j] + (ds[0] * ratio + velj[1] * (ti - ts[0]));
						m_joints[i][j] = (j < 3 ? m_joints[i][j] / 1000 : m_joints[i][j] * (M_PI / 180));
					}
					else
					{
						double tf = ti - (ts[1] + ts[0]);
						m_joints[i][j] = start[j] + ((ds[0] + ds[1])*ratio + velj[1] * tf + 0.5 * accj[2] * pow(tf, 2));
						m_joints[i][j] = (j < 3 ? m_joints[i][j] / 1000 : m_joints[i][j] * (M_PI / 180));
					}
				}
				m_joints[num_samples - 1][j] = finish[j];
				m_joints[num_samples - 1][j] = (j < 3 ? m_joints[num_samples - 1][j] / 1000 : m_joints[num_samples - 1][j] * (M_PI / 180));
			}
		}
	}

	void Trajectory::LinearTrajectory(double start[5], double finish[5], double vel[3], double acc, bool interpolated)
	{
		//Split path into XYZ and BC
		double
			start_xyz[3], finish_xyz[3],
			start_bc[2], finish_bc[2];

		vector_split(start, start_xyz, start_bc);
		vector_split(finish, finish_xyz, finish_bc);

		//Set path
		m_path->SetLinearPath(start_xyz, finish_xyz);
		m_path->SetBCPath(start_bc, finish_bc);

		double xyz_plen = m_path->get_path_length();
		double bc_plen = m_path->get_bc_path_length();
		double combined_plen = sqrt(pow(xyz_plen,2) + pow(bc_plen,2));

		//Set profile
		if (interpolated)
		{
			m_vel_prof->SetInterpolationProfile(vel[0], vel[1], combined_plen);
		}
		else
		{
			m_vel_prof->SetRampProfile(vel, acc, combined_plen);
		}

		//Get path vector
		double uvec[3], uvec_bc[2];
		m_path->GetLinearPath(uvec);
		m_path->GetBCPath(uvec_bc);

		//Get profile path positions
		double path_time;
		vector<double> path_pos;
		m_vel_prof->GetProfile(path_time, path_pos);

		//Relation between bc_path and path -> pos_bc[i] = pos[i] * racio = pos[i] * (len_ac / len)
		double xyz_racio = m_path->get_path_length() / combined_plen;
		double bc_racio = m_path->get_bc_path_length() / combined_plen;

		//Initialize vector of joint coordinates
		m_joints.assign(path_pos.size(), vector<double>(5, 0.0));
		for (size_t i = 0; i < path_pos.size(); ++i)
		{
			m_joints[i][0] = (uvec[0] * path_pos[i] * xyz_racio + start_xyz[0]) / 1000;
			m_joints[i][1] = (uvec[1] * path_pos[i] * xyz_racio + start_xyz[1]) / 1000;
			m_joints[i][2] = (uvec[2] * path_pos[i] * xyz_racio + start_xyz[2]) / 1000;

			m_joints[i][3] = (uvec_bc[0] * path_pos[i] * bc_racio + start_bc[0]) * (M_PI / 180);
			m_joints[i][4] = (uvec_bc[1] * path_pos[i] * bc_racio + start_bc[1]) * (M_PI / 180);
		}
	}

	void Trajectory::ArcTrajectory(double start[5], double finish[5], double center[5], Plane plane, bool clkwise, double vel[3], double acc, bool interpolation)
	{
		//Split path into XYZ and BC
		double
			start_xyz[3], finish_xyz[3], center_xyz[3],
			start_bc[2], finish_bc[2], center_bc[2];

		vector_split(start, start_xyz, start_bc);
		vector_split(finish, finish_xyz, finish_bc);
		vector_split(center, center_xyz, center_bc);

		//Center relative to start point
		center_xyz[0] += start_xyz[0];
		center_xyz[1] += start_xyz[1];
		center_xyz[2] += start_xyz[2];

		//Set path
		m_path->SetArcPath(start_xyz, finish_xyz, center_xyz, plane, clkwise);
		m_path->SetBCPath(start_bc, finish_bc);

		double xyz_plen = m_path->get_path_length();
		double bc_plen = m_path->get_bc_path_length();
		double combined_plen = sqrt(pow(xyz_plen, 2) + pow(bc_plen, 2));

		//Set velocity profile
		if (interpolation)
		{
			m_vel_prof->SetInterpolationProfile(vel[0], vel[2], m_path->get_path_length());
		}
		else
		{
			m_vel_prof->SetRampProfile(vel, acc, m_path->get_path_length());
		}

		//Get path information
		double gamma, alpha, radius;
		double uvec_bc[2];
		m_path->GetArcPath(gamma, alpha, radius);
		m_path->GetBCPath(uvec_bc);

		//Get profile path positions
		double path_time;
		vector<double> path_pos;
		m_vel_prof->GetProfile(path_time, path_pos);

		//Relation between bc_path and path -> pos_bc[i] = pos[i] * racio = pos[i] * (len_bc / len)
		//double path_racio = m_path->get_bc_path_length() / m_path->get_path_length();

		//Relation between bc_path and path -> pos_bc[i] = pos[i] * racio = pos[i] * (len_bc / len)
		double xyz_racio = m_path->get_path_length() / combined_plen;
		double bc_racio = m_path->get_bc_path_length() / combined_plen;

		//Sample path according to the sample times
		vector<double> ti = linspace(0, 1, path_pos.size());

		//Initialize vector of joint coordinates
		m_joints.assign(path_pos.size(), vector<double>(5, 0.0));
		
		int c1, c2, c3;
		switch (plane)
		{
		case Motion::XY:
			c1 = 0; //x
			c2 = 1; //y
			c3 = 2; //z
			break;
		case Motion::XZ:
			c1 = 0; //x
			c2 = 2; //z
			c3 = 1; //y
			break;
		case Motion::YZ:
			c1 = 1; //y
			c2 = 2; //z
			c3 = 0; //x
			break;
		}

		for (size_t i = 0; i < path_pos.size(); ++i)
		{
			double path = alpha + gamma * ti[i];
			m_joints[i][c1] = (radius * cos(path) * xyz_racio + center_xyz[c1]) / 1000;
			m_joints[i][c2] = (radius * sin(path) * xyz_racio + center_xyz[c2]) / 1000;
			m_joints[i][c3] = (start_xyz[c3]) / 1000;

			m_joints[i][3] = (uvec_bc[0] * path_pos[i] * bc_racio + start_bc[0]) * (M_PI / 180);
			m_joints[i][4] = (uvec_bc[1] * path_pos[i] * bc_racio + start_bc[1]) * (M_PI / 180);
		}
	}

	void Trajectory::ArcTrajectory(double start[5], double finish[5], double radius, Plane plane, bool clkwise, double vel[3], double acc, bool interpolation)
	{
		//Split path into XYZ and BC
		double
			start_xyz[3], finish_xyz[3],
			start_bc[2], finish_bc[2];

		vector_split(start, start_xyz, start_bc);
		vector_split(finish, finish_xyz, finish_bc);

		//Set path
		m_path->SetArcPath(start_xyz, finish_xyz, radius, plane, clkwise);
		m_path->SetBCPath(start_bc, finish_bc);
		//Negative radius case
		radius = std::abs(radius);

		//Relation between bc_path and path -> pos_bc[i] = pos[i] * racio = pos[i] * (len_bc / len)
		double xyz_plen = m_path->get_path_length();
		double bc_plen = m_path->get_bc_path_length();
		double combined_plen = sqrt(pow(xyz_plen, 2) + pow(bc_plen, 2));

		//Set velocity profile
		if (interpolation)
		{
			m_vel_prof->SetInterpolationProfile(vel[0], vel[2], m_path->get_path_length());
		}
		else
		{
			m_vel_prof->SetRampProfile(vel, acc, m_path->get_path_length());
		}

		//Get path information
		double gamma, alpha, center[3];
		double uvec_bc[2];
		m_path->GetArcPath(gamma, alpha, center);
		m_path->GetBCPath(uvec_bc);

		//Get profile path positions
		double path_time;
		vector<double> path_pos;
		m_vel_prof->GetProfile(path_time, path_pos);

		//Relation between bc_path and path -> pos_bc[i] = pos[i] * racio = pos[i] * (len_bc / len)
		double xyz_racio = m_path->get_path_length() / combined_plen;
		double bc_racio = m_path->get_bc_path_length() / combined_plen;

		//Sample path according to the sample times
		vector<double> ti = linspace(0, 1, path_pos.size());

		//Initialize vector of joint coordinates
		m_joints.assign(path_pos.size(), vector<double>(5, 0.0));

		int c1, c2, c3;
		switch (plane)
		{
		case Motion::XY:
			c1 = 0; //x
			c2 = 1; //y
			c3 = 2; //z
			break;
		case Motion::XZ:
			c1 = 0; //x
			c2 = 2; //z
			c3 = 1; //y
			break;
		case Motion::YZ:
			c1 = 1; //y
			c2 = 2; //z
			c3 = 0; //x
			break;
		}

		for (size_t i = 0; i < path_pos.size(); ++i)
		{
			double path = alpha + gamma * ti[i];
			m_joints[i][c1] = (radius * cos(path) * xyz_racio + center[c1]) / 1000;
			m_joints[i][c2] = (radius * sin(path) * xyz_racio + center[c2]) / 1000;
			m_joints[i][c3] = (start[c3]) / 1000;

			m_joints[i][3] = (uvec_bc[0] * path_pos[i] * bc_racio + start_bc[0]) * (M_PI / 180);
			m_joints[i][4] = (uvec_bc[1] * path_pos[i] * bc_racio + start_bc[1]) * (M_PI / 180);
		}
	}

	void Trajectory::CircleTrajectory(double start[5], double center[5], Plane plane, bool clkwise, double vel[3], double acc, bool interpolation)
	{
		//Split path into XYZ and BC
		double
			start_xyz[3], center_xyz[3],
			start_bc[2], center_bc[2];

		vector_split(start, start_xyz, start_bc);
		vector_split(center, center_xyz, center_bc);

		//Center relative to start point
		center_xyz[0] += start_xyz[0];
		center_xyz[1] += start_xyz[1];
		center_xyz[2] += start_xyz[2];

		//Set path
		m_path->SetCirclePath(start_xyz, center_xyz, plane, clkwise);
		//m_path->SetBCPath(start_bc, finish_bc);

		double xyz_plen = m_path->get_path_length();
		//double bc_plen = m_path->get_bc_path_length();
		double combined_plen = sqrt(pow(xyz_plen, 2));// +pow(bc_plen, 2));

													  //Set velocity profile
		if (interpolation)
		{
			m_vel_prof->SetInterpolationProfile(vel[0], vel[2], m_path->get_path_length());
		}
		else
		{
			m_vel_prof->SetRampProfile(vel, acc, m_path->get_path_length());
		}

		//Get path information
		double gamma, alpha, radius;
		double uvec_bc[2];
		m_path->GetArcPath(gamma, alpha, radius);
		//m_path->GetBCPath(uvec_bc);


		//Get profile path positions
		double path_time;
		vector<double> path_pos;
		m_vel_prof->GetProfile(path_time, path_pos);

		//Relation between bc_path and path -> pos_bc[i] = pos[i] * racio = pos[i] * (len_bc / len)
		//double path_racio = m_path->get_bc_path_length() / m_path->get_path_length();

		//Relation between bc_path and path -> pos_bc[i] = pos[i] * racio = pos[i] * (len_bc / len)
		double xyz_racio = m_path->get_path_length() / combined_plen;
		//double bc_racio = m_path->get_bc_path_length() / combined_plen;

		//Sample path according to the sample times
		vector<double> ti = linspace(0, 1, path_pos.size());

		//Initialize vector of joint coordinates
		m_joints.assign(path_pos.size(), vector<double>(5, 0.0));

		int c1, c2, c3;
		switch (plane)
		{
		case Motion::XY:
			c1 = 0; //x
			c2 = 1; //y
			c3 = 2; //z
			break;
		case Motion::XZ:
			c1 = 0; //x
			c2 = 2; //z
			c3 = 1; //y
			break;
		case Motion::YZ:
			c1 = 1; //y
			c2 = 2; //z
			c3 = 0; //x
			break;
		}

		for (size_t i = 0; i < path_pos.size(); ++i)
		{
			double path = alpha + gamma * ti[i];
			m_joints[i][c1] = (radius * cos(path) * xyz_racio + center_xyz[c1]) / 1000;
			m_joints[i][c2] = (radius * sin(path) * xyz_racio + center_xyz[c2]) / 1000;
			m_joints[i][c3] = (start_xyz[c3]) / 1000;

			//m_joints[i][3] = (uvec_bc[0] * path_pos[i] * bc_racio + start_bc[0]) * (M_PI / 180);
			//m_joints[i][4] = (uvec_bc[1] * path_pos[i] * bc_racio + start_bc[1]) * (M_PI / 180);
		}
	}

	void Trajectory::Dwell(double start[5], double time)
	{
		//Calculate number of dwell steps
		int it = ceil(time / m_dt);

		//Initialize vector of joint coordinates
		m_joints.assign(it, vector<double>(5, 0.0));
		
		for (size_t i = 0; i < it; ++i)
		{
			m_joints[i][0] = start[0] / 1000;
			m_joints[i][1] = start[1] / 1000;
			m_joints[i][2] = start[2] / 1000;
			m_joints[i][3] = start[3] / 1000;
			m_joints[i][4] = start[4] / 1000;
		}
	}

	void Trajectory::GetTrajectory(std::vector<std::vector<double> > &joints)
	{
		joints = m_joints;
	}

	std::vector<double> Trajectory::linspace(double a, double b, size_t N)
	{
		vector<double> xs(N);
		vector<double>::iterator it;
		
		double h = (b - a) / N;
		double val;
		for (it = xs.begin(), val = a; it != xs.end(); ++it, val += h)
		{
			*it = val;
		}

		return xs;
	}

	void Trajectory::vector_split(double in[5], double out1[3], double out2[2])
	{
		std::copy(in, in + 3, out1);
		std::copy(in + 3, in + 5, out2);
	}
}
