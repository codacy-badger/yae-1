using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace yae.Async.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmark>();
            Console.WriteLine(summary);
        }
    }

    public class DumbAsyncOperation : VoidAsyncOperation<object>
    {
        protected override ValueTask CanExecuteSynchronous(object input) => Benchmark.GetTask();
        protected override void Continuation(object input)
        {
            //throw new NotImplementedException();
        }
    }

    public class SemaphoreOperation : VoidAsyncOperation<SemaphoreSlim>
    {
        protected override ValueTask CanExecuteSynchronous(SemaphoreSlim input)
        {
            return input.Wait(0) ? default : new ValueTask(input.WaitAsync());
        }

        protected override void Continuation(SemaphoreSlim input)
        {
            input.Release();
        }
    }

    [CoreJob, MemoryDiagnoser, WarmupCount(2), IterationCount(10)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class Benchmark
    {
        private DumbAsyncOperation _operation;
        private SemaphoreSlim _semaphore1;
        private SemaphoreSlim _semaphore2;
        private SemaphoreOperation _semaphoreOperation;

        [GlobalSetup]
        public void Setup()
        { 
            _semaphore1 = new SemaphoreSlim(1);
            _semaphore2 = new SemaphoreSlim(1);
            _operation = new DumbAsyncOperation();
            _semaphoreOperation = new SemaphoreOperation();
        }

        //[Benchmark]
        public ValueTask New()
        {
            return _operation.ExecuteAsync(null);
        }

        //[Benchmark]
        public ValueTask Plain()
        {
            static async ValueTask Await(ValueTask t)
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                finally
                {
                    OnFinally();
                }
            }

            static ValueTask GetTask()
            {
                return Benchmark.GetTask();
            }

            static void OnFinally() { }

            var release = true;
            try
            {
                var task = GetTask();
                if (task.IsCompletedSuccessfully) return default;
                release = false;
                return Await(task);
            }
            finally{ if(release) OnFinally(); }
        }

        [Benchmark]
        public ValueTask Plain_Semaphore()
        {
            var semaphore = _semaphore1;
            async ValueTask AwaitSemaphore(Task t)
            {
                try
                {
                    await t.ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            var release = true;
            var lockTaken = semaphore.Wait(0);
            try
            {
                if (lockTaken) return default;
                release = false;
                return AwaitSemaphore(semaphore.WaitAsync());
            }
            finally
            {
                if (release) semaphore.Release();
            }
        }

        [Benchmark]
        public ValueTask New_Semaphore()
        {
            return _semaphoreOperation.ExecuteAsync(_semaphore2);
        }

        [Benchmark]
        public ValueTask Plain_Merge()
        {
            var semaphore = _semaphore1;

            async ValueTask AwaitContinuation(ValueTask t)
            {
                try
                {
                    await t;
                }
                finally
                {
                    semaphore.Release();
                }
            }

            async ValueTask AwaitBoth(Task t, ValueTask continuation)
            {
                try
                {
                    await t.ConfigureAwait(false);
                    await continuation.ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            var release = true;
            var lockTaken = semaphore.Wait(0);
            if (!lockTaken) return AwaitBoth(semaphore.WaitAsync(), GetTask());

            try
            {
                var task = GetTask();
                if (task.IsCompletedSuccessfully) return default;
                release = false;
                return AwaitContinuation(task);
            }
            finally
            {
                if (release) semaphore.Release();
            }
        }

        [Benchmark]
        public ValueTask New_Merge()
        {
            return _semaphoreOperation.MergeWith(_semaphore2, _operation, null);
        }


        public static ValueTask GetTask() => default;
    }
}
