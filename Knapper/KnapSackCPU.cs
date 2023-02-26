using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Knapper
{
    /// <summary>
    /// Knapsack solver methods utilizing CPU and Threading
    /// </summary>
    public class KnapSackCPU
    {


        /// <summary>
        /// Runs knapback solver singlethreaded
        /// </summary>
        /// <param name="list"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static Vector2UInt[] KnapsackSolver(Vector2UInt[] list, uint capacity)
        {
            list = list.OrderByDescending(x => x.value).ToArray();

            int n = list.Length;
            int count = (1 << n) - 1;

            // Combination index, Item index
            bool[][] results = new bool[count][];
            // Combination index, weight, value
            Vector2UInt[] resultValues = new Vector2UInt[count];

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
                        resultValues[combinationIndex - 1].weight += list[elementIndex].weight;
                        resultValues[combinationIndex - 1].value += list[elementIndex].value;
                    }
                }

                // If the current combination is better than the last best
                if (resultValues[combinationIndex - 1].weight <= capacity && resultValues[combinationIndex - 1].value > resultValues[bestResultIndex].value)
                    bestResultIndex = (combinationIndex - 1);

            }

            Console.WriteLine("Compute: Weight: " + resultValues[bestResultIndex].weight + " Value: " + resultValues[bestResultIndex].value);

            return null;
        }


        /// <summary>
        /// Runs knapsack solver threaded without locks
        /// </summary>
        /// <param name="list"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static bool[] KnapsackSolverThreaded(Vector2UInt[] list, uint capacity)
        {

            // list = list.OrderByDescending(x => x.Y).ToArray();

            int n = list.Length;
            ulong combinationCount = (ulong)(1UL << n) - 1UL;

            int numThreads = Environment.ProcessorCount;
            var tasks = new Task[numThreads];
            ulong chunkSize = (ulong)(combinationCount + (ulong)numThreads - 1) / (ulong)numThreads;

            Vector2UInt bestResult = new Vector2UInt();
            bool[] currentBestBits = new bool[n];

            for (int t = 0; t < numThreads; t++)
            {
                // Combination index, Item index

                ulong start = (ulong)t * chunkSize + 1;
                ulong end;

                if ((ulong)(t + 1) * chunkSize < combinationCount)
                {
                    end = (ulong)(t + 1) * chunkSize;
                }
                else
                {
                    end = combinationCount;
                }

                tasks[t] = Task.Factory.StartNew(() =>
                {
                    uint value = 0;
                    uint weight = 0;
                    bool[] currentBits = new bool[n];

                    for (ulong combinationIndex = start; combinationIndex <= end; combinationIndex++)
                    {
                        value = 0;
                        weight = 0;

                        for (int elementIndex = 0; elementIndex < n; elementIndex++)
                        {
                            if ((combinationIndex & (ulong)(1 << elementIndex)) != 0)
                            {
                                value += list[elementIndex].value;
                                weight += list[elementIndex].weight;
                                currentBits[elementIndex] = true;
                            }
                            else
                            {
                                currentBits[elementIndex] = false;
                            }
                        }

                        // If the current combination is better than the last best
                        if (weight <= capacity && value > bestResult.value)
                        {
                            bestResult = new Vector2UInt(weight, value);
                            currentBestBits = currentBits;
                        }
                    }
                });
            }


            Task.WaitAll(tasks.Where(t => t != null).ToArray());

            Console.WriteLine("Threaded: Weight: " + bestResult.weight + " Value: " + bestResult.value);

            return currentBestBits;

        }


        /// <summary>
        /// Runs knapsack solver threaded with concurrency checks
        /// </summary>
        /// <param name="list"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static bool[] KnapsackSolverThreadedLock(Vector2UInt[] list, uint capacity)
        {

            // list = list.OrderByDescending(x => x.Y).ToArray();

            int n = list.Length;
            ulong combinationCount = (ulong)(1UL << n) - 1UL;

            int numThreads = Environment.ProcessorCount;
            var tasks = new Task[numThreads];
            ulong chunkSize = (ulong)(combinationCount + (ulong)numThreads - 1) / (ulong)numThreads;

            Vector2UInt bestResult = new Vector2UInt();
            bool[] currentBestBits = new bool[n];
            object lockObj = new object(); // create lock object

            for (int t = 0; t < numThreads; t++)
            {
                // Combination index, Item index

                ulong start = (ulong)t * chunkSize + 1;
                ulong end;

                if ((ulong)(t + 1) * chunkSize < combinationCount)
                {
                    end = (ulong)(t + 1) * chunkSize;
                }
                else
                {
                    end = combinationCount;
                }

                tasks[t] = Task.Factory.StartNew(() =>
                {
                    uint value = 0;
                    uint weight = 0;
                    bool[] currentBits = new bool[n];

                    for (ulong combinationIndex = start; combinationIndex <= end; combinationIndex++)
                    {
                        value = 0;
                        weight = 0;

                        for (int elementIndex = 0; elementIndex < n; elementIndex++)
                        {
                            if ((combinationIndex & (ulong)(1 << elementIndex)) != 0)
                            {
                                value += list[elementIndex].value;
                                weight += list[elementIndex].weight;
                                currentBits[elementIndex] = true;
                            }
                            else
                            {
                                currentBits[elementIndex] = false;
                            }
                        }

                        // If the current combination is better than the last best
                        if (weight <= capacity && value > bestResult.value)
                        {
                            lock (lockObj) // acquire lock
                            {
                                if (value > bestResult.value) // check again inside the lock
                                {
                                    bestResult = new Vector2UInt(weight, value);
                                    currentBestBits = currentBits;
                                }
                            } // release lock
                        }
                    }
                });
            }


            Task.WaitAll(tasks.Where(t => t != null).ToArray());

            Console.WriteLine("Threaded: Weight: " + bestResult.weight + " Value: " + bestResult.value);

            return currentBestBits;

        }




    }
}
