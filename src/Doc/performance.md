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
Loop overhead|0.502205|0.002618017
lock|10.48168|0.042191334
Synchronized.WithLock|11.074585|0.154348616
Synchronized.Lock|23.35174|1.227891217
SemaphoreSlim.Wait|26.62024|0.105064133
AsyncSynchronized.WithLock|35.361215|0.94974477
AsyncSynchronized.Lock|58.553095|1.554136994
SemaphoreSlim.WaitAsync|25.621235|0.277216726
AsyncSynchronized.WithLockAsync|80.763075|1.842565078
AsyncSynchronized.LockAsync|103.82279|1.143010283

### Multiple Locks

Similarly, that of acquiring 2 locks was measured.

Method|Mean|STD
---|---|---
Loop overhead|0.50222|0.004764833
lock|20.710595|0.181092486
Synchronized.Lock|39.91558|0.510229626
MultiSync.All monitor|176.746805|3.011581144
MultiSync.All monitor reuse array|167.6974|1.265423182
SemaphoreSlim.WaitAsync|50.941635|0.240256196
MultiSync.AllAsync semaphoreslim|370.783275|3.404508767

### Dining Philosophers

The original C++ code was rewritten in C# but with 10s eating time. For the 4-core case, 11 times was run using core 4, 5, 6 and 7. For the 8 core case, 11 times was run using all cores.

![dining_core_4](https://user-images.githubusercontent.com/2862593/103096006-754f1b00-463d-11eb-8469-0abcb0f955a4.png)

![dining_core_8](https://user-images.githubusercontent.com/2862593/103096004-74b68480-463d-11eb-85d3-751f5deb166c.png)

Similary, an async version was run 14 and 11 times for the 4-core case and the 8-core case.

![async_dining_core_4](https://user-images.githubusercontent.com/2862593/103216353-d5510480-4950-11eb-990e-f5d6dc2547c1.png)

![async_dining_core_8](https://user-images.githubusercontent.com/2862593/103216363-d84bf500-4950-11eb-8ac2-0c73f5a142d4.png)

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
Loop overhead|0.36252|0.003144596
lock|12.412035|0.0109658
Synchronized.WithLock|15.138875|0.232656967
Synchronized.Lock|22.0225|0.6347454
SemaphoreSlim.Wait|114.748535|1.128779152
AsyncSynchronized.WithLock|124.169925|2.015787666
AsyncSynchronized.Lock|147.514825|1.482475072
SemaphoreSlim.WaitAsync|103.236805|1.896086705
AsyncSynchronized.WithLockAsync|279.55305|1.981495923
AsyncSynchronized.LockAsync|304.958515|1.775532174

### Multiple Locks

Similarly, that of acquiring 2 locks was measured.

Method|Mean|STD
---|---|---
Loop overhead|0.56915|0.003600295
lock|24.191225|0.181012533
Synchronized.Lock|46.275455|0.86436219
MultiSync.All monitor|179.386635|3.11450811
MultiSync.All monitor reuse array|166.543805|2.345737108
SemaphoreSlim.WaitAsync|193.14303|2.538284758
MultiSync.AllAsync semaphoreslim|752.56565|3.728029591

### Dining Philosophers

The original C++ code was rewritten in C# but with 10s eating time. For the 4-core case, 13 times was run using core 4, 5, 6 and 7. For the 8-core case, 13 times was run using all cores.

![dining_fw_4](https://user-images.githubusercontent.com/2862593/103096003-74b68480-463d-11eb-8d73-7742e6d33907.png)

![dining_fw_8](https://user-images.githubusercontent.com/2862593/103096001-73855780-463d-11eb-8230-e877f1aa16c2.png)

Similary, an async version was run 12 and 15 times for the 4-core case and the 8-core case.

![async_dining_fw_4](https://user-images.githubusercontent.com/2862593/103216360-d7b35e80-4950-11eb-9517-91cb7d925b19.png)

![async_dining_fw_8](https://user-images.githubusercontent.com/2862593/103216355-d6823180-4950-11eb-89b5-8596ec1c36fb.png)

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
