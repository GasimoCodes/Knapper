using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Knapper
{
	internal class KnapSackHeuristics
	{

		public static Vector2UInt[] KnapsackSolver(Vector2UInt[] list, uint capacity)
		{

            Array.Sort(list, (a, b) => (b.value / b.weight).CompareTo(a.value / a.weight));

            // initialize the knapsack
            int n = list.Length;
			Vector2UInt[] knapsack = new Vector2UInt[n];

			// add items to the knapsack until it is full
			int i = 1;
			while (capacity > 0 && i < n)
			{
				uint weight = list[i].weight;

				if ((capacity - weight) >= 0)
				{
					knapsack[i] = list[i];
					capacity -= weight;

				} else
				{
					break;
				}
				i++;
			}

			Console.WriteLine("Approximation: Weight: " + knapsack.Sum(x => x.weight) + " Value: " + knapsack.Sum(x => x.value));

			return knapsack;
		}


	}
}
