#pragma once

#include "string_concurrent_queue.h"

#include <mutex>
#include <string>

class Speaker
{
public:
	Speaker();

	// Serializes messages into JSON format - Called from simulator thread
	void Formulate(int line, bool collision);
	
	// Returns the number of messages stored in queue
	int NumberMessages();

	// Retrieves the current message and deletes it
	// Only 1 message stored and sent at a time
	std::string ReadMessage();

private:
	// Stores JSON messages - FIFO
	StringConcurrentQueue JSON_scq;
};