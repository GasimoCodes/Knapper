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
			int maxCapacity = 65;


			Vector2[] items = {
				new Vector2(1, 10),
				new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(1, 10),
				new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(1, 10),
				new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(1, 10),
				new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(1, 10),
				new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(2, 4),
				new Vector2(4, 12),
								new Vector2(2, 4),
				new Vector2(4, 12),

			};

			Vector2[][] result;

			
			stopwatch.Restart();
			result = GetCombinationOptimized(items);
			Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
			
			//Console.ReadLine();
			printCombinations(result);



			stopwatch.Restart();
			result = GetCombinationOptimizedThreaded(items);
			Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");

			//Console.ReadLine();
			printCombinations(result);

			/*
			stopwatch.Restart();
			result = GetCombinationOptimizedParallel(items);
			Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
			
			//Console.ReadLine();
			printCombinations(result);
			
			*/

			

			// Vector2[] value = getBestCapacity(GetCombinationOptimizedParallel(items), 3);

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
						//Console.Write(list[elementIndex].X);
						combination.Add(list[elementIndex]);
					}
				}

				results[combinationIndex - 1] = combination.ToArray();
				//Console.WriteLine();
			}

			return results;
		}

		public static Vector2[] getBestCapacity(Vector2[][] input, int maxCapacity)
		{
			// Index same as input index, (int1 = value, int2 = weight)
			Dictionary<KeyValuePair<int, int>, Vector2[]> values = new Dictionary<KeyValuePair<int, int>, Vector2[]>();

			foreach (Vector2[] val in input)
			{
				int value = (int)val.Sum(x => x.Y);
				int weight = (int)val.Sum(x => x.X);

				//				KeyValuePair<int, int> keyPair = new KeyValuePair<int, int> ( value, weight );

				values.TryAdd(new KeyValuePair<int, int>(value, weight), val);
			}

			foreach (KeyValuePair<KeyValuePair<int, int>, Vector2[]> val in values.OrderByDescending(x => x.Key.Key))
			{
				if (val.Key.Value <= maxCapacity)
				{
					Console.WriteLine($"VALUE: {val.Key.Key} WEIGHT: {val.Key.Value}");
					return val.Value;
				}
			}

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

	}
}