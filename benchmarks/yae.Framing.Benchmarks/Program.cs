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

    class Encoder : IFrameEncoder<Frame>
    {
        public int GetHeaderLength(Frame frame) => 4;
        public ReadOnlyMemory<byte> GetPayload(Frame frame) => frame.Data;

        public void WriteHeader(Span<byte> span, Frame frame)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span, frame.Data.Length);
        }
    }
    class Decoder : IFrameDecoder<Frame>
    {
        private byte[] _data;
        public Decoder()
        {
            _data = new byte[1024];
        }
        public bool TryParseFrame(ReadOnlySequence<byte> buffer, out Frame frame, out SequencePosition consumedTo)
        {
            var reader = new SequenceReader<byte>(buffer);

            if(!reader.TryReadLittleEndian(out int length))
            {
                frame = default;
                consumedTo = default;
                return false;
            }

            if(reader.Remaining < length)
            {
                frame = default;
                consumedTo = default;
                return false;
            }

            frame = new Frame
            {
                Data = _data
            };

            consumedTo = reader.Sequence.GetPosition(4 + length);
            return true;
        }
    }

    //[ClrJob, CoreJob, MemoryDiagnoser, WarmupCount(2), IterationCount(10)]
    [CoreJob, MemoryDiagnoser, WarmupCount(2), IterationCount(1)]
    public class PipeFrameBenchmarks
    {
        IFrameConsumer<Frame> _consumer;
        IFrameProducer<Frame> _producer;

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

        [Benchmark]
        public async Task ProducerToConsumer()
        {
            int n = 10;
            var enumerator = _consumer.ConsumeAsync().GetAsyncEnumerator();
            for (int i = 0; i < n; i++)
            {
                await _producer.ProduceAsync(frm);
                if (!await enumerator.MoveNextAsync())
                    break;

                var frame = enumerator.Current;
                if(frame.Data.Length != 1024)
                    throw new Exception("Length doenst match");
            }
            Console.WriteLine($"Test ended up with {n} it");
        }

        //[Benchmark]
        public async Task Test()
        {
            await Task.Delay(1);
        }
    }
}
