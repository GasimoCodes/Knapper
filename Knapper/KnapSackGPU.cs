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
            long memoryBlocks = (acc.MemorySize / (acc.MaxNumThreads * sizeof(uint)*6));
            Console.WriteLine($"Layout: " + memoryBlocks + " iterations can be stored (" + (memoryBlocks * chunkSize * sizeof(uint) * 3 / 1000000) + "MB)");

            // Allocate all the vector groups we can (with a reserve)
            MemoryBuffer1D<Vector3UInt, Stride1D.Dense> results = acc.Allocate1D<Vector3UInt>(memoryBlocks * chunkSize);

            // Allocate array for sending compiled data back to PC
            MemoryBuffer1D<Vector3UInt, Stride1D.Dense> transferResults = acc.Allocate1D<Vector3UInt>(64);

            // Upload kernel
            Action<Index1D, ArrayView<Vector2UInt>, ArrayView<Vector3UInt>, ulong> loadedKernel =
            acc.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector2UInt>, ArrayView<Vector3UInt>, ulong>(Kernel);


            Stopwatch sw = Stopwatch.StartNew();


            int collapseEvery = Math.Min(iterCount, 4);
            int collapseEveryCounter = 0;



            // Repeat this until we went over ALL the elements
            for (int t = 0; t < iterCount; t++)
            {
                collapseEveryCounter++;

                // Start from 0 or the last group index
                ulong groupStart = (ulong)(t * chunkSize);
                ulong groupEnd;
                
                if(((ulong)(t + 1) * (ulong)chunkSize) < combinationCount-1)
                {
                    // End on last combination of current group
                    groupEnd = (ulong)((t + 1) * chunkSize);

                } else
                {
                    // Otherwise, end is last combination.
                    groupEnd = combinationCount;
                }

                // Console.WriteLine("START: " + groupStart + "\tEND " + groupEnd + " DELTA: " + (groupEnd  - groupStart));



                loadedKernel((int)(groupEnd-groupStart), inputs.View, results.View, groupStart);
                acc.Synchronize();

            }



            sw.Stop();


            /*
            foreach (Vector2 output in results.GetAsArray1D())
            {
                Console.WriteLine($"X: {output.X}, Y: {output.Y}");
            }
            */

            Console.WriteLine(sw.ElapsedMilliseconds + "ms\n\n");

            acc.Dispose();
            context.Dispose();
        }

















        static void Kernel(Index1D index, ArrayView<Vector2UInt> data, ArrayView<Vector3UInt> output, ulong indexOffset)
        {

            uint value = 0;
            uint weight = 0;

            //index of the combination
            ulong combinationIndex = (ulong)index + indexOffset;

            for (int elementIndex = 0; elementIndex < data.Length; elementIndex++)
            {
                if (((combinationIndex) & (ulong)(1 << elementIndex)) != 0)
                {
                    value += data[elementIndex].weight;
                    weight += data[elementIndex].value;
                }
            }

            output[index] = new Vector3UInt(weight, value, combinationIndex-1);
        }


        static void KernelBestValue(Index1D index, ArrayView<Vector3UInt> data, int amount, int maxCarry)
        {
            // For each element of chunk
            for (int i = index; i <= amount + index; i++)
            {
                // Compare to first, if better
                if (data[i].weight <= maxCarry && data[i].value > data[index].value)
                {
                    // Replace first
                    data[index] = data[i];
                }
            }
        }




    }
}
