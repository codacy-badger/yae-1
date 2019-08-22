using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("yae.Framing.Tests")]
namespace yae.Framing
{
    internal class PipeFrameConsumer<T> : IFrameConsumer<T>
    {
        private PipeReader _reader;
        private readonly IFrameDecoder<T> _decoder;
        private CancellationTokenSource _cts;
        internal bool IsProgressing { get; private set; }


        internal PipeFrameConsumer(PipeReader reader, IFrameDecoder<T> decoder)
        {
            _reader = reader;
            _decoder = decoder;
            IsProgressing = false;
        }

        /// <summary>
        /// Consumes asynchronously the pipe and returns frames
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException">Thrown on cancellation</exception>
        public async IAsyncEnumerable<T> ConsumeAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            //todo: better try-catch style!
            var reader = _reader ?? throw new ObjectDisposedException(ToString()); //to prevent dispose!
            _cts = new CancellationTokenSource();
            while (true)
            {
                /*ReadResult readResult;
                try
                {
                    if (!(IsProgressing && reader.TryRead(out readResult)))
                        readResult = await reader.ReadAsync(token);
                }
                catch
                {
                    break;
                }*/
                /*ReadResult readResult;
                try
                {
                    readResult = await reader.ReadAsync(token);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Close(ex);
                    }
                    catch
                    {
                    }

                    break;
                }


                if (readResult.IsCanceled)
                    break; //try to handle it btw

                var buffer = readResult.Buffer;

                IsProgressing = false;

                while (_decoder.TryParseFrame(buffer, out var frame, out var consumedTo))
                {
                    IsProgressing = true;
                    yield return frame;
                    buffer = buffer.Slice(consumedTo);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (!IsProgressing && readResult.IsCompleted) break;*/

                var buffer = await GetBuffer(_cts);
                IsProgressing = false;

                while (_decoder.TryParseFrame(buffer, out var frame, out var consumedTo))
                {
                    IsProgressing = true;
                    yield return frame;
                    buffer = buffer.Slice(consumedTo);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);


            }
        }

        public async ValueTask<ReadOnlySequence<byte>> GetBuffer(CancellationTokenSource cts)
        {
            try
            {
                var readResult = await _reader.ReadAsync(cts.Token);
                if (readResult.IsCanceled)
                {
                    cts.Cancel();
                    return default;
                }

                var buffer = readResult.Buffer;
                if (!IsProgressing && readResult.IsCompleted) cts.Cancel(false);
                return buffer;
            }
            catch (Exception ex)
            {
                Close(ex);
                return default;
            }
        }

        public void Close(Exception ex = null)
        {
            var reader = Interlocked.Exchange(ref _reader, null);
            if (reader == null) throw new ObjectDisposedException(ToString());

            GC.SuppressFinalize(this);
            try { reader.Complete(ex); } catch { }
            try { reader.CancelPendingRead(); } catch { }
        }

        public void Dispose() => Close();
    }
}
