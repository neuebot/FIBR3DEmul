#ifndef V_REPEXT_STRING_CONCURRENT_QUEUE
#define V_REPEXT_STRING_CONCURRENT_QUEUE

#include <deque>
#include <thread>
#include <mutex>
#include <condition_variable>
#include <string>

class StringConcurrentQueue {
public:
    StringConcurrentQueue();

    int size();
    bool empty();
    void clear();
	void push_front(const std::string &t);
    void push_back(const std::string &t);
    
	std::string& back();
    void pop_back();

	std::string consume_back();

private:
    std::deque<std::string> m_deque;
    std::mutex m_mutex;
	std::condition_variable m_cond;

};

#endif //V_REPEXT_STRING_CONCURRENT_QUEUE
