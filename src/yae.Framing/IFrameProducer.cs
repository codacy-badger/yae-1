using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace yae.Framing
{
    public interface IFrameProducer<in T> : IDisposable
    {
        ValueTask ProduceAsync(T frame);
        ValueTask ProduceAsync(IEnumerable<T> frames);
        ValueTask ProduceAsync(IAsyncEnumerable<T> framesAsync);
    }
}