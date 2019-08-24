using System;
using System.Buffers;

namespace yae.Framing
{
    public abstract class HeaderFrameDecoder<TFrame> : IFrameDecoder<InputFrame<TFrame>>
    {
        public bool TryParseFrame(ReadOnlySequence<byte> buffer, out InputFrame<TFrame> frameWrapper, 
            out SequencePosition consumedTo)
        {
            var reader = new SequenceReader<byte>(buffer);


            var canParseHeader = TryParseHeader(ref reader, out var frame, out var length);
            var canParsePayload = reader.Remaining >= length;

            if(length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (canParseHeader && canParsePayload) 
            {
                frameWrapper = new InputFrame<TFrame>(frame, buffer.Slice(reader.Position, length));
                reader.Advance(length);

                consumedTo = reader.Position;
                return true;
            }

            frameWrapper = default;
            consumedTo = default;
            return false;
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