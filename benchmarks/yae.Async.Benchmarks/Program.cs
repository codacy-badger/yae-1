using System;
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

    public class OldOperation : AsyncOperation
    {
        public readonly object _obj1;
        public readonly object _obj2;
        public readonly object _obj3;

        public OldOperation(object obj1, object obj2, object obj3)
        {
            _obj1 = obj1;
            _obj2 = obj2;
            _obj3 = obj3;
        }
        protected override ValueTask CanCompleteSync()
        {
            return Benchmark.GetTask();
        }
    }

    public class NewOperation : AsyncOperationV2<(object, object, object)>
    {
        protected override ValueTask CanCompleteSync((object, object, object) input)
        {
            return Benchmark.GetTask();
        }
    }

    [CoreJob, MemoryDiagnoser, WarmupCount(2), IterationCount(10)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class Benchmark
    {
        private NewOperation _new;

        [GlobalSetup]
        public void Setup()
        {
            _new = new NewOperation();
        }

        [Benchmark]
        public ValueTask Old()
        {
            var op = new OldOperation(new object(), new object(), new object());
            return op.ExecuteAsync();
        }

        [Benchmark]
        public ValueTask New()
        {
            return _new.ExecuteAsync((new object(), new object(), new object()));
        }

        [Benchmark]
        public ValueTask Plain()
        {
            async ValueTask Await(ValueTask t)
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

            ValueTask GetTask((object obj1, object obj2, object obj3) input)
            {
                return Benchmark.GetTask();
            }

            void OnFinally() { }

            var obj1 = new object();
            var obj2 = new object();
            var obj3 = new object();

            var release = true;
            try
            {
                var task = GetTask((obj1, obj2, obj3));
                if (task.IsCompletedSuccessfully) return default;
                release = false;
                return Await(task);
            }
            finally{ if(release) OnFinally(); }
        }

        public static ValueTask GetTask() => new ValueTask(Task.Delay(1));
    }
}
