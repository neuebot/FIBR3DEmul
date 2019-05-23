#pragma once

#include "basic_concurrent_queue.h"
#include "vector_concurrent_queue.h"

#include <atomic>
#include <map>
#include <mutex>
#include <string>
#include <vector>

class Printer {
public:
	Printer(const std::string &pname, const int pdofs);

public:
	int handle;
	std::string name;
	int dofs;
	std::vector<int> joint_handles;
	std::atomic<bool> collisions_enabled;
	std::vector<int> collision_handles;
	bool ready;

public:
	//Current line
	BasicConcurrentQueue<int> line;
	VectorConcurrentQueue<double> target_jpos;

public:
	std::vector<double> get_cur_pos();
	void set_cur_pos(const std::vector<double> &pos);

private:
	std::vector<double> cur_pos;
	std::mutex mutex_cpos;

};