using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Knapper
{

	internal class Program
	{
		static void Main(string[] args)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			int maxCapacity = 60;

			// Weight, value
			Vector2[] items = {
				new Vector2(1, 10),
				new Vector2(2, 4),
				new Vector2(4, 12),
				new Vector2(5, 10),
				new Vector2(6, 4),
				new Vector2(7, 12),
				new Vector2(8, 10),
				new Vector2(9, 4),
				new Vector2(10, 12),
				new Vector2(11, 4),
				new Vector2(12, 12),
				new Vector2(13, 4),
				new Vector2(14, 12),
				new Vector2(15, 4),
				new Vector2(16, 12),
				new Vector2(17, 10),
								new Vector2(8, 10),
				new Vector2(9, 4),
				new Vector2(10, 12),
				new Vector2(11, 4),
				new Vector2(12, 12),
				new Vector2(13, 4),
				new Vector2(14, 12),
				new Vector2(15, 4),
				new Vector2(16, 12),
				new Vector2(17, 10),
			};

			Vector2[][] result;

			
			stopwatch.Restart();
			result = GetCombinationOptimized(items);
			
			printCombinations(result);
			Vector2[] value = getBestCapacity(GetCombinationOptimizedParallel(items), maxCapacity);
			Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");

			/*
			stopwatch.Restart();
			result = GetCombinationOptimizedParallel(items);
			Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
			printCombinations(result);
			*/

			stopwatch.Restart();
			KnapsackSolver(items, maxCapacity);
			Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
		}



		public static Vector2[][] GetCombinationOptimized(Vector2[] list)
		{
			int n = list.Length;
			int count = (1 << n) - 1;

			Vector2[][] results = new Vector2[count][];

			// For each combination
			for (int combinationIndex = 1; combinationIndex <= count; combinationIndex++)
			{
				List<Vector2> combination = new List<Vector2>();

				// For each element in list
				for (int elementIndex = 0; elementIndex < n; elementIndex++)
				{
					// Check whether to include the item in the combination using bit shift
					if ((combinationIndex & (1 << elementIndex)) != 0)
					{
						combination.Add(list[elementIndex]);
					}
				}

				results[combinationIndex - 1] = combination.ToArray();

			}

			return results;
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
						// results[combinationIndex - 1][elementIndex-1] = true;
						resultValues[combinationIndex - 1].X += list[elementIndex].X;
						resultValues[combinationIndex - 1].Y += list[elementIndex].Y;
					}
				}

				// If the current combination is better than the last best
				if(resultValues[combinationIndex-1].X <= capacity && resultValues[combinationIndex-1].Y > resultValues[bestResultIndex].Y)
					bestResultIndex = (combinationIndex-1);

			}

			Console.WriteLine("BEST WEIGHT: " + resultValues[bestResultIndex].X + " BEST VALUE: " + resultValues[bestResultIndex].Y);

			return null;
		}




		public static Vector2[][] GetCombinationOptimizedParallel(Vector2[] list)
		{
			int n = list.Length;
			int count = (1 << n) - 1;
			int maxDegreeOfParallelism = Environment.ProcessorCount;

			Vector2[][] results = new Vector2[count][];

			int chunkSize = count / maxDegreeOfParallelism;

			List<Task> tasks = new List<Task>();
			for (int i = 0; i < maxDegreeOfParallelism; i++)
			{
				int startIndex = i * chunkSize + 1;
				int endIndex = (i == maxDegreeOfParallelism - 1) ? count : (i + 1) * chunkSize;

				tasks.Add(Task.Run(() =>
				{
					for (int combinationIndex = startIndex; combinationIndex <= endIndex; combinationIndex++)
					{
						List<Vector2> combination = new List<Vector2>();

						for (int elementIndex = 0; elementIndex < n; elementIndex++)
						{
							if ((combinationIndex & (1 << elementIndex)) != 0)
							{
								combination.Add(list[elementIndex]);
							}
						}

						results[combinationIndex - 1] = combination.ToArray();
					}
				}));
			}

			Task.WaitAll(tasks.ToArray());

			return results;
		}



		private static Vector2[][] GetCombinationOptimizedThreaded(Vector2[] list)
		{

			int n = list.Length;
			int count = (1 << n) - 1;
			int numThreads = Environment.ProcessorCount;
			var tasks = new Task[numThreads];
			int chunkSize = (count + numThreads - 1) / numThreads;

			Vector2[][] results = new Vector2[count][];


			for (int t = 0; t < numThreads; t++)
			{

				int start = t * chunkSize + 1;
				int end = Math.Min((t + 1) * chunkSize, count);

				tasks[t] = Task.Factory.StartNew(() =>
				{
					for (int i = start; i <= end; i++)
					{

						List<Vector2> combination = new List<Vector2>();

						for (int j = 0; j < n; j++)
						{
							if ((i & (1 << j)) != 0)
							{
								// Console.Write(list[j].X);

								combination.Add(list[j]);
							}
						}

						results[i - 1] = combination.ToArray();


					}
				});

			}
			Task.WaitAll(tasks.Where(t => t != null).ToArray());

			return results;

		}


		public static Vector2[] getBestCapacity(Vector2[][] input, int maxCapacity)
		{
			// Index same as input index, (int1 = weight, int2 = value)
			Dictionary<KeyValuePair<int, int>, Vector2[]> values = new Dictionary<KeyValuePair<int, int>, Vector2[]>();

			foreach (Vector2[] val in input)
			{
				int weight = (int)val.Sum(x => x.X);
				int value = (int)val.Sum(x => x.Y);

				values.TryAdd(new KeyValuePair<int, int>(weight, value), val);
			}

			foreach (KeyValuePair<KeyValuePair<int, int>, Vector2[]> val in values.OrderByDescending(x => x.Key.Value))
			{
				if (val.Key.Key <= maxCapacity)
				{
					Console.WriteLine($"VALUE: {val.Key.Value} WEIGHT: {val.Key.Key}");
					return val.Value;
				}
			}

			return null;
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