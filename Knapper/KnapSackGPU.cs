using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using System.Threading;
using System.Numerics;
using ILGPU.Runtime.OpenCL;
using ILGPU.IR;
using System.Diagnostics;
using ILGPU.Algorithms;
using ILGPU.Backends;

namespace Knapper
{
    public static class KnapSackGPU
    {


        public static void KnapsackSolver(Vector2UInt[] list, uint capacity, ulong start = 0, ulong end = 0)
        {
            using Context context = Context.Create(builder => builder.AllAccelerators());
            Device d = context.GetPreferredDevice(preferCPU: false);
            Accelerator acc = d.CreateAccelerator(context);

            int n = list.Length;
            ulong combinationCount = (1UL << n) - 1;

            if (end == 0)
            {
                end = combinationCount;
            }

            // How many items we need to go through
            ulong delta = end - start;

            // Allocate and copy the init list
            MemoryBuffer1D<Vector2UInt, Stride1D.Dense> inputs = acc.Allocate1D(list);

            // IterationCount = CombinationCount / MaxNumThreads
            int iterCount = (int)(((delta) + (ulong)acc.MaxNumThreads - 1UL) / (ulong)acc.MaxNumThreads);
            int chunkSize = acc.MaxNumThreads;

            Console.WriteLine($"\nDEVICE:\t{acc.Name}\n" +
                $"SPEED:     \t{acc.MaxNumThreads}/it. ({iterCount} it.)\n" +
                $"MEMORY:     \t{acc.MemorySize / 1000000} MB\n" +
                $"COMBINATIONS:\t{delta}/{combinationCount}\n");


            // Max amount of iterations we can store in memory at a given time


            long memoryBlocks;

            if ((ulong)acc.MaxNumThreads > delta)
            {
                memoryBlocks = (long)(delta * sizeof(uint) * 6);
            } 
            else
            {
                memoryBlocks = (acc.MemorySize / (acc.MaxNumThreads * sizeof(uint) * 6));
            }

            Console.WriteLine($"Layout: " + memoryBlocks + " iterations can be stored (" + (memoryBlocks * chunkSize * sizeof(uint) * 3 / 1000000) + "MB)");


            ulong iterCountMemStore = (delta + ((ulong)memoryBlocks * (ulong)chunkSize) - 1UL) / ((ulong)(memoryBlocks) * (ulong)chunkSize);


            // Allocate all the vector groups we can (with a reserve)
            MemoryBuffer1D<Vector3UInt, Stride1D.Dense> results = acc.Allocate1D<Vector3UInt>(memoryBlocks * chunkSize);

            // Allocate array for sending compiled data back to PC
            MemoryBuffer1D<Vector3UInt, Stride1D.Dense> transferResults = acc.Allocate1D<Vector3UInt>(64);

            Console.WriteLine($"(MassIterations: {iterCountMemStore})");


            int collapseEvery = Math.Min(iterCount, 4);
            int collapseEveryCounter = 0;


            Stopwatch sw = Stopwatch.StartNew();
            Vector3UInt bestValue = new Vector3UInt(0,0,0);

            // Execute all we can fit into a memory block
            for (ulong t = 0; t < iterCountMemStore; t++)
            {

                // Upload kernel
                Action<Index1D, ArrayView<Vector2UInt>, ArrayView<Vector3UInt>, ulong> loadedKernel =
                    acc.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector2UInt>, ArrayView<Vector3UInt>, ulong>(KernelGetWeightValue);


                // Start from 0 or the last group index
                ulong groupStart = (ulong)(t * ((ulong)chunkSize * (ulong)memoryBlocks));
                ulong groupEnd;


                if (((ulong)(t + 1) * (ulong)chunkSize * (ulong)memoryBlocks) < end - 1)
                {
                    // End on last combination of current group
                    groupEnd = (ulong)(t + 1) * (ulong)chunkSize * (ulong)memoryBlocks;
                }
                else
                {
                    // Otherwise, end is last combination.
                    groupEnd = end;
                }

                // Console.WriteLine("START: " + groupStart + "\tEND " + groupEnd + " DELTA: " + (groupEnd  - groupStart));

                loadedKernel((int)(groupEnd - groupStart), inputs.View, results.View, groupStart);
                acc.Synchronize();

                //Program.printCombinations(results.GetAsArray1D());

                // - - - - - - - - - - - - Collapse to amount of threads - - - - - - - - - - - - 

                // Upload kernel
                Action<Index1D, ArrayView<Vector3UInt>, int, uint> sortKernel =
                    acc.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector3UInt>, int, uint>(KernelFindBestValue);

                // Max Reduce factor in order to reduce the combinations down to 1 per thread
                int reduceFactor = (int)((groupEnd - groupStart) / (ulong)chunkSize);

                if (reduceFactor > 0)
                {
                    sortKernel(chunkSize, results.View, reduceFactor, capacity);
                    acc.Synchronize();
                }

                // - - - - - - - - - - - - Collapse to 64 - - - - - - - - - - - - 

                // Upload kernel
                Action<Index1D, ArrayView<Vector3UInt>, ArrayView<Vector3UInt>, int> uploadKernel =
                    acc.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector3UInt>, ArrayView<Vector3UInt>, int>(KernelCollapseUploadBestValue);

                int reduceFactor2 = chunkSize / 64;

                uploadKernel(64, results.View, transferResults.View, reduceFactor2);
                acc.Synchronize();

                Vector3UInt[] fromGPU = transferResults.GetAsArray1D();

                // - - - - - - - - - - - - Collapse to 1 - - - - - - - - - - - - 
                
                foreach (Vector3UInt fromGPUValue in fromGPU)
                {
                    if(fromGPUValue.value > bestValue.value)
                    {
                        bestValue = fromGPUValue;
                    }
                }

            }



            sw.Stop();

            Console.WriteLine("GPU: Weight: " + bestValue.weight + " Value: " + bestValue.value);
            Console.WriteLine(sw.ElapsedMilliseconds + "ms\n\n");


            acc.Dispose();
            context.Dispose();
        }





        static void KernelGetWeightValue(Index1D index, ArrayView<Vector2UInt> data, ArrayView<Vector3UInt> output, ulong indexOffset)
        {

            uint value = 0;
            uint weight = 0;

            //index of the combination
            ulong combinationIndex = (ulong)index + indexOffset;

            for (int elementIndex = 0; elementIndex < data.Length; elementIndex++)
            {
                if (((combinationIndex) & (ulong)(1 << elementIndex)) != 0)
                {
                    weight += data[elementIndex].weight;
                    value += data[elementIndex].value;
                }
            }

            output[index] = new Vector3UInt(weight, value, combinationIndex - 1);
        }



        static void KernelFindBestValue(Index1D index, ArrayView<Vector3UInt> data, int reduceFactor, uint maxCarry)
        {
            int indexBest = index;

            // For each element of chunk
            for (int i = index; i < reduceFactor; i++)
            {
                // Compare to first, if better
                if (data[i].weight <= maxCarry && data[i].value > data[indexBest].value)
                {
                    // Replace first
                    indexBest = i;
                }
            }

            // write data to current group index (aka each reduceFactorTh element)
            data[index] = data[indexBest];

        }


        static void KernelCollapseUploadBestValue(Index1D index, ArrayView<Vector3UInt> data, ArrayView<Vector3UInt> upload, int reduceFactor)
        {
            int indexBest = index;

            // For each element of chunk
            for (int i = index; i < reduceFactor; i++)
            {
                // Compare to first, if better
                if (data[i].value > data[indexBest].value)
                {
                    // Replace first
                    indexBest = i;
                }
            }

            // write data to current group index (aka each reduceFactorTh element)
            upload[index] = data[indexBest];

        }


    }
}
