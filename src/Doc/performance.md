# Performance

## .Net Core

The benchmark program was run with the environment:

- InSync 1.0.0
- Targeting .NET Core 2.1
- .NET SDK 5.0.101
- Windows 10
- Ryzen 3700X
    - fix clock at 4GHz
    - SMT was disabled 
- StopWatch resolution 100ns
- High process priority

### Single Lock

With no contentions, execution time of different methods of acquiring 1 lock was measured. Because of insufficient resolution of `StopWatch`, each method was repeated 200,000 times. The averages of time in nanoseconds were calculated. 100 times of such measurement were ran to get 100 averages for each method. Core 4 was used.

Method|Mean|STD
---|---|---
Loop overhead|0.251335|0.003273752
lock|10.44946|0.037760254
Synchronized.WithLock|11.077325|0.125649291
Synchronized.Lock|24.920565|0.589758128
SemaphoreSlim.Wait|26.13611|0.300629437
AsyncSynchronized.WithLock|37.637085|0.366919775
AsyncSynchronized.Lock|51.63986|0.724153676
SemaphoreSlim.WaitAsync|37.379345|0.691119172
AsyncSynchronized.WithLockAsync|95.74239|0.231985083
AsyncSynchronized.LockAsync|110.948145|0.605324165

### Multiple Locks

Similarly, that of acquiring 2 locks was measured.

Method|Mean|STD
---|---|---
Loop overhead|0.501575|0.001162058
lock|20.69993|0.188203361
Synchronized.Lock|42.319905|0.945111729
MultiSync.All Monitor|102.502125|1.99298697
MultiSync.All Monitor reusing array|98.72238|1.695725165
SemaphoreSlim.WaitAsync|73.396305|0.943295799
MultiSync.AllAsync SemaphoreSlim|321.813115|1.390673596

### Dining Philosophers

The original C++ code was rewritten in C# but with 10s eating time. For the 4-core case, 11 times was run using core 4, 5, 6 and 7. For the 8 core case, 10 times was run using all the cores.

![dining_core_4](https://user-images.githubusercontent.com/2862593/107522942-2eb67f80-6bef-11eb-8800-cb235ad6dcec.png)

![dining_core_8](https://user-images.githubusercontent.com/2862593/107522935-2d855280-6bef-11eb-8145-414b9c57b393.png)

Similary, an async version was run 13 and 13 times for the 4-core case and the 8-core case.

![dining_fw_4](https://user-images.githubusercontent.com/2862593/107522957-31b17000-6bef-11eb-84a7-ba14aec67005.png)

![dining_fw_8](https://user-images.githubusercontent.com/2862593/107522952-3118d980-6bef-11eb-8c94-22ca74dba6d1.png)

## .NET Framework

The benchmark program was run with the environment:

- InSync 1.0.0
- Targeting .NET Framework 4.6.1
- Windows 10
- Ryzen 3700X
    - fix clock at 4GHz
    - SMT was disabled 
- StopWatch resolution 100ns
- High process priority

### Single Lock

With no contentions, execution time of different methods of acquiring 1 lock was measured. Because of insufficient resolution of `StopWatch`, each method was repeated 200,000 times. The averages of time in nanoseconds were calculated. 100 times of such measurement were ran to get 100 averages for each method. Core 4 was used.

Method|Mean|STD
---|---|---
Loop overhead|0.36223|0.001206924
lock|12.41344|0.014205078
Synchronized.WithLock|15.11072|0.070974434
Synchronized.Lock|24.416935|0.668656778
SemaphoreSlim.Wait|115.474135|1.21495724
AsyncSynchronized.WithLock|123.731365|1.971970009
AsyncSynchronized.Lock|144.250505|1.632774856
SemaphoreSlim.WaitAsync|107.495325|1.348014322
AsyncSynchronized.WithLockAsync|295.73078|1.973408472
AsyncSynchronized.LockAsync|317.64382|1.759929962

### Multiple Locks

Similarly, that of acquiring 2 locks was measured.

Method|Mean|STD
---|---|---
Loop overhead|0.57007|0.012049984
lock|24.06699|0.131246722
Synchronized.Lock|43.842105|0.545906966
MultiSync.All Monitor reusing array|116.076655|1.714836014
MultiSync.All Monitor|127.937495|1.485412852
SemaphoreSlim.WaitAsync|214.37862|2.697429956
MultiSync.AllAsync SemaphoreSlim|715.955235|2.760225229

### Dining Philosophers

The original C++ code was rewritten in C# but with 10s eating time. For the 4-core case, 12 times was run using core 4, 5, 6 and 7. For the 8-core case, 13 times was run using all cores.

![async_dining_core_4](https://user-images.githubusercontent.com/2862593/107522949-30804300-6bef-11eb-8239-90d7bc2849d6.png)

![async_dining_core_8](https://user-images.githubusercontent.com/2862593/107522947-30804300-6bef-11eb-81bf-2647d86c200d.png)

Similary, an async version was run 13 and 13 times for the 4-core case and the 8-core case.

![async_dining_fw_4](https://user-images.githubusercontent.com/2862593/107522946-2fe7ac80-6bef-11eb-8caa-798329189595.png)

![async_dining_fw_8](https://user-images.githubusercontent.com/2862593/107522945-2f4f1600-6bef-11eb-89dd-435113077cb2.png)

## C++

For reference, a slightly modified version of the original C++ code was run. The eating time was 10s. Other modifications were for conveniently running continously. The environment was:

- Toolset Visual Studio 2019 v142
- Optimization O2
- Windows 10
- Ryzen 3700X
    - fix clock at 4GHz
    - SMT was disabled 
- StopWatch resolution 100ns
- High process priority

For the 4-core case, 10 times was run using core 4, 5, 6 and 7. For the 8 core case, 9 times was run using all cores.

![dining_cpp_4](https://user-images.githubusercontent.com/2862593/103096000-72542a80-463d-11eb-8261-eeff7f94fa9b.png)

![dining_cpp_8](https://user-images.githubusercontent.com/2862593/103096007-754f1b00-463d-11eb-832b-28993acc5949.png)
