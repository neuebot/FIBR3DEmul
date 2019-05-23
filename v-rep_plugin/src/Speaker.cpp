#include "Speaker.h"

#include <boost/serialization/serialization.hpp>
#include <boost/property_tree/ptree.hpp>
#include <boost/property_tree/json_parser.hpp>

#include <vector>

using boost::property_tree::ptree;
using boost::property_tree::write_json;

Speaker::Speaker()
{
}

void Speaker::Formulate(int line, bool collision)
{
	ptree reply;
	reply.put("line", line);
	reply.put("collision", collision);
	std::ostringstream os;
	write_json(os, reply);
	//os << std::endl;

	//Pushes message into queue
	JSON_scq.push_front(os.str());
}

int Speaker::NumberMessages() 
{
	return JSON_scq.size();
}

std::string Speaker::ReadMessage()
{
	std::string msg = "{}";
	if (!JSON_scq.empty()) {
		msg = JSON_scq.consume_back();
	}
	return msg;
}
