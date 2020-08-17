#include "Extruder.h"

#include "simConst.h"

using namespace std;

Extruder::Extruder(const string &ename, const string &bname) :
	ext_name(ename),
	bed_name(bname)
{
	vector<float> color_red = { 1.000f, 0.2706f, 0.000f };
	vector<float> color_green = { 0.0274f, 0.8039f, 0.1961f };
	vector<float> color_blue = { 0.1176f, 0.5647f, 1.000f };
	vector<float> color_yellow = { 1.000f, 1.000f, 0.000f };

	color_map.emplace(std::pair<int, vector<float> >(0, color_red));
	color_map.emplace(std::pair<int, vector<float> >(1, color_green));
	color_map.emplace(std::pair<int, vector<float> >(2, color_blue));

	//drawobj_mode = (int)sim_drawing_spherepoints;
	drawobj_mode = (int)sim_drawing_cubepoints;

	//color - RGB
	std::copy(color_red.begin(), color_red.end(), drawobj_color);

	//Filament size meters
	drawobj_size = 0.0015f;

	//Filament resolution
	filament_resolution = 0.5;

	//Alternative filament
	alternative_color1 = color_yellow;
	alternative_size1 = 0.0004;

	//Octree name
	octree_name = "FilamentOctree";
	octree_leaf_size = 0.01;

	//if vector of points is float
	// 200.000.000 points represent ~2400mb
	buffer_size = 200000000;
}

void Extruder::SetInitialTransformation(const std::vector<float>& transf)
{
	init_transf = transf;
}

std::vector<float> Extruder::GetInitialTransformation()
{
	return init_transf;
}

void Extruder::CalculateWorldPoint(const std::vector<double>& cpos, float wp[3])
{
	wp[0] = (cpos[0] * init_transf[0] + cpos[1] * init_transf[1] + cpos[2] * init_transf[2]) + init_transf[3];
	wp[1] = (cpos[0] * init_transf[4] + cpos[1] * init_transf[5] + cpos[2] * init_transf[6]) + init_transf[7];
	wp[2] = (cpos[0] * init_transf[8] + cpos[1] * init_transf[9] + cpos[2] * init_transf[10]) + init_transf[11];
}

void Extruder::CalculateWorldPoint(float cpos[3], float wp[3])
{
	wp[0] = (cpos[0] * init_transf[0] + cpos[1] * init_transf[1] + cpos[2] * init_transf[2]) + init_transf[3];
	wp[1] = (cpos[0] * init_transf[4] + cpos[1] * init_transf[5] + cpos[2] * init_transf[6]) + init_transf[7];
	wp[2] = (cpos[0] * init_transf[8] + cpos[1] * init_transf[9] + cpos[2] * init_transf[10]) + init_transf[11];
}

void Extruder::CalculateRealPoint(const std::vector<double>& cpos, std::vector<double>& bpos)
{
	///Calculates the actual coordinates of the extrusion point in the bed
	/// uses the XYZ and BC coordinates (check MATLAB NewCoordinates.m)

	bpos.assign(3, 0.0);
	//bpos[0] = (Z*sin(B)*pow(cos(C), 2) + X*cos(B)*cos(C) + Z*sin(B)*pow(sin(C), 2) + Y*cos(B)*sin(C)) / ((pow(cos(B), 2) + pow(sin(B), 2))*(pow(cos(C), 2) + pow(sin(C), 2))); //X 
	//bpos[1] = (Y*cos(C) - X*sin(C)) / (pow(cos(C), 2) + pow(sin(C), 2)); //Y 
	//bpos[2] = -(X*cos(C)*sin(B) + Y*sin(B)*sin(C) - Z*cos(B)*pow(cos(C), 2) - Z*cos(B)*pow(sin(C), 2)) / ((pow(cos(B), 2) + pow(sin(B), 2))*(pow(cos(C), 2) + pow(sin(C), 2))); //Z
	bpos[0] = (cpos[2] *sin(cpos[3])*pow(cos(cpos[4]), 2) + cpos[0]*cos(cpos[3])*cos(cpos[4]) + cpos[2] *sin(cpos[3])*pow(sin(cpos[4]), 2) + cpos[1]*cos(cpos[3])*sin(cpos[4])) / ((pow(cos(cpos[3]), 2) + pow(sin(cpos[3]), 2))*(pow(cos(cpos[4]), 2) + pow(sin(cpos[4]), 2))); //X 
	bpos[1] = (cpos[1]*cos(cpos[4]) - cpos[0]*sin(cpos[4])) / (pow(cos(cpos[4]), 2) + pow(sin(cpos[4]), 2)); //Y 
	bpos[2] = -(cpos[0]*cos(cpos[4])*sin(cpos[3]) + cpos[1]*sin(cpos[3])*sin(cpos[4]) - cpos[2]*cos(cpos[3])*pow(cos(cpos[4]), 2) - cpos[2] *cos(cpos[3])*pow(sin(cpos[4]), 2)) / ((pow(cos(cpos[3]), 2) + pow(sin(cpos[3]), 2))*(pow(cos(cpos[4]), 2) + pow(sin(cpos[4]), 2))); //Z

}

double Extruder::CalculateRealDistancePoints(const std::vector<double>& lpos, const std::vector<double>& cpos) {
	std::vector<double> lrpos, crpos;

	CalculateRealPoint(lpos, lrpos);
	CalculateRealPoint(cpos, crpos);

	return sqrt(pow(crpos[0] - lrpos[0], 2) + pow(crpos[1] - lrpos[1], 2) + pow(crpos[2] - lrpos[2], 2));
}

void Extruder::SetFilamentType(int type_id)
{
	switch (type_id) {
	case 6:
		drawobj_mode = (int)sim_drawing_cubepoints;
		break;
	case 7:
		drawobj_mode = (int)sim_drawing_spherepoints;
		break;
	default:
		drawobj_mode = (int)sim_drawing_cubepoints;
		break;
	}
}

int Extruder::GetFilamentType()
{
	return drawobj_mode;
}


void Extruder::SetFilamentColor(int color_id)
{
	std::lock_guard<std::mutex> lk(mutex_filament);

	if (color_id == 0 || color_id == 1 || color_id == 2)
	{
		std::vector<float> selected_color;

		//Find selected color
		auto search = color_map.find(color_id);
		
		if (search != color_map.end()) {
			selected_color = search->second;

			std::copy(selected_color.begin(), selected_color.end(), drawobj_color);
		}
	}
}

float * Extruder::GetFilamentColor()
{
	std::lock_guard<std::mutex> lk(mutex_filament);
	return drawobj_color;
}

void Extruder::SetFilamentSize(float size)
{
	std::lock_guard<std::mutex> lk(mutex_filament);
	drawobj_size = size;
}

float Extruder::GetFilamentSize()
{
	std::lock_guard<std::mutex> lk(mutex_filament);
	return drawobj_size;
}

void Extruder::SetFilamentResolution(float res)
{
	// Set minimum resolution = 0.5
	filament_resolution = (res < 0.5f ? 0.5f : res);
	// Set maximum resolution = 3.0
	filament_resolution = (res > 3.0f ? 3.0f : res);
}

float Extruder::GetFilamentResolution()
{
	return filament_resolution;
}

void Extruder::PaintInfo(const int type, std::vector<float>& color, float & size)
{
	color.resize(3);
	if (type == 1000) { //Rapid Move
		color[0] = alternative_color1[0]; //yellow
		color[1] = alternative_color1[1];
		color[2] = alternative_color1[2];

		size = alternative_size1; //4mm
	}
	else {
		std::lock_guard<std::mutex> lk(mutex_filament);
		color[0] = drawobj_color[0];
		color[1] = drawobj_color[1];
		color[2] = drawobj_color[2];
		size = drawobj_size;
	}
}