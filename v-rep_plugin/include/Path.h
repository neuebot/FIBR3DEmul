#pragma once

namespace Motion {

	enum Plane {XY, XZ, YZ};

	class Path
	{
	public:
		Path();

		enum Planes { XY, XZ, YZ };
	
		void SetLinearPath(double xi[3], double xf[3]);
		void SetArcPath(double start[3], double finish[3], double center[3], Plane plane, bool clkwise);
		void SetArcPath(double start[3], double finish[3], double radius, Plane plane, bool clkwise);
		void SetCirclePath(double start[3], double center[3], Plane plane, bool clkwise);
		// 5-axis functionality
		void SetBCPath(double xi[2], double xf[2]);
	
		void GetLinearPath(double *u_vec);
		void GetArcPath(double &gamma, double &alpha, double *center);
		void GetArcPath(double &gamma, double &alpha, double &radius);
		// 5-axis functionality
		void GetBCPath(double *u_bc_vec);

		double get_path_length();
		// 5-axis functionality
		double get_bc_path_length();

	private:
		//Both
		double m_tolerance;
		double m_path_length;
		double m_bc_path_length;

		//Linear path
		double m_unit_vec[3];
		double m_unit_bc_vec[2];

		//Arc path
		double m_gamma; //arc angle
		double m_alpha; //start angle
		double m_center[3];
		double m_radius;

	};

}
