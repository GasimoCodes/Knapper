using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;
using System.Linq;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU;

namespace Knapper
{

    internal class Program
    {
        static void Main(string[] args)
        {
            int maxCapacity = 60;

            // Weight, value
            Vector2[] items = {
                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),
                              new Vector2(1, 10),
                new Vector2(2, 4),
                                new Vector2(1, 10),
                new Vector2(2, 4),

            };

            KnapSackGPU.KnapsackSolver(items, maxCapacity);
            

            Stopwatch stopwatch = Stopwatch.StartNew();
            Vector2[][] result;
            
            Console.WriteLine("Total items: " + items.Length);
            
            /*
            stopwatch.Restart();
            KnapsackSolverThreaded(items, maxCapacity);
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
            */
            
        }





        public static Vector2[] KnapsackSolver(Vector2[] list, int capacity)
        {
            list = list.OrderByDescending(x => x.Y).ToArray();

            int n = list.Length;
            int count = (1 << n) - 1;

            // Combination index, Item index
            bool[][] results = new bool[count][];
            // Combination index, weight, value
            Vector2[] resultValues = new Vector2[count];

            int bestResultIndex = 0;

            // For each combination
            for (int combinationIndex = 1; combinationIndex <= count; combinationIndex++)
            {

                // For each element in list
                for (int elementIndex = 0; elementIndex < n; elementIndex++)
                {
                    // Check whether to include the item in the combination using bit shift
                    if ((combinationIndex & (1 << elementIndex)) != 0)
                    {
                        //results[combinationIndex - 1][elementIndex-1] = true;
                        resultValues[combinationIndex - 1].X += list[elementIndex].X;
                        resultValues[combinationIndex - 1].Y += list[elementIndex].Y;
                    }
                }

                // If the current combination is better than the last best
                if (resultValues[combinationIndex - 1].X <= capacity && resultValues[combinationIndex - 1].Y > resultValues[bestResultIndex].Y)
                    bestResultIndex = (combinationIndex - 1);

            }

            Console.WriteLine("BEST WEIGHT: " + resultValues[bestResultIndex].X + " BEST VALUE: " + resultValues[bestResultIndex].Y);

            return null;
        }





        private static bool[] KnapsackSolverThreaded(Vector2[] list, int capacity)
        {

            list = list.OrderByDescending(x => x.Y).ToArray();

            int n = list.Length;
            int combinationCount = (1 << n) - 1;

            int numThreads = Environment.ProcessorCount;
            var tasks = new Task[numThreads];
            int chunkSize = (combinationCount + numThreads - 1) / numThreads;


            Vector2 bestResult = new Vector2();
            bool[] currentBestBits = new bool[n];

            for (int t = 0; t < numThreads; t++)
            {
                // Combination index, Item index

                int start = t * chunkSize + 1;
                int end = Math.Min((t + 1) * chunkSize, combinationCount);

                tasks[t] = Task.Factory.StartNew(() =>
                {
                    float value = 0;
                    float weight = 0;
                    bool[] currentBits = new bool[n];

                    for (int combinationIndex = start; combinationIndex <= end; combinationIndex++)
                    {
                        value = 0;
                        weight = 0;

                        for (int elementIndex = 0; elementIndex < n; elementIndex++)
                        {
                            if ((combinationIndex & (1 << elementIndex)) != 0)
                            {
                                value += list[elementIndex].X;
                                weight += list[elementIndex].Y;
                                currentBits[elementIndex] = true;
                            }
                            else
                            {
                                currentBits[elementIndex] = false;
                            }
                        }


                        // If the current combination is better than the last best
                        if (weight <= capacity && value > bestResult.Y)
                        {
                            bestResult = new Vector2(weight, value);
                            currentBestBits = currentBits;
                        }
                    }
                });
            }
            Task.WaitAll(tasks.Where(t => t != null).ToArray());

            Console.WriteLine("BEST WEIGHT: " + bestResult.X + " BEST VALUE: " + bestResult.Y);

            return currentBestBits;

        }

        public static void printCombinations(Vector2[][] list)
        {
            string toWrite = "";
            /*
			foreach (Vector2[] innerList in list)
			{
				foreach (Vector2 inner in innerList)
				{
					toWrite += (inner.X + ",");
				}
				toWrite += "\n";
			}
			*/
            toWrite += ("---\n TOTAL OF: " + list.Length + "\n---");

            Console.WriteLine(toWrite);

        }

    }
}