using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Knapper
{
    /// <summary>
    /// Approximates the knapsack solution
    /// </summary>
	internal class KnapSackApprox
	{

        /// <summary>
        /// Solves the knapsack problem using Greedy Search
        /// </summary>
        /// <param name="list"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static Vector2UInt[] KnapsackSolver(Vector2UInt[] list, uint capacity)
        {
            // sort the items by value to weight ratio in decreasing order
            Array.Sort(list, (a, b) => b.value.CompareTo(a.value) != 0 ? b.value.CompareTo(a.value) : a.weight.CompareTo(b.weight));

            // initialize the knapsack
            int n = list.Length;
            Vector2UInt[] knapsack = new Vector2UInt[n];

            // add items to the knapsack until it is full
            uint totalWeight = 0;
            for (int i = 0; i < n; i++)
            {
                if (totalWeight + list[i].weight <= capacity)
                {
                    knapsack[i] = list[i];
                    totalWeight += list[i].weight;
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("Approximation: Weight: " + knapsack.Sum(x => x.weight) + " Value: " + knapsack.Sum(x => x.value));

            return knapsack;
        }



    }
}
