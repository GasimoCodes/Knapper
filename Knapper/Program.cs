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
            uint maxCapacity = 10;
            Stopwatch stopwatch = new Stopwatch();

            // Weight, value
            Vector2UInt[] items = {
                new Vector2UInt(1, 10),
                new Vector2UInt(2, 4),
                new Vector2UInt(3, 15)/*,
                new Vector2UInt(3, 15),
                new Vector2UInt(1, 10),
                new Vector2UInt(2, 4),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(1, 10),
                new Vector2UInt(2, 4),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(1, 10),
                new Vector2UInt(2, 4),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(1, 10),
                new Vector2UInt(2, 4),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(2, 4),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(2, 4),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15),
                new Vector2UInt(3, 15)*/
            };
            Console.WriteLine("Total items: " + items.Length);



            // GPU
            KnapSackGPU.KnapsackSolver(items, maxCapacity);


            // Approx.
            KnapSackHeuristics.KnapsackSolver(items, maxCapacity);


            // CPU
            // KnapSackCPU.KnapsackSolver(items, maxCapacity);

            stopwatch.Restart();
            KnapSackCPU.KnapsackSolverThreaded(items, maxCapacity);
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
            






            /*
            stopwatch.Restart();
            
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
            */

        }





        public static void printCombinations(Vector3UInt[] list)
        {
            string toWrite = "";
            
				foreach (Vector3UInt innerList in list)
				{
						toWrite += (innerList.weight+ "," + innerList.value);
					toWrite += "\n";
				}
				
            toWrite += ("---\n TOTAL OF: " + list.Length + "\n---");

            Console.WriteLine(toWrite);

        }

    }
}