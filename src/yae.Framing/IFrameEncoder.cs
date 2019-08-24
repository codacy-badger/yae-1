using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing
{
    public interface IFrameEncoder<in TFrame>
    {
        ValueTask<FlushResult> WriteAsync(PipeWriter writer, TFrame frameWrapper);
    }
}