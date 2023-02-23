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

        public static Vector2[] KnapsackSolver(Vector2[] list, int capacity)
        {
            // sort the items in decreasing order of value per unit weight
            Array.Sort(list, (a, b) => (b.Y / b.X).CompareTo(a.Y / a.X));

            // initialize the knapsack
            int n = list.Length;
            Vector2[] knapsack = new Vector2[n];

            // add items to the knapsack until it is full
            int i = 1;
            while (capacity > 0 && i < n)
            {
                int weight = (int)list[i].X;
                knapsack[i] = list[i];
                capacity -= weight;
                i++;
            }

            Console.WriteLine("Approximation: Weight: " + knapsack.Sum(x => x.X) + " Value: " + knapsack.Sum(x => x.Y));

            return knapsack;
        }


    }
}
