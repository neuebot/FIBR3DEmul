#pragma once

#include "basic_concurrent_queue.h"
#include "vector_concurrent_queue.h"
#include "pair_concurrent_queue.h"

#include <map>
#include <mutex>
#include <string>
#include <vector>

class Extruder {
public:
	Extruder(const std::string &ename, const std::string &bname);

	// Set initial height minus object half-size
	void SetInitialTransformation(const std::vector<float> &transf);
	std::vector<float> GetInitialTransformation();
	void CalculateWorldPoint(const std::vector<double> &cpos, float wp[3]);
	void CalculateWorldPoint(float cpos[3], float wp[3]);

	void CalculateRealPoint(const std::vector<double> &cpos, std::vector<double> &bpos);
	double CalculateRealDistancePoints(const std::vector<double> &lpos, const std::vector<double> &cpos);

	void SetFilamentType(int type_id);
	int GetFilamentType();
	void SetFilamentColor(int color_id);
	float* GetFilamentColor();
	void SetFilamentSize(float size);
	float GetFilamentSize();
	void SetFilamentResolution(float res);
	float GetFilamentResolution();

public:
	std::string ext_name;
	int ext_handle;
	std::string bed_name;
	int bed_handle;
	std::string octree_name;
	int octree_handle;
	double octree_leaf_size;
	std::map<int, std::vector<float> > color_map;

	// G0 instructions filament
	std::vector<float> alternative_color1;
	float alternative_size1;

	std::vector<int> paint_handle;
	VectorConcurrentQueue<float> paint_items;
	VectorConcurrentQueue<float> paint_items2;

	//std::deque<int> object_list;
	//BasicConcurrentQueue<int> all_drawing;

	void PaintInfo(const int type, std::vector<float> &color, float &size);
	PairConcurrentQueue< bool, int > paint;

public:
	int drawobj_mode;
	float drawobj_color[3];
	float drawobj_size;
	size_t buffer_size;
	//last detected and transformed point (extruder to world)
	float last_printer_point[5];

private:
	float filament_resolution;
	
	std::vector<float> init_transf;
	std::mutex mutex_filament;
};