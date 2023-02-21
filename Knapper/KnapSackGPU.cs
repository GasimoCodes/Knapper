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


        public static void KnapsackSolver(Vector2[] list, int capacity)
        {
            using Context context = Context.Create(builder => builder.AllAccelerators());
            Device d = context.GetPreferredDevice(preferCPU: false);
            Accelerator acc = d.CreateAccelerator(context);

            int n = list.Length;
            ulong combinationCount = (1UL << n) - 1;
            
            // Allocate and copy the init list
            MemoryBuffer1D<Vector2, Stride1D.Dense> inputs = acc.Allocate1D(list);
            // Allocate Vector for each thread
            MemoryBuffer1D<Vector3, Stride1D.Dense> results = acc.Allocate1D<Vector3>(acc.MaxNumThreads);
            MemoryBuffer1D<Vector3, Stride1D.Dense> staticMemory = acc.Allocate1D<Vector3>(0);


            Console.WriteLine($"DEV: {acc.Name}");
            Console.WriteLine($"\nTOTAL COMBINATIONS: {combinationCount}");
            Console.WriteLine("AVAILABLE THREADS: " + acc.MaxNumThreads);

            int iterCount = (int)((combinationCount + (ulong)acc.MaxNumThreads - 1UL) / (ulong)acc.MaxNumThreads);
            int chunkSize = acc.MaxNumThreads;
            Console.WriteLine("App will run for " + iterCount + " iteration(s)\n\n");
            
            
            // Upload kernel
            Action<Index1D, ArrayView<Vector2>, ArrayView<Vector3>, int, ArrayView<Vector3>> loadedKernel =
    acc.LoadAutoGroupedStreamKernel<Index1D, ArrayView<Vector2>, ArrayView<Vector3>, int, ArrayView<Vector3>>(Kernel);


            Vector2 bestResult = new Vector2();
            bool[] currentBestBits = new bool[n];

            Stopwatch sw = Stopwatch.StartNew();


            // Repeat this until we went over ALL the elements
            for (int t = 0; t < iterCount; t++)
            {
                // What will we cover in this iteration
                int start = t * chunkSize;
                int end = (int)Math.Min(((ulong)t + 1UL) * (ulong)chunkSize, combinationCount);

                // Console.WriteLine("START: " + start + "END " + end);

                loadedKernel(end-start, inputs.View, results.View, start, staticMemory.View);
                acc.Synchronize();


                // Get best value, somehow


                // Store best value


            }

            sw.Stop();


            /*
            foreach (Vector2 output in results.GetAsArray1D())
            {
                Console.WriteLine($"X: {output.X}, Y: {output.Y}");
            }*/

            Console.WriteLine(sw.ElapsedMilliseconds + "ms");

            acc.Dispose();
            context.Dispose();


        }


        static void Kernel(Index1D index, ArrayView<Vector2> data, ArrayView<Vector3> output, int indexOffset, ArrayView<Vector3> bestValue)
        {

            float value = 0;
            float weight = 0;
            int combinationIndex = (index+1)+ indexOffset;


            for (int elementIndex = 0; elementIndex < data.Length; elementIndex++)
            {
                if (((combinationIndex) & (1 << elementIndex)) != 0)
                {
                    value += data[elementIndex].X;
                    weight += data[elementIndex].Y;
                    // currentBits[elementIndex] = true;
                }
                else
                {
                    // currentBits[elementIndex] = false;
                }
            }

            /*
            // If the current combination is better than the last best
            if (weight <= capacity && value > bestResult.Y)
            {
                bestResult = new Vector2(weight, value);
                currentBestBits = currentBits;
            }
            */

            output[index] = new Vector3(weight, value, (combinationIndex-1));
        }


        static void KernelBestValue(Index1D index, ArrayView<Vector2> data, ArrayView<Vector3> bestValue)
        {

            
            /*
            // If the current combination is better than the last best
            if (weight <= capacity && value > bestResult.Y)
            {
                bestResult = new Vector2(weight, value);
                currentBestBits = currentBits;
            }
            */

        }




    }
}
