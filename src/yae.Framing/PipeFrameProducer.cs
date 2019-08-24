using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
//using Nito.AsyncEx;

namespace yae.Framing
{
    internal sealed class PipeFrameProducer<T> : IFrameProducer<T>
    {
        private PipeWriter _writer;
        private readonly SemaphoreSlim _semaphore;
        private readonly IFrameEncoder<T> _encoder;

        public PipeFrameProducer(PipeWriter writer, IFrameEncoder<T> encoder)
        {
            _writer = writer;
            _encoder = encoder;
            _semaphore = new SemaphoreSlim(1);
        }

        public ValueTask ProduceAsync(T frame)
        {
            if (!_semaphore.Wait(0)) return WriteAsyncSlowPath(frame);

            var release = true;
            try
            {
                var writer = _writer ?? throw new ObjectDisposedException(ToString());
                var write = _encoder.WriteAsync(writer, frame);
                if (write.IsCompletedSuccessfully) return default;
                release = false;
                return AwaitFlushAndRelease(write);
            }
            finally
            {
                if (release) _semaphore.Release();
            }
        }

        public async ValueTask ProduceAsync(IEnumerable<T> frames)
        {
            var writer = _writer ?? throw new ObjectDisposedException(ToString());

            await _semaphore.WaitAsync();
            try
            {
                foreach (var frame in frames)
                {
                    await _encoder.WriteAsync(writer, frame);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        //todo: can probably refactor both produces
        public async ValueTask ProduceAsync(IAsyncEnumerable<T> framesAsync)
        {
            var writer = _writer ?? throw new ObjectDisposedException(ToString());

            await _semaphore.WaitAsync();
            try
            {
                await foreach (var frame in framesAsync)
                {
                    await _encoder.WriteAsync(writer, frame);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async ValueTask AwaitFlushAndRelease(ValueTask<FlushResult> flush)
        {
            try
            {
                await flush;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async ValueTask WriteAsyncSlowPath(T frame)
        {
            await _semaphore.WaitAsync();
            try
            {
                var writer = _writer ?? throw new ObjectDisposedException(ToString());
                await _encoder.WriteAsync(writer, frame);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            
            var writer = Interlocked.Exchange(ref _writer, null);
            if (writer == null) throw new ObjectDisposedException(ToString());
            try { writer.Complete(); } catch { }
            try { writer.CancelPendingFlush(); } catch { }
            _semaphore.Dispose();
        }
    }
}