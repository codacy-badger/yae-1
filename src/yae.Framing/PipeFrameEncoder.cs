using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using PooledAwait;

namespace yae.Framing
{
    public abstract class PipeFrameEncoder<TFrame> : IFrameEncoder<TFrame> where TFrame : IFrame
    {
        private PipeWriter _writer;
        private readonly SemaphoreSlim _semaphore;

        protected PipeFrameEncoder(PipeWriter writer)
        {
            _writer = writer;
            _semaphore = new SemaphoreSlim(1);
        }

        protected abstract ValueTask<FlushResult> Write(PipeWriter writer, TFrame frame);

        public ValueTask EncodeAsync(TFrame frame)
        {
            static async PooledValueTask Produce(PipeFrameEncoder<TFrame> obj, TFrame frm)
            {
                if (obj._writer == null)
                {
                    return;
                }

                var writer = obj._writer;
                await obj._semaphore.WaitAsync();

                try
                {
                    await obj.Write(writer, frm);
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }

            return Produce(this, frame);
        }

        public ValueTask EncodeEnumerableAsync(IEnumerable<TFrame> frames, CancellationToken token = default)
        {
            static async PooledValueTask Produce(PipeFrameEncoder<TFrame> obj, IEnumerable<TFrame> enumerable)
            {
                if (obj._writer == null)
                    return;

                var writer = obj._writer;

                await obj._semaphore.WaitAsync();

                try
                {
                    foreach (var frm in enumerable)
                    {
                        await obj.Write(writer, frm);
                    }
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }
            return Produce(this, frames);
        }

        public ValueTask EncodeEnumerableAsync(IAsyncEnumerable<TFrame> frames, CancellationToken token = default)
        {
            static async PooledValueTask Produce(PipeFrameEncoder<TFrame> obj, IAsyncEnumerable<TFrame> enumerable)
            {
                if (obj._writer == null)
                    return;

                var writer = obj._writer;

                await obj._semaphore.WaitAsync();

                try
                {
                    await foreach (var frm in enumerable)
                    {
                        await obj.Write(writer, frm);
                    }
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }
            return Produce(this, frames);
        }

        public void Close(Exception ex = null)
        {
            var writer = Interlocked.Exchange(ref _writer, null);
            if (writer == null) return;
            try { writer.Complete(ex); } catch { }
            try { writer.CancelPendingFlush(); } catch { }
            _semaphore.Dispose();
        }

        public void Dispose() => Close();
    }
}