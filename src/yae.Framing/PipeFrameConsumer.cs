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
    internal sealed class PipeFrameConsumer<T> : PipeConsumer, IFrameConsumer<T>
    {
        private readonly PipeReader _reader;
        private readonly IFrameDecoder<T> _decoder;


        public PipeFrameConsumer(PipeReader reader, IFrameDecoder<T> decoder) : base(reader)
        {
            _reader = reader;
            _decoder = decoder;
        }

        public new async IAsyncEnumerable<T> ConsumeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            await foreach (var buffer in base.ConsumeAsync(token))
            {
                /*var reader = new SequenceReader<byte>(buffer);
                var bufferLocal = buffer; //we can't update result of buffer

                while (_decoder.TryParseFrame(bufferLocal, out var frame, out var consumedTo))
                {
                    yield return frame;
                    bufferLocal = bufferLocal.Slice(consumedTo);
                }

                _reader.AdvanceTo(bufferLocal.Start, bufferLocal.End);*/
                foreach (var frame in ParseFrames(buffer))
                {
                    yield return frame;
                }
            }
        }

        private IEnumerable<T> ParseFrames(ReadOnlySequence<byte> buffer)
        {
            while (_decoder.TryParseFrame(new SequenceReader<byte>(buffer), out var frame, out var consumedTo))
            {
                yield return frame;
                buffer = buffer.Slice(consumedTo);
            }

            _reader.AdvanceTo(buffer.Start, buffer.End);
        }
    }
}
