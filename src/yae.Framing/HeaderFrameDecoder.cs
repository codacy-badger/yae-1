using System;
using System.Buffers;

namespace yae.Framing
{
    public abstract class HeaderFrameDecoder<TFrame> : IFrameDecoder<IFrameWrapper<TFrame>>
    {
        public bool TryParseFrame(ReadOnlySequence<byte> buffer, out IFrameWrapper<TFrame> frameWrapper, 
            out SequencePosition consumedTo)
        {
            var reader = new SequenceReader<byte>(buffer);
            if (
                !TryParseHeader(ref reader, out var frame, out var length) || 
                buffer.Length < length)
            {
                if(length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length));

                consumedTo = default;
                frameWrapper = default;
                return false;
            }

            frameWrapper = new FrameWrapper<TFrame>(frame, buffer.Slice(reader.Position, length));
            consumedTo = buffer.GetPosition(length);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequenceReader"></param>
        /// <param name="frame"></param>
        /// <param name="frameLength"></param>
        /// <returns></returns>
        protected abstract bool TryParseHeader(ref SequenceReader<byte> sequenceReader, out TFrame frame, out int length);
    }
}