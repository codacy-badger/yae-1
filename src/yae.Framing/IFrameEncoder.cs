using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace yae.Framing
{
    //todo: may we change PipeWriter by a T ?
    public interface IFrameEncoder<in TFrame>
    {
        ValueTask<FlushResult> WriteAsync(PipeWriter writer, TFrame outputFrame);
    }
}