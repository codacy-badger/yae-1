using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace yae.Framing
{
    public interface IFrameEncoder<in TState, in TFrame>
    {
        ValueTask WriteAsync(TState writer, TFrame frame);
    }
}