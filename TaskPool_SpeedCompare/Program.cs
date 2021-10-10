using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace TaskPool_SpeedCompare
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var summary = BenchmarkRunner.Run<Tests>();

            Console.ReadLine();
        }
    }

    public class Tests
    {

        public const int Pool = 10;

        [Params(10, 20, 100)]
        public static int Max;
        public static List<int> DelayList = null!;

        public static int Count;

        public async Task TestTask(int s)
        {
            await Task.Delay(s);
            Interlocked.Increment(ref Count);
            Console.WriteLine($"TestTask({s})");
        }

        [GlobalSetup]
        public void TestGlobalSetup()
        {

            DelayList = new List<int>(Max);
            for (var i = 0; i < Max; i++)
            {
                var rand = new Random();
                DelayList.Add(Convert.ToInt32(rand.NextDouble() * 500));
            }

            Console.WriteLine($"Initialized Tests with {Max} delays");
        }

        [IterationCleanup]
        public void TestCleanup()
        {
            Console.WriteLine($"Task Count: {Count}");
            Count = 0;
        }

        [Benchmark]
        public async Task Test1()
        {

            Console.WriteLine("Running Test 1");

            var tasks = DelayList.Select(TestTask).ToList();

            using SemaphoreSlim throttler = new(Pool);
            var postTaskTasks = new List<Task>();

            foreach (var task in tasks)
            {
                await throttler.WaitAsync();
                postTaskTasks.Add(Task.Run(() => {
                    task.Wait();
                    throttler.Release();
                }));
            }
            tasks.Clear();

            await Task.WhenAll(postTaskTasks);
        }

        [Benchmark]
        public void Test2()
        {

            Console.WriteLine("Running Test 2");

            var tasks = new List<Task>();
            var count = 0L;
            foreach (var r in DelayList)
            {
                tasks.Add(TestTask(r));
                Interlocked.Increment(ref count);

                if ((Interlocked.Read(ref count) % Pool).Equals(0) || Interlocked.Read(ref count) == 0)
                {
                    Task t = Task.WhenAll(tasks);
                    tasks.Clear();
                    try
                    {
                        t.Wait();
                    }
                    catch
                    {
                        //ignore
                    }
                }
            }
        }

        [Benchmark]
        public void Test3()
        {

            Console.WriteLine("Running Test 3");

            Parallel.ForEach(DelayList, new ParallelOptions { MaxDegreeOfParallelism = Pool }, t => TestTask(t).Wait());
        }

        [Benchmark]
        public void Test4()
        {

            Console.WriteLine("Running Test 4");

            using var throttler = new SemaphoreSlim(Pool);
            var tasks = new List<Task>();
            foreach (var r in DelayList)
            {
                throttler.Wait();

                Task t = Task.Factory.StartNew(() => {
                    try
                    {
                        TestTask(r).Wait();
                    }
                    finally
                    {
                        throttler.Release();
                    }
                });

                tasks.Add(t);
            }

            Task.WhenAll(tasks.ToArray());
        }

        [Benchmark]
        public async Task Test5()
        {

            Console.WriteLine("Running Test 5");

            var tasks = new List<Task>();

            var count = 0L;
            foreach (var r in DelayList)
            {
                tasks.Add(TestTask(r));
                Interlocked.Increment(ref count);

                if ((Interlocked.Read(ref count) % Pool).Equals(0) || Max == Interlocked.Read(ref count))
                {
                    await Task.WhenAll(tasks);
                }
            }
        }

    }
}
