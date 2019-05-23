#include "VelocityProfile.h"

#include <cmath>

using namespace std;

static double TOLERANCE = 0.00001;

template <typename T> int sgn(T val) {
	return (T(0) < val + std::numeric_limits<T>::epsilon()) - (val < T(0));
}

template <typename T> T sgn_tol(T val) {
	return (T(TOLERANCE) < val) - (val < -T(TOLERANCE));
}

namespace Motion {

	VelocityProfile::VelocityProfile(double dt) :
		m_dt(dt)
	{
	}

	void VelocityProfile::SetRampProfile(double vel[3], double acc, double length)
	{
		m_path_pos.clear();

		//maximum acceleration allowed with signals
		double a[3];
		a[0] = sgn_tol(vel[1] - vel[0]) * acc;
		a[1] = 0.0;
		a[2] = sgn_tol(vel[2] - vel[1]) * acc;

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
		if(length >= ds[0] + ds[2])
		{ 
			//constant velocity segment length
			ds[1] = length - (ds[0] + ds[2]);
			//check time needed at constant speed
			ts[1] = ds[1] / vel[1];
		}
		else
		{
			if (ts[2] < TOLERANCE)
			{
				ts[0] = (2 * length) / (vel[2] + vel[0]);
				a[0] = (vel[2] - vel[0]) / ts[0];
				ds[0] = vel[0] * ts[0] + 0.5 * a[0] * pow(ts[0], 2);
				ds[1] = 0.0; 
				ds[2] = 0.0;
				ts[1] = 0.0;
				ts[2] = 0.0;
			}
			else if(ts[0] < TOLERANCE)
			{
				ts[2] = (2 * length) / (vel[2] + vel[0]);
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
				double p_i = ((pow(vel[2], 2) - pow(vel[0], 2)) - 2 * a[2] * length) / (2 * a[0] - 2 * a[2]);
				//polynomial solving coefficients
				double as = 0.5*a[0];
				double bs = vel[0];
				double cs = -p_i;

				double r1, r2;
				if (quadroots(as, bs, cs, r1, r2))
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

		if(num_samples > 0) 
		{
			m_path_pos.assign(num_samples, 0.0);
			m_path_time = num_samples * m_dt;

			for (size_t i = 0; i < num_samples - 1; i++)
			{
				//sample time
				double ti = (i+1) * m_dt;
				if (ti < ts[0])
				{
					m_path_pos[i] = vel[0] * ti + 0.5 * a[0] * pow(ti, 2);
				}
				else if (ti >= ts[0] && ti < (ts[1] + ts[0]))
				{
					m_path_pos[i] = ds[0] + vel[1] * (ti - ts[0]);
				}
				else
				{
					double tf = ti - (ts[1] + ts[0]);
					m_path_pos[i] = (ds[0] + ds[1]) + vel[1] * tf + 0.5 * a[2] * pow(tf, 2);
				}
			}
			m_path_pos[num_samples - 1] = length;
		}
	}

	void VelocityProfile::SetInterpolationProfile(double vel_i, double vel_f, double length)
	{
		//Total time of path at constant acceleration from vel_i to vel_f
		m_path_time = (2 * length) / (vel_f + vel_i);
		//Acceleration required
		double acc = (vel_f - vel_i) / m_path_time;

		//Path samples
		double num_samples = ceil(m_path_time / m_dt);
		//Array of path positions
		m_path_pos.assign(num_samples, 0.0);

		for (size_t i = 0; i < num_samples; i++)
		{
			//sample time
			double ti = i * m_dt;
			//acceleration formula
			m_path_pos[i] = vel_i * ti + 0.5 * acc * pow(ti, 2);
		}
	}

	void VelocityProfile::GetProfile(double & path_time, std::vector<double>& path_pos)
	{
		path_time = m_path_time;
		path_pos = m_path_pos;
	}

	bool VelocityProfile::quadroots(double a, double b, double c, double & r1, double & r2)
	{
		//returns false if there is no real only root
		double determinant = pow(b,2) - 4 * a*c;

		if (determinant > 0) {
			//Roots are real and different
			r1 = (-b + sqrt(determinant)) / (2 * a);
			r2 = (-b - sqrt(determinant)) / (2 * a);
		}

		else if (determinant == 0) {
			//Roots are real and same.
			r1 = (-b + sqrt(determinant)) / (2 * a);
		}

		else {
			return false;
		}

		return true;
	}

}