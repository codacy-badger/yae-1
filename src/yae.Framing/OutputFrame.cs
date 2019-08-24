using System;

namespace yae.Framing
{
    public class OutputFrame<T>
    {
        public T Frame { get; }
        public ReadOnlyMemory<byte> Payload { get; }

        public OutputFrame(T frame, ReadOnlyMemory<byte> payload)
        {
            Frame = frame;
            Payload = payload;
        }
    }
}