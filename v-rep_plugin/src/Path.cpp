#define _USE_MATH_DEFINES
#include <cmath>

#include "Path.h"

#include <algorithm>
#include <complex>

using namespace std;

template <typename T> int sgn(T val) {
	return (T(0) < val + std::numeric_limits<T>::epsilon()) - (val < T(0));
}

namespace Motion {

	Path::Path()
	{
		m_tolerance = 1e-6;
	}

	void Path::SetLinearPath(double xi[3], double xf[3])
	{
		double path[3];
		path[0] = xf[0] - xi[0];
		path[1] = xf[1] - xi[1];
		path[2] = xf[2] - xi[2];

		m_path_length = sqrt(pow(path[0], 2) + pow(path[1], 2) + pow(path[2], 2));
		if(m_path_length > m_tolerance)
		{
			m_unit_vec[0] = path[0] / m_path_length;
			m_unit_vec[1] = path[1] / m_path_length;
			m_unit_vec[2] = path[2] / m_path_length;
		}
		else {
			m_unit_vec[0] = 0.0;
			m_unit_vec[1] = 0.0;
			m_unit_vec[2] = 0.0;
		}
	}

	void Path::SetArcPath(double start[3], double finish[3], double center[3], Plane plane, bool clkwise)
	{
		//copy to member variable
		copy(center, center + 3, m_center);

		double s[2], f[2];
		switch (plane) {
		case XY:
			s[0] = start[0];
			s[1] = start[1];
			f[0] = finish[0];
			f[1] = finish[1];
			break;
		case XZ:
			s[0] = start[0];
			s[1] = start[2];
			f[0] = finish[0];
			f[1] = finish[2];
			break;
		case YZ:
			s[0] = start[1];
			s[1] = start[2];
			f[0] = finish[1];
			f[1] = finish[2];
			break;
		}

		//origin to center
		//origin to center
		double oo[2], ee[2];
		oo[0] = s[0] - center[0];
		oo[1] = s[1] - center[1];
		ee[0] = f[0] - center[0];
		ee[1] = f[1] - center[1];

		//radius
		m_radius = sqrt(pow(oo[0], 2) + pow(oo[1], 2));
		//start angle
		m_alpha = atan2(oo[1], oo[0]);
		//end angle - only required to calculate arc angle
		double beta = atan2(ee[1], ee[0]);

		//m_gamma the arc angle (beta - m_alpha)
		if (m_alpha < 0 && beta >= 0)
		{
			m_gamma = beta - (m_alpha + 2 * M_PI);
		}
		else if (m_alpha > 0 && beta <= 0)
		{
			m_gamma = (beta + 2 * M_PI) - m_alpha;
		}
		else
		{
			m_gamma = beta - m_alpha;
		}

		if (clkwise && m_gamma > 0)
		{
			m_gamma = m_gamma - 2 * M_PI;
		}
		else if (!clkwise && m_gamma < 0)
		{
			m_gamma = m_gamma + 2 * M_PI;
		}

		//Saving path length
		m_path_length = m_radius * abs(m_gamma);
	}

	void Path::SetArcPath(double start[3], double finish[3], double radius, Plane plane, bool clkwise)
	{
		//copy to member variable
		m_radius = std::abs(radius);

		double s[2], f[2];
		switch (plane) {
		case XY:
			s[0] = start[0];
			s[1] = start[1];
			f[0] = finish[0];
			f[1] = finish[1];
			break;
		case XZ:
			s[0] = start[0];
			s[1] = start[2];
			f[0] = finish[0];
			f[1] = finish[2];
			break;
		case YZ:
			s[0] = start[1];
			s[1] = start[2];
			f[0] = finish[1];
			f[1] = finish[2];
			break;
		}

		//Better explanation go to:
		//http://mathforum.org/library/drmath/view/53027.html
		//q = distance between start and end
		double q = sqrt(pow((f[0] - s[0]), 2) + pow((f[1] - s[1]), 2));

		//m = middle point between s and f
		//center = center point of arc
		double m[2], c[2];
		m[0] = (s[0] + f[0]) / 2;
		m[1] = (s[1] + f[1]) / 2;

		double term = (pow(radius, 2) - pow((q / 2), 2) < 0 ? 0.0 : pow(radius, 2) - pow((q / 2), 2));

		if (!clkwise && radius > 0 || clkwise && radius < 0)
		{
			c[0] = std::real(m[0] + sqrt(term) * (s[1] - f[1]) / q);
			c[1] = std::real(m[1] + sqrt(term) * (f[0] - s[0]) / q);
		}
		else
		{
			c[0] = std::real(m[0] - sqrt(term) * (s[1] - f[1]) / q);
			c[1] = std::real(m[1] - sqrt(term) * (f[0] - s[0]) / q);
		}

		//origin to center
		double oo[2], ee[2];
		oo[0] = s[0] - c[0];
		oo[1] = s[1] - c[1];
		ee[0] = f[0] - c[0];
		ee[1] = f[1] - c[1];

		//start angle
		m_alpha = atan2(oo[1], oo[0]);
		//end angle - only required to calculate arc angle
		double beta = atan2(ee[1], ee[0]);

		//m_gamma the arc angle (beta - m_alpha)
		if (m_alpha < 0 && beta >= 0)
		{
			m_gamma = beta - (m_alpha + 2 * M_PI);
		}
		else if (m_alpha >= 0 && beta < 0)
		{
			m_gamma = (beta + 2 * M_PI) - m_alpha;
		}
		else
		{
			m_gamma = beta - m_alpha;
		}

		if (clkwise && m_gamma > 0)
		{
			m_gamma = m_gamma - 2 * M_PI;
		}
		else if (!clkwise && m_gamma < 0)
		{
			m_gamma = m_gamma + 2 * M_PI;
		}

		//Saving path length
		m_path_length = m_radius * abs(m_gamma);

		switch (plane) {
		case XY:
			m_center[0] = c[0];
			m_center[1] = c[1];
			m_center[2] = start[2];
			break;
		case XZ:
			m_center[0] = c[0];
			m_center[1] = start[1];
			m_center[2] = c[1];
			break;
		case YZ:
			m_center[0] = start[0];
			m_center[1] = c[0];
			m_center[2] = c[1];
			break;
		}
	}

	void Path::SetCirclePath(double start[3], double center[3], Plane plane, bool clkwise)
	{
		//copy to member variable
		copy(center, center + 3, m_center);

		double s[2];
		switch (plane) {
		case XY:
			s[0] = start[0];
			s[1] = start[1];
			break;
		case XZ:
			s[0] = start[0];
			s[1] = start[2];
			break;
		case YZ:
			s[0] = start[1];
			s[1] = start[2];
			break;
		}

		//origin to center
		//origin to center
		double oo[2];
		oo[0] = s[0] - center[0];
		oo[1] = s[1] - center[1];

		//radius
		m_radius = sqrt(pow(oo[0], 2) + pow(oo[1], 2));
		//start angle
		m_alpha = atan2(oo[1], oo[0]);

		//m_gamma the arc angle (beta - m_alpha) - full circle
		if (!clkwise) 
		{
			m_gamma = 2 * M_PI;
		}
		else
		{
			m_gamma = -2 * M_PI;
		}

		//Saving path length
		m_path_length = m_radius * abs(m_gamma);
	}

	void Path::SetBCPath(double xi[2], double xf[2])
	{
		double bc_path[2];
		//B (not cyclic)
		bc_path[0] = xf[0] - xi[0];
		//C (cyclic)
		//Since the joint is cyclic, it turns beyond 360, but it still receives joint angles within -180 and 180.
		//To avoid weird turns, we check the number of turns from the input value and apply it to the final value.
		int turns = std::floor(xi[1] / 360.0);
		xf[1] += turns * 360;
		double corC = xf[1] - xi[1];
		//Since C is cyclic, and the default behavior is to run the shortest distance.
		//E.G If the joint goes from -170 to 170, it should do -20 instead of +340					
		double invC = corC - sgn(corC) * 360;// *2 * M_PI;
		// When both values are too close we select the positive
		if (std::abs(invC) - std::abs(corC) < m_tolerance) {
			bc_path[1] = (corC > invC ? corC : invC);
		}
		else {
			bc_path[1] = (std::abs(invC) > std::abs(corC) ? corC : invC);
		}
		

		m_bc_path_length = sqrt(pow(bc_path[0], 2) + pow(bc_path[1], 2));
		if (m_bc_path_length < m_tolerance) {
			m_unit_bc_vec[0] = 0.0;
			m_unit_bc_vec[1] = 0.0;
		}
		else {
			m_unit_bc_vec[0] = bc_path[0] / m_bc_path_length;
			m_unit_bc_vec[1] = bc_path[1] / m_bc_path_length;
		}
	}

	double Path::get_path_length()
	{
		return m_path_length;
	}

	double Path::get_bc_path_length()
	{
		return m_bc_path_length;
	}

	void Path::GetLinearPath(double * u_vec)
	{
		copy(m_unit_vec, m_unit_vec + 3, u_vec);
	}

	void Path::GetArcPath(double & gamma, double & alpha, double * center)
	{
		gamma = m_gamma;
		alpha = m_alpha;
		copy(m_center, m_center + 3, center);
	}

	void Path::GetArcPath(double & gamma, double & alpha, double & radius)
	{
		gamma = m_gamma;
		alpha = m_alpha;
		radius = m_radius;
	}
	void Path::GetBCPath(double * u_bc_vec)
	{
		copy(m_unit_bc_vec, m_unit_bc_vec + 2, u_bc_vec);
	}
}
