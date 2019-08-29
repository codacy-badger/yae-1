using PooledAwait;
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
    public abstract class PipeFrameDecoder<TFrame> : IFrameDecoder<TFrame> where TFrame : IFrame
    {
        private PipeReader _reader;

        protected PipeFrameDecoder(PipeReader reader)
        {
            _reader = reader;
        }

        public async IAsyncEnumerable<TFrame> DecodeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                static async PooledValueTask<ReadResult> AsPooled(PipeReader reader)
                {
                    return await reader.ReadAsync().ConfigureAwait(false);
                }

                if (_reader == null)
                    yield break;

                ReadResult result;
                try
                {
                    result = await AsPooled(_reader);
                }
                catch (Exception ex)
                {
                    Close(ex);
                    yield break;
                }

                if (result.IsCanceled)
                    break;

                var buffer = result.Buffer;
                if (buffer.IsEmpty && result.IsCompleted)
                    break;

                while (TryParseFrame(new SequenceReader<byte>(buffer), out var frame, out var consumedTo))
                {
                    yield return frame;
                    buffer = buffer.Slice(consumedTo);
                }

                _reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                    break;
            }

            Close();
        }
        public abstract bool TryParseFrame(SequenceReader<byte> reader, out TFrame frame, out SequencePosition consumedTo);

        public void Close(Exception ex = null)
        {
            var reader = Interlocked.Exchange(ref _reader, null);
            if (reader == null) return;

            try { reader.Complete(ex); } catch { }
            try { reader.CancelPendingRead(); } catch { }
        }

        public void Dispose() => Close();
    }
}
