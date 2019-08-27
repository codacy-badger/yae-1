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
        private readonly AbstractPipeConsumer _pipeConsumer;


        public PipeFrameConsumer(PipeReader reader, IFrameDecoder<T> decoder)
        {
            _reader = reader;
            _pipeConsumer = new PipeConsumer(reader);
            _decoder = decoder;
        }

        /// <summary>
        /// Consumes asynchronously the pipe and returns frames.
        /// Automatically call dispose at the end of the enumerable.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<T> ConsumeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            await foreach (var buffer in _pipeConsumer.ConsumeAsync(token))
            {
                var bufferLocal = buffer; //we can't update result of buffer

                while (_decoder.TryParseFrame(bufferLocal, out var frame, out var consumedTo))
                {
                    yield return frame;
                    bufferLocal = bufferLocal.Slice(consumedTo);
                }

                _reader.AdvanceTo(bufferLocal.Start, bufferLocal.End);
            }
        }

        public void Close(Exception ex = null) => _pipeConsumer.Close(ex);
        public void Dispose() => Close();
    }
}
