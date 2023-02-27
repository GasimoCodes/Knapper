# C# 0-1 Knapsack solver
This repository contains sample algorythmns for solving the [0-1 (Bound) Knapsack problem](https://en.wikipedia.org/wiki/Knapsack_problem) using CPU **Singlethreading**, **Multithreading**, **Greedy Approximation** and **GPU Acceleration** (on any compute-capable GPU using [ILGPU.NET](https://ilgpu.net/))
 



## Methods overview


|                |Solution                          |State|
|----------------|-------------------------------|-----------------------------|
|Single Threaded|**Optimal** (Bruteforce)           | :heavy_check_mark: Stable up to $32$ Items|
|Multithreaded          |**Optimal** (Meet in the middle bruteforce on N threads)            |            :heavy_check_mark: Stable up to $64$ Items|
|Approximation          |**Approximation** (Greedy Approximation)|:heavy_check_mark: Stable up to $64$ Items|
|GPU Approximation          |❌ Throws Invalid solution (WIP)|Unstable|



## Compiling
Requires **Visual Studio 2022** (or higher) and/or a **.NET 6.0 SDK** toolchain. ILGPU Libraries will be compiled automatically on first build.


## Methods



