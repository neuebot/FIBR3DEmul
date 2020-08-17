#include "Printer.h"

#include "simConst.h"

using namespace std;

Printer::Printer(const std::string &rname, const int pdofs) :
	name(rname),
	dofs(pdofs),
	collisions_enabled(true),
	ready(false)
{
	//Reserve memory for joint target positions and extrude to avoid constant reallocations
	    
}

std::vector<double> Printer::get_cur_pos()
{
	std::lock_guard<std::mutex> lk(mutex_cpos);
	return cur_pos;
}

void Printer::set_cur_pos(const std::vector<double> &pos)
{
	std::lock_guard<std::mutex> lk(mutex_cpos);
	cur_pos = pos;
}
