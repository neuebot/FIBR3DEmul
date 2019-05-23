#ifndef V_REPEXT_PAIR_CONCURRENT_QUEUE
#define V_REPEXT_PAIR_CONCURRENT_QUEUE

#include <deque>
#include <thread>
#include <mutex>
#include <vector>
#include <utility>
#include <condition_variable>

/*!
 * \brief The PairConcurrentQueue class
 * Consider template if required other vector types
 */
template <typename T1, typename T2>
class PairConcurrentQueue {
public:
	PairConcurrentQueue();

	bool empty();
	int size();
	void clear();
    void push_front(const std::pair<T1, T2> &t);
	void push_back(const std::pair<T1, T2> &t);
	void push_front(const T1 t1, const T2 t2);
	void push_back(const T1 t1, const T2 t2);
	std::pair<T1, T2> back();
	std::pair<T1, T2> at(size_t index);
    void pop_back();

private:
    std::deque< std::pair<T1, T2> > m_deque;
    std::mutex m_mutex;
	std::condition_variable m_cond;
};

template <typename T1, typename T2>
PairConcurrentQueue<T1, T2>::PairConcurrentQueue()
{}

template <typename T1, typename T2>
bool PairConcurrentQueue<T1, T2>::empty()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	bool empty = m_deque.empty();
	lk.unlock();
	return empty;
}

template <typename T1, typename T2>
int PairConcurrentQueue<T1, T2>::size()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	size_t sz = m_deque.size();
	lk.unlock();
	return sz;
}

template <typename T1, typename T2>
void PairConcurrentQueue<T1, T2>::clear()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.clear();
	lk.unlock();
}

template <typename T1, typename T2>
void PairConcurrentQueue<T1, T2>::push_front(const std::pair<T1, T2> &t)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.push_front(t);
	lk.unlock();
	m_cond.notify_one();
}

template <typename T1, typename T2>
void PairConcurrentQueue<T1, T2>::push_back(const std::pair<T1, T2> &t)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.push_back(t);
	lk.unlock();
	m_cond.notify_one();
}

template<typename T1, typename T2>
inline void PairConcurrentQueue<T1, T2>::push_front(const T1 t1, const T2 t2)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.emplace_front(t1, t2);
	lk.unlock();
	m_cond.notify_one();
}

template<typename T1, typename T2>
inline void PairConcurrentQueue<T1, T2>::push_back(const T1 t1, const T2 t2)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.emplace_back(t1, t2);
	lk.unlock();
	m_cond.notify_one();
}

template <typename T1, typename T2>
std::pair<T1, T2> PairConcurrentQueue<T1, T2>::back()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque.back();
}

template <typename T1, typename T2>
std::pair<T1, T2> PairConcurrentQueue<T1, T2>::at(size_t index)
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque.at(index);	
}

template <typename T1, typename T2>
void PairConcurrentQueue<T1, T2>::pop_back()
{
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	m_deque.pop_back();
}



#endif //V_REPEXT_PAIR_CONCURRENT_QUEUE
