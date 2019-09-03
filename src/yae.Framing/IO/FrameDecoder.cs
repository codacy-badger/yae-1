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
using yae.Framing.Parsing;

[assembly: InternalsVisibleTo("yae.Framing.Tests")]
namespace yae.Framing.IO
{
    public class FrameDecoder<TFrame> : IDisposable
    {
        private readonly IFrameReader<TFrame> _frameReader;
        private PipeReader _reader;
        public int FramesRead { get; private set; }

        public FrameDecoder(IFrameReader<TFrame> frameReader) => _frameReader = frameReader;

        public IAsyncEnumerable<(TFrame frame, ReadOnlySequence<byte> payload)> DecodeAsync()
            => DecodeAsync(default);

        public async IAsyncEnumerable<(TFrame frame, ReadOnlySequence<byte> payload)> 
            DecodeAsync([EnumeratorCancellation] CancellationToken token)
        {
            while (true)
            {
                static async PooledValueTask<ReadResult> AsPooled(PipeReader r, CancellationToken t)
                {
                    return await r.ReadAsync(t).ConfigureAwait(false);
                }

                var reader = _reader;
                if (reader == null)
                {
                    break;
                }

                var result = await AsPooled(reader, token);

                if (result.IsCanceled)
                {
                    break;
                }

                var buffer = result.Buffer;
                if (buffer.IsEmpty && result.IsCompleted)
                {
                    break;
                }

                foreach (var frame in ParseFrames(buffer))
                {
                    yield return frame;
                }

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }

        private IEnumerable<(TFrame frame, ReadOnlySequence<byte> payload)> ParseFrames(ReadOnlySequence<byte> buffer)
        {
            while (true)
            {
                var reader = new SequenceReader<byte>(buffer);
                if (!_frameReader.TryParseFrame(ref reader, out var frame, out var length))
                {
                    break;
                }

                if(length < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }

                if (reader.Remaining < length)
                {
                    break;
                }

                var payload = buffer.Slice(reader.Position, length);
                FramesRead++;
                yield return (frame, payload);
                buffer = buffer.Slice(payload.End);

            }

            _reader.AdvanceTo(buffer.Start, buffer.End);
        }

        public bool Reset(PipeReader reader)
        {
            if (Interlocked.CompareExchange(ref _reader, reader, null) != null)
            {
                return false;
            }

            FramesRead = 0;
            return true;
        }

        public void Close() => Close(null);
        public void Close(Exception ex)
        {
            var reader = Interlocked.Exchange(ref _reader, null);
            if (reader == null)
            {
                return;
            }

            try { reader.Complete(ex); } catch { /* silent complete */ }
            try { reader.CancelPendingRead(); } catch { /* silent cancel */ }
        }
        public void Dispose() => Close();
    }
}
