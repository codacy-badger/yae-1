using System.Buffers;

namespace yae.Framing
{
    internal class BufferHolder
    {
        public ReadOnlySequence<byte> Buffer { get; set; }
    }

    public readonly ref struct SequenceHolder
    {
        public readonly ReadOnlySequence<byte> Sequence { get; }

        public SequenceHolder(ReadOnlySequence<byte> sequence)
        {
            Sequence = sequence;
        }
    }
}
