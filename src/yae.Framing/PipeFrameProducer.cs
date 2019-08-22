using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace yae.Framing
{
    public abstract class PipeFrameProducer<T> : IFrameProducer<T>
    {
        private PipeWriter _writer;
        internal readonly SemaphoreSlim _semaphore;

        protected PipeFrameProducer(PipeWriter writer)
        {
            _writer = writer;
            _semaphore = new SemaphoreSlim(1);
        }

        public ValueTask<int> ProduceAsync(T frame)
        {
            async ValueTask<int> AwaitFlushAndRelease(ValueTask<FlushResult> flush)
            {
                try
                {
                    await flush;
                    return 0;
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            if (!_semaphore.Wait(0))
            {
                return WriteAsyncSlowPath(frame);
            }

            var release = true;
            try
            {
                var writer = _writer ?? throw new ObjectDisposedException(ToString());
                var write = WriteAsync(writer, frame);
                if (write.IsCompletedSuccessfully) return new ValueTask<int>(0);
                release = false;
                return AwaitFlushAndRelease(write);
            }
            finally
            {
                if (release) _semaphore.Release();
            }
        }

        private async ValueTask<int> WriteAsyncSlowPath(T frame)
        {
            await _semaphore.WaitAsync();
            try
            {
                var writer = _writer ?? throw new ObjectDisposedException(ToString());
                await WriteAsync(writer, frame);
                return -1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected abstract ValueTask<FlushResult> WriteAsync(PipeWriter writer, T frame);
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            
            var writer = Interlocked.Exchange(ref _writer, null);
            if (writer == null) return;
            try { writer.Complete(); } catch { }
            try { writer.CancelPendingFlush(); } catch { }
            _semaphore.Dispose();
        }
    }
}