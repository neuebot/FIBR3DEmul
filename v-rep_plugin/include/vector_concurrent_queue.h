#ifndef V_REPEXT_VECTOR_CONCURRENT_QUEUE
#define V_REPEXT_VECTOR_CONCURRENT_QUEUE

#include <deque>
#include <thread>
#include <mutex>
#include <vector>
#include <condition_variable>

/*!
 * \brief The VectorConcurrentQueue class
 * Consider template if required other vector types
 */
template <typename T>
class VectorConcurrentQueue {
public:
    VectorConcurrentQueue();

	bool empty();
	int size();
	void clear();
    void push_front(const std::vector<T> &t);
	void push_back(const std::vector<T> &t);
    std::vector<T> back();
	std::vector<T> at(size_t index);
    void pop_back();

private:
    std::deque< std::vector<T> > m_deque;
    std::mutex m_mutex;
	std::condition_variable m_cond;
};

template <typename T>
VectorConcurrentQueue<T>::VectorConcurrentQueue()
{}

template <typename T>
bool VectorConcurrentQueue<T>::empty()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	bool empty = m_deque.empty();
	lk.unlock();
	return empty;
}

template <typename T>
int VectorConcurrentQueue<T>::size()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	size_t sz = m_deque.size();
	lk.unlock();
	return sz;
}

template <typename T>
void VectorConcurrentQueue<T>::clear()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.clear();
	lk.unlock();
}

template <typename T>
void VectorConcurrentQueue<T>::push_front(const std::vector<T> &t)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.push_front(t);
	lk.unlock();
	m_cond.notify_one();
}

template <typename T>
void VectorConcurrentQueue<T>::push_back(const std::vector<T> &t)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.push_back(t);
	lk.unlock();
	m_cond.notify_one();
}

template <typename T>
std::vector<T> VectorConcurrentQueue<T>::back()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque.back();
}

template<typename T>
std::vector<T> VectorConcurrentQueue<T>::at(size_t index)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque.at(index);	
}

template <typename T>
void VectorConcurrentQueue<T>::pop_back()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	m_deque.pop_back();
}



#endif //V_REPEXT_VECTOR_CONCURRENT_QUEUE
