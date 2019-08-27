using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PooledAwait;

namespace yae.Framing
{
    public class PipeConsumer : AbstractPipeConsumer
    {
        public PipeConsumer(PipeReader reader) : base(reader) { }
        protected override ValueTask<ReadResult> ReadAsync() => Reader.ReadAsync();
    }

    public abstract class AbstractPipeConsumer : IFrameConsumer<ReadOnlySequence<byte>>
    {
        protected PipeReader Reader;

        protected AbstractPipeConsumer(PipeReader reader)
        {
            Reader = reader;
        }

        protected abstract ValueTask<ReadResult> ReadAsync();
        public async IAsyncEnumerable<ReadOnlySequence<byte>> ConsumeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            { 
                static async PooledValueTask<ReadResult> AsPooled(AbstractPipeConsumer r)
                {
                    var reader = r ?? throw new ObjectDisposedException(r.ToString());
                    return await reader.ReadAsync().ConfigureAwait(false);
                }

                ReadResult result;
                try
                {
                    result = await AsPooled(this);
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

                yield return buffer;

                if (result.IsCompleted)
                    break;
            }

            Close();
        }

        public void Dispose() => Close();

        public void Close(Exception ex = null)
        {
            var reader = Interlocked.Exchange(ref Reader, null);
            if (reader == null) throw new ObjectDisposedException(ToString());

            try { reader.Complete(ex); } catch {}
            try { reader.CancelPendingRead(); } catch {}
        }
    }
}
