using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using PooledAwait;
using yae.Framing.Parsing;

namespace yae.Framing.IO
{
    public class FrameEncoder<TFrame> : IDisposable
    {
        internal PipeWriter Writer;
        private SemaphoreSlim _semaphore;
        private readonly IFrameWriter<TFrame> _frameWriter;
        public int FramesWritten { get; private set; }

        /// <summary>
        /// You must use <see cref="Reset"/> before using the Encode methods.
        /// </summary>
        /// <param name="frameWriter"></param>
        public FrameEncoder(IFrameWriter<TFrame> frameWriter) => _frameWriter = frameWriter;

        public ValueTask EncodeAsync(TFrame frame, ReadOnlyMemory<byte> payload) => EncodeAsync(frame, payload, default);
        public ValueTask EncodeAsync(TFrame frame, ReadOnlyMemory<byte> payload, CancellationToken token)
        {
            static async PooledValueTask Produce(FrameEncoder<TFrame> obj, TFrame frm, ReadOnlyMemory<byte> p, CancellationToken token)
            {
                if (obj.Writer == null)
                {
                    return;
                }

                var writer = obj.Writer;
                await obj._semaphore.WaitAsync(token).ConfigureAwait(false);

                try
                {
                    await obj._frameWriter.Write(writer, frm, p).ConfigureAwait(false);
                    obj.FramesWritten++;
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }

            return Produce(this, frame, payload, token);
        }

        public ValueTask EncodeAsyncEnumerableAsync(IEnumerable<(TFrame frame, ReadOnlyMemory<byte> payload)> frames) =>
            EncodeAsyncEnumerableAsync(frames, default);
        public ValueTask EncodeAsyncEnumerableAsync(IEnumerable<(TFrame frame, ReadOnlyMemory<byte> payload)> frames, CancellationToken token)
        {
            static async PooledValueTask Produce(FrameEncoder<TFrame> obj, IEnumerable<(TFrame frame, ReadOnlyMemory<byte> payload)> enumerable, CancellationToken t)
            {
                if (obj.Writer == null)
                {
                    return;
                }

                var writer = obj.Writer;

                await obj._semaphore.WaitAsync(t).ConfigureAwait(false);

                try
                {
                    foreach (var (frame, payload) in enumerable)
                    {
                        await obj._frameWriter.Write(writer, frame, payload).ConfigureAwait(false);
                        obj.FramesWritten++;
                    }
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }
            return Produce(this, frames, token);
        }

        public ValueTask EncodeAsyncEnumerableAsync(IAsyncEnumerable<(TFrame frame, ReadOnlyMemory<byte> payload)> frames) =>
            EncodeAsyncEnumerableAsync(frames, default);
        public ValueTask EncodeAsyncEnumerableAsync(IAsyncEnumerable<(TFrame frame, ReadOnlyMemory<byte> payload)> frames, CancellationToken token)
        {
            static async PooledValueTask Produce(FrameEncoder<TFrame> obj, IAsyncEnumerable<(TFrame frame, ReadOnlyMemory<byte> payload)> enumerable, CancellationToken t)
            {
                if (obj.Writer == null)
                {
                    return;
                }

                var writer = obj.Writer;

                await obj._semaphore.WaitAsync(t).ConfigureAwait(false);

                try
                {
                    await foreach (var (frame, payload) in enumerable)
                    {
                        await obj._frameWriter.Write(writer, frame, payload).ConfigureAwait(false);
                        obj.FramesWritten++;
                    }
                }
                finally
                {
                    obj._semaphore.Release();
                }
            }
            return Produce(this, frames, token);
        }

        /// <summary>
        /// Reset or set encoder with a new writer. You must Close (or Dispose) before resetting the encoder.
        /// </summary>
        /// <param name="writer"></param>
        /// <returns>true if it has been successfully reset, otherwise false</returns>
        public bool Reset(PipeWriter writer)
        {
            if (Interlocked.CompareExchange(ref Writer, writer, null) != null)
            {
                return false;
            }

            FramesWritten = 0;
            _semaphore = new SemaphoreSlim(1);
            return true;
        }

        public void Close() => Close(null);
        public void Close(Exception ex)
        {
            var writer = Interlocked.Exchange(ref Writer, null);
            if (writer == null)
            {
                return;
            }

            try { writer.Complete(ex); } catch { /* silent complete */}
            try { writer.CancelPendingFlush(); } catch { /* ignore errors */ }
        }

        public void Dispose()
        {
            Close();
            _semaphore?.Dispose();
            _semaphore = null;
        }
    }
}