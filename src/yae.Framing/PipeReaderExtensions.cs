using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace yae.Framing
{
    public static class PipeReaderExtensions
    {
        public static async ValueTask<(bool success, ReadResult result)> TryReadAsync
            (this PipeReader reader, CancellationToken token = default)
        {
            try
            {
                var result = await reader.ReadAsync(token);
                return (true, result);
            }
            catch (OperationCanceledException) //Cancellation support
            {
                return (false, default);
            }
            catch (InvalidOperationException) //Reading after completion
            {
                return (false, default);
            }
        }

        public static async IAsyncEnumerable<ReadOnlySequence<byte>> ToAsyncEnumerable
            (this PipeReader reader, [EnumeratorCancellation] CancellationToken token = default)
        {
            while (true)
            {
                var (success, result) = await reader.TryReadAsync(token); 
                if (!success)
                    yield break;

                if (result.IsCanceled)
                    yield break; //can remove this

                var buffer = result.Buffer;
                if (buffer.IsEmpty && result.IsCompleted)
                    yield break;

                yield return buffer;
            }
        }
    }
}
