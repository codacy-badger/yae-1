using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using PooledAwait;

namespace yae.Framing
{
    internal sealed class PipeFrameProducer<T> : IFrameProducer<T>
    {
        private PipeWriter _writer;
        private readonly SemaphoreSlim _semaphore;
        private readonly PipeFrameEncoder<T> _encoder;

        public PipeFrameProducer(PipeWriter writer, PipeFrameEncoder<T> encoder)
        {
            _writer = writer;
            _encoder = encoder;
            _semaphore = new SemaphoreSlim(1);
        }

        public ValueTask ProduceAsync(T frame)
        {
            static async PooledValueTask Produce(PipeFrameProducer<T> obj, T frm)
            {
                var writer = obj._writer ?? throw new ObjectDisposedException(nameof(PipeFrameProducer<T>));

                await obj._semaphore.WaitAsync();

                try
                {
                    await obj._encoder.WriteAsync(writer, frm);
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }
            
            return Produce(this, frame);
        }

        public ValueTask ProduceAsync(IEnumerable<T> frames)
        {
            static async PooledValueTask Produce(PipeFrameProducer<T> obj, IEnumerable<T> enumerable)
            {
                var writer = obj._writer ?? throw new ObjectDisposedException(nameof(PipeFrameProducer<T>));

                await obj._semaphore.WaitAsync();

                try
                {
                    foreach (var frm in enumerable)
                    {
                        await obj._encoder.WriteAsync(writer, frm);
                    }
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }
            return Produce(this, frames);
        }

        public ValueTask ProduceAsync(IAsyncEnumerable<T> frames)
        {
            static async PooledValueTask Produce(PipeFrameProducer<T> obj, IAsyncEnumerable<T> enumerable)
            {
                var writer = obj._writer ?? throw new ObjectDisposedException(nameof(PipeFrameProducer<T>));

                await obj._semaphore.WaitAsync();

                try
                {
                    await foreach (var frm in enumerable)
                    {
                        await obj._encoder.WriteAsync(writer, frm);
                    }
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }
            return Produce(this, frames);
        }

        public void Dispose()
        {
            var writer = Interlocked.Exchange(ref _writer, null);
            if (writer == null) throw new ObjectDisposedException(ToString());
            try { writer.Complete(); } catch { }
            try { writer.CancelPendingFlush(); } catch { }
            _semaphore.Dispose();
        }
    }
}