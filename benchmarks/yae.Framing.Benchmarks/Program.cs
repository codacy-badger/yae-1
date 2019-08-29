using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Tasks;


namespace yae.Framing.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PipeFrameBenchmarks>();
            Console.WriteLine(summary);
            Console.WriteLine("fini");
            Console.ReadLine();
            //Console.WriteLine("Hello World!");
        }
    }

    class Frame
    {
        public Memory<byte> Data { get; set; }
    }


    //[ClrJob, CoreJob, MemoryDiagnoser, WarmupCount(2), IterationCount(10)]
    [CoreJob, MemoryDiagnoser, WarmupCount(2), IterationCount(1)]
    public class PipeFrameBenchmarks
    {

        Frame frm;

        [GlobalSetup]
        public void Setup()
        {
            frm = new Frame { Data = new byte[1024] };
        }

        [IterationSetup]
        public void It()
        {
            var pipe = new Pipe();
            //_consumer = PipeConsumerFactory.CreatePipeConsumerFrame(pipe.Reader, new Decoder());
            //_producer = PipeProducerFactory.CreatePipeProducerFrame(pipe.Writer, new Encoder());
        }

        //[Benchmark]
        public async Task Test()
        {
            await Task.Delay(1);
        }
    }
}
