using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("yae.Framing.Tests")]
namespace yae.Framing
{
    internal sealed class PipeFrameConsumer<T> : IFrameConsumer<T>
    {
        private PipeReader _reader;
        private readonly IFrameDecoder<T> _decoder;


        public PipeFrameConsumer(PipeReader reader, IFrameDecoder<T> decoder)
        {
            _reader = reader;
            _decoder = decoder;
        }

        /// <summary>
        /// Consumes asynchronously the pipe and returns frames
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ConsumeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            var reader = _reader ?? throw new ObjectDisposedException(ToString());

            await foreach (var buffer in reader.ToAsyncEnumerable(token))
            {
                var bufferLocal = buffer; //we can't update result of buffer

                while (_decoder.TryParseFrame(bufferLocal, out var frame, out var consumedTo))
                {
                    yield return frame;
                    bufferLocal = bufferLocal.Slice(consumedTo);
                }

                reader.AdvanceTo(bufferLocal.Start, bufferLocal.End);
            }

            //Close(); //todo: should we auto-close?
        }


        public void Close(Exception ex = null)
        {
            var reader = Interlocked.Exchange(ref _reader, null);
            if (reader == null) throw new ObjectDisposedException(ToString());

            GC.SuppressFinalize(this);
            try { reader.Complete(ex); } catch { }
            try { reader.CancelPendingRead(); } catch { }
        }

        public void Dispose() => Close();
    }
}
