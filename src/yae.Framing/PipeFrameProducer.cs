using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace yae.Framing
{
    public abstract class PipeFrameProducer<T> : IFrameProducer<T>
    {
        private readonly AsyncLock _mutex;
        private PipeWriter _writer;

        protected PipeFrameProducer(PipeWriter writer)
        {
            _writer = writer;
            _mutex = new AsyncLock();
        }

        public async ValueTask<int> ProduceAsync(T frame)
        {
            using (await _mutex.LockAsync())
            {
                return await WriteAsync(_writer, frame);
            }
        }

        protected abstract ValueTask<int> WriteAsync(PipeWriter writer, T frame);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            var writer = Interlocked.Exchange(ref _writer, null);
            if (writer == null) return;
            try { writer.Complete(); } catch { }
            try { writer.CancelPendingFlush(); } catch { }
        }
    }
}