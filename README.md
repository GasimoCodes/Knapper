# C# 0-1 Knapsack solver
This repository was made as a school assignment and contains sample algorithms for solving the [0-1 (Bound) Knapsack problem](https://en.wikipedia.org/wiki/Knapsack_problem) using CPU **Singlethreading**, **Multithreading**, **Greedy Approximation** and **GPU Acceleration** (on any compute-capable GPU using [ILGPU.NET](https://ilgpu.net/))
 

## Methods overview

|                |Solution                          |State|
|----------------|-------------------------------|-----------------------------|
|Single Threaded|**Optimal** (Bruteforce)           | :heavy_check_mark: Stable up to $32$ Items|
|Multithreaded          |**Optimal** (Meet in the middle bruteforce on N threads)            |            :heavy_check_mark: Stable up to $64$ Items|
|Approximation          |**Approximation** (Greedy Approximation)|:heavy_check_mark: Stable up to $64$ Items|
|GPU|❌ Throws Invalid solution (WIP)|Unstable|



## Compiling
Requires **Visual Studio 2022** (or higher) and/or a **.NET 6.0 SDK** toolchain. ILGPU Libraries will be compiled automatically on first build.

This code is not intended for production and is provided as is.

## Knapper GPU (WIP) Explained
Due to the nature of GPUs, the code had to be strongly edited.  The rough algorithm is as follows:

 - Based on GPU memory, approximate how many combinations we can store at once
 - Deploy GPU kernel which will (1 combination / thread) calculate the weight and value of all combinations which were stored in memory 
 - If the amount of combinations allows, each GPU thread goes over a group of combinations and stores the best combination at the group index. This is repeated.
 - If the amount of combinations no longer meets the previous condition, we use a fixed amount of threads (64) to reduce the remaining combinations down to 64
 - The remaining 64 combination array is sent from GPU to CPU to select the best combination
 - Solution is returned   
