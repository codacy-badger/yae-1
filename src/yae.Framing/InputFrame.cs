using System.Buffers;

namespace yae.Framing
{
    public class InputFrame<T>
    {
        public T Frame { get; }
        public ReadOnlySequence<byte> Payload { get; }

        public InputFrame(T frame, ReadOnlySequence<byte> payload)
        {
            Frame = frame;
            Payload = payload;
        }
    }
}