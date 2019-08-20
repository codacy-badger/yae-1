using System;
using System.Threading.Tasks;

namespace yae.Framing
{
    public interface IFrameProducer<T> : IDisposable
    {
        ValueTask<int> ProduceAsync(T data);
    }
}