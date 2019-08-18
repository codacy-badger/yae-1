using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using yae.Buffers.Framing;

namespace yae.Buffers
{
    public interface IPipeConsumer<T> : IDisposable
    {
        IAsyncEnumerable<T> ConsumeAsync(CancellationToken token = default);
    }

    public interface IPipeProducer<T>
    {
        ValueTask<int> ProduceAsync(T data);
    }

    public abstract class PipeProducer<T> : IPipeProducer<T>
    {
        private readonly PipeWriter _writer;
        private readonly SemaphoreSlim _semaphore;

        public PipeProducer(PipeWriter writer)
        {
            _writer = writer;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async ValueTask<int> ProduceAsync(T data)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await WriteAsync(_writer, data);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected abstract ValueTask<int> WriteAsync(PipeWriter writer, T data);
    }

    //todo: rework to handle another frame type (delimiter ones)
    public interface IFrameEncoder<T>
    {
        int GetHeaderLength(T frame);
        void WriteHeader(Span<byte> span, T frame);
        ReadOnlyMemory<byte> GetPayload(T frame);
    }
    public class PipeProducerFrame<T> : PipeProducer<T>
    {
        private readonly IFrameEncoder<T> _encoder;
        public PipeProducerFrame(PipeWriter writer, IFrameEncoder<T> encoder) : base(writer)
        {
            _encoder = encoder;
        }

        protected async override ValueTask<int> WriteAsync(PipeWriter writer, T frame)
        {
            var headerLen = _encoder.GetHeaderLength(frame);
            var headerMem = writer.GetMemory(headerLen);
            _encoder.WriteHeader(headerMem.Span, frame);
            writer.Advance(headerLen);

            var payload = _encoder.GetPayload(frame);
            if (payload.IsEmpty)
                return headerLen;
            
            await writer.WriteAsync(payload);
            return headerLen + payload.Length;
        }
    }

    public static class PipeConsumerFactory
    {
        public static IPipeConsumer<T> CreatePipeConsumerFrame<T>(PipeReader reader, IFrameDecoder<T> decoder)
        {
            return new PipeConsumerFrame<T>(reader, decoder);
        }
    }
}
