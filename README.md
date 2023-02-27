# C# 0-1 Knapsack solver
This repository was made as a school assignment and contains sample algorithms for solving the [0-1 (Bound) Knapsack problem](https://en.wikipedia.org/wiki/Knapsack_problem) using CPU **Singlethreading**, **Multithreading**, **Greedy Approximation** and **GPU Acceleration** (on any compute-capable GPU using [ILGPU.NET](https://ilgpu.net/))
 


## Methods overview

|                |Solution                          |State|
|----------------|-------------------------------|-----------------------------|
|Single Threaded|**Optimal** (Bruteforce)           | :heavy_check_mark: Stable up to $32$ Items|
|Multithreaded          |**Optimal** (Meet in the middle bruteforce on N threads)            |            :heavy_check_mark: Stable up to $64$ Items|
|Approximation          |**Approximation** (Greedy Approximation)|:heavy_check_mark: Tested up to $64$ Items|
|GPU|Throws Invalid solutions|❌ Unstable|



## Compiling
Requires **Visual Studio 2022** (or higher) and/or a **.NET 6.0 SDK** toolchain. ILGPU Libraries will be compiled automatically on first build.
This code is not intended for production and is provided as is.


## Usage

### Example
```csharp
KnapSackCPU.KnapsackSolverThreaded(items, maxCapacity);
```
Items - `Vector2UInt[](uint weight, uint value)` array of items to put in Knapsack

maxCapacity - Knapsack capacity, uint limit.

Returns the solution in `Vector3UInt (weight, value, combinationIndex)` format.

### Example approximation
```csharp
KnapSackApprox.KnapsackSolver(items, maxCapacity);
```
Items - `Vector2UInt[](uint weight, uint value)` array of items to put in Knapsack

maxCapacity - Knapsack capacity, uint limit.

Returns  `Vector2UInt[](uint weight, uint value)` of items in the Knapsack.



## Knapper GPU (WIP) Explained
Due to the nature of GPUs, the code had to be strongly edited.  The rough algorithm is as follows:

 - Based on GPU memory, **approximate how many combinations we can store at once**
 - Deploy GPU kernel which will (1 combination / thread) **calculate the weight and value of all combinations** which were stored in memory 
 - If the amount of combinations allows, **each GPU thread goes over a group of combinations** and **stores the best combination at its group index**. This is repeated until the amount of combinations no longer allows the distribution.
 - We use a fixed amount of threads (64) to **reduce the remaining combinations down to 64** and save them to a separate array.
 - The remaining **64 combination array is sent from GPU to CPU to select the best combination**
 - *(If the first step didnt fit all the combinations in memory, repeat for the rest of combinations)*
 - **Solution is returned**   
