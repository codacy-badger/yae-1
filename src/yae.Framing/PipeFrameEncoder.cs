using System.IO.Pipelines;
using System.Threading.Tasks;
using PooledAwait;

namespace yae.Framing
{
    public abstract class PipeFrameEncoder<T> : IFrameEncoder<PipeWriter, T>
    {
        protected abstract ValueTask<FlushResult> Write(PipeWriter writer, T frame);

        public ValueTask WriteAsync(PipeWriter writer, T frame)
        {
            static async PooledValueTask Pooled(PipeFrameEncoder<T> encoder, PipeWriter w, T frm)
            {
                await encoder.Write(w, frm).ConfigureAwait(false);
            }

            return Pooled(this, writer, frame);
        }
    }
}