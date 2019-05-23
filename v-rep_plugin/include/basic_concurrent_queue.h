#ifndef V_REPEXT_BASIC_CONCURRENT_QUEUE
#define V_REPEXT_BASIC_CONCURRENT_QUEUE

#include <deque>
#include <thread>
#include <mutex>
#include <condition_variable>

template <typename T>
class BasicConcurrentQueue {
public:
    BasicConcurrentQueue();

    void push_front(const T t);
    void push_back(const T t);

	T& front();
    T& back();
	//Not retrived the reference, because the object is destroyed right after
    T only_back();

	T& operator[](std::size_t idx);
    
	void pop_back();
    void clear();

    bool empty();
    int size();

private:
    std::deque<T> m_deque;
    std::mutex m_mutex;
	std::condition_variable m_cond;
};

template<typename T>
BasicConcurrentQueue<T>::BasicConcurrentQueue()
{}

template<typename T>
void BasicConcurrentQueue<T>::push_front(const T t) {
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.push_front(t);
	lk.unlock();
	m_cond.notify_one();
}

template<typename T>
void BasicConcurrentQueue<T>::push_back(const T t) {
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.push_back(t);
	lk.unlock();
	m_cond.notify_one();
}

template<typename T>
T& BasicConcurrentQueue<T>::front() {
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque.front();
}

template<typename T>
T& BasicConcurrentQueue<T>::back() {
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque.back();
}

template<typename T>
T BasicConcurrentQueue<T>::only_back() {
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	T val = m_deque.back();
    m_deque.clear();
    return val;
}

///DANGEROUS
template<typename T>
T& BasicConcurrentQueue<T>::operator[](std::size_t idx) {
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	return m_deque[idx];
}

template<typename T>
void BasicConcurrentQueue<T>::pop_back() {
	std::unique_lock<std::mutex> lk(m_mutex);
	while (m_deque.empty()) {
		m_cond.wait(lk);
	}
	m_deque.pop_back();
}

template<typename T>
void BasicConcurrentQueue<T>::clear() {
	std::unique_lock<std::mutex> lk(m_mutex);
	m_deque.clear();
	lk.unlock();
}

template<typename T>
bool BasicConcurrentQueue<T>::empty() {
	std::unique_lock<std::mutex> lk(m_mutex);
	bool empty = m_deque.empty();
	lk.unlock();
	return empty;
}

template<typename T>
int BasicConcurrentQueue<T>::size() {
	std::unique_lock<std::mutex> lk(m_mutex);
	size_t sz = m_deque.size();
	lk.unlock();
	return sz;
}

#endif //V_REPEXT_BASIC_CONCURRENT_QUEUE
