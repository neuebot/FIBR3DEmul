#pragma once

#include <vector>

namespace Motion {

	class VelocityProfile
	{
	public:
		VelocityProfile(double dt);

		void SetRampProfile(double vel[3], double acc, double length);
		void SetInterpolationProfile(double vel_i, double vel_f, double length);

		void GetProfile(double &path_time, std::vector<double> &path_pos);
		bool quadroots(double a, double b, double c, double &r1, double &r2);

	private:
		double m_dt;

		std::vector<double> m_path_pos;
		double m_path_time;
	};

}

