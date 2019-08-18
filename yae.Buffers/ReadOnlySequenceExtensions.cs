using System;
using System.Buffers;
using System.Text;

namespace yae.Buffers
{
    public static class ReadOnlySequenceExtensions
    {
        public static string AsString(this ReadOnlySequence<byte> buffer)
        {
            
            if(buffer.IsSingleSegment)
            {
                return Encoding.UTF8.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.UTF8.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }
    }

    /*public abstract class HeaderFrameSplitter<T> : IFrameSplitter
         where T : IFrame
     {
         public bool TryParseFrame(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
         {
             if(!TryParseHeader(buffer, out var frame))
             {
                 payload = default;
                 return false;
             }
             payload = buffer.Slice(0, frame.Length);
             return true;
             throw new NotImplementedException();
         }

         protected abstract bool TryParseHeader(in ReadOnlySequence<byte> buffer, out T frame)
     }*/


}
