#include "string_concurrent_queue.h"

StringConcurrentQueue::StringConcurrentQueue()
{
}

int StringConcurrentQueue::size() {
    std::unique_lock<std::mutex> lk(m_mutex);
	size_t sz = m_deque.size();
	lk.unlock();
	return sz;
}

bool StringConcurrentQueue::empty() {
	std::unique_lock<std::mutex> lk(m_mutex);
	bool empty = m_deque.empty();
	lk.unlock();
    return empty;
}

void StringConcurrentQueue::clear()
{
	std::unique_lock<std::mutex> lk(m_mutex);
    m_deque.clear();
	lk.unlock();
}

void StringConcurrentQueue::push_front(const std::string &t)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.push_front(t);
	lk.unlock();
	m_cond.notify_one();
}

void StringConcurrentQueue::push_back(const std::string &t)
{
	std::unique_lock<std::mutex> lk(m_mutex);
    m_deque.push_back(t);
	lk.unlock();
	m_cond.notify_one();
}

std::string& StringConcurrentQueue::back()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque.back();
}

void StringConcurrentQueue::pop_back()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	m_deque.pop_back();
}

std::string StringConcurrentQueue::consume_back()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait_for(lk, std::chrono::milliseconds(100));
	}
	std::string msg = m_deque.back();
	m_deque.pop_back();
	return msg;
}


