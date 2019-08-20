using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("yae.Framing.Tests")]
namespace yae.Framing
{
    internal class PipeFrameConsumer<T> : IFrameConsumer<T>
    {
        private PipeReader _reader;
        private readonly IFrameDecoder<T> _decoder;

        internal bool IsProgressing { get; private set; }


        internal PipeFrameConsumer(PipeReader reader, IFrameDecoder<T> decoder)
        {
            _reader = reader;
            _decoder = decoder;
            IsProgressing = false;
        }

        /// <summary>
        /// Consumes asynchronously the pipe and returns frames
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException">Thrown on cancellation</exception>
        public async IAsyncEnumerable<T> ConsumeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            var reader = _reader; //to prevent dispose!
            var holder = new BufferHolder();
            while (true)
            {
                ReadResult readResult;
                try
                {
                    if (!(IsProgressing && reader.TryRead(out readResult)))
                        readResult = await reader.ReadAsync(token);
                }
                catch
                {
                    break;
                }


                if (readResult.IsCanceled)
                    break; //try to handle it btw

                holder.Buffer = readResult.Buffer;

                IsProgressing = false;

                foreach (var frame in _decoder.AsEnumerable(holder))
                {
                    IsProgressing = true;
                    yield return frame;
                }

                reader.AdvanceTo(holder.Buffer.Start, holder.Buffer.End);

                if (!IsProgressing && readResult.IsCompleted) break;

            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            var reader = Interlocked.Exchange(ref _reader, null);
            if (reader == null) return;
            try { reader.Complete(); } catch { }
            try { reader.CancelPendingRead(); } catch { }
        }
    }
}
