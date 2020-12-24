#include <chrono>
#include <mutex>
#include <random>
#include <array>
#include <vector>
#include <thread>
#include <iostream>

using Lock = std::unique_lock<std::mutex>;

typedef void (*LockFuncType)(Lock&, Lock&);

const int EatingTime = 10;

class SmartPolite
{
public:
    const static std::string name;
    void operator()(Lock& l0, Lock& l1)
    {
        while (true)
        {
            {
                std::unique_lock<Lock> u0(l0);
                if (l1.try_lock())
                {
                    u0.release();
                    break;
                }
            }
            std::this_thread::yield();
            {
                std::unique_lock<Lock> u1(l1);
                if (l0.try_lock())
                {
                    u1.release();
                    break;
                }
            }
            std::this_thread::yield();
        }
    }
};

const std::string SmartPolite::name = "SmartAndPolite";

class Smart
{
public:
    const static std::string name;
    void operator()(Lock& l0, Lock& l1)
    {
        while (true)
        {
            {
                std::unique_lock<Lock> u0(l0);
                if (l1.try_lock())
                {
                    u0.release();
                    break;
                }
            }
            {
                std::unique_lock<Lock> u1(l1);
                if (l0.try_lock())
                {
                    u1.release();
                    break;
                }
            }
        }
    }
};
const std::string Smart::name = "Smart";

class Persistent
{
public:
    const static std::string name;
    void operator()(Lock& l0, Lock& l1)
    {
        while (true)
        {
            std::unique_lock<Lock> u0(l0);
            if (l1.try_lock())
            {
                u0.release();
                break;
            }
        }
    }
};
const std::string Persistent::name = "Persistent";

class Ordered
{
public:
    const static std::string name;
    void operator()(Lock& l0, Lock& l1)
    {
        if (l0.mutex() < l1.mutex())
        {
            std::unique_lock<Lock> u0(l0);
            l1.lock();
            u0.release();
        }
        else
        {
            std::unique_lock<Lock> u1(l1);
            l0.lock();
            u1.release();
        }
    }
};
const std::string Ordered::name = "Ordered";

template <typename lock_func>
class Philosopher
{
    std::mt19937_64 eng_{ std::random_device{}() };

    lock_func lock;
    std::mutex& left_fork_;
    std::mutex& right_fork_;
    std::chrono::milliseconds eat_time_{ 0 };
    static constexpr std::chrono::seconds full_{ EatingTime };

public:
    Philosopher(lock_func lock, std::mutex& left, std::mutex& right);
    void dine();

private:
    void eat();
    bool flip_coin();
    std::chrono::milliseconds get_eat_duration();
};

template <typename lock_func>
constexpr std::chrono::seconds Philosopher<lock_func>::full_;

template <typename lock_func>
Philosopher<lock_func>::Philosopher(lock_func lock, std::mutex& left, std::mutex& right)
    : lock(lock)
    , left_fork_(left)
    , right_fork_(right)
{}

template <typename lock_func>
void
Philosopher<lock_func>::dine()
{
    while (eat_time_ < full_)
        eat();
}

template <typename lock_func>
void
Philosopher<lock_func>::eat()
{
    Lock first;
    Lock second;
    if (flip_coin())
    {
        first = Lock(left_fork_, std::defer_lock);
        second = Lock(right_fork_, std::defer_lock);
    }
    else
    {
        first = Lock(right_fork_, std::defer_lock);
        second = Lock(left_fork_, std::defer_lock);
    }
    auto d = get_eat_duration();
    lock(first, second);
    auto end = std::chrono::steady_clock::now() + d;
    while (std::chrono::steady_clock::now() < end)
        ;
    eat_time_ += d;
}

template <typename lock_func>
bool
Philosopher<lock_func>::flip_coin()
{
    std::bernoulli_distribution d;
    return d(eng_);
}

template <typename lock_func>
std::chrono::milliseconds
Philosopher<lock_func>::get_eat_duration()
{
    std::uniform_int_distribution<> ms(1, 10);
    return std::min(std::chrono::milliseconds(ms(eng_)), full_ - eat_time_);
}

template <class lock_functor>
void run()
{
    for (unsigned nt = 2; nt <= 32; ++nt)
    {
        std::vector<std::mutex> table(nt);
        std::vector<Philosopher<lock_functor>> diners;
        for (unsigned i = 0; i < table.size(); ++i)
        {
            int j = i;
            int k = j < table.size() - 1 ? j + 1 : 0;
            diners.push_back(Philosopher<lock_functor>(lock_functor(), table[j], table[k]));
        }
        std::vector<std::thread> threads(diners.size());
        unsigned i = 0;
        auto t0 = std::chrono::high_resolution_clock::now();
        for (auto& t : threads)
        {
            t = std::thread(&Philosopher<lock_functor>::dine, diners[i]);
            ++i;
        }
        for (auto& t : threads)
            t.join();
        auto t1 = std::chrono::high_resolution_clock::now();
        using secs = std::chrono::duration<float>;
        std::cout << lock_functor::name << "," << nt << "," << secs(t1 - t0).count() << std::endl;
        std::cout.flush();
    }
}

int
main()
{
    std::cerr << "Press enter to start...";
    std::cin.ignore();
    while (true)
    {
        run<Ordered>();
        run<SmartPolite>();
    }
}