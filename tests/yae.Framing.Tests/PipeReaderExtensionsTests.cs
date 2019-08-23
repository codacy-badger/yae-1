using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace yae.Framing.Tests
{
    public class PipeReaderExtensions
    {
        [Fact]
        public async Task TryReadAsync_ShouldCancelWithToken()
        {
            var (reader, _) = GetPipe();

            using var cts = new CancellationTokenSource();
            var readTask = reader.TryReadAsync(cts.Token);
            cts.Cancel();
            var (success, _) = await readTask;
            Assert.False(success);
        }

        [Fact]
        public async Task TryReadAsync_ShouldComplete()
        {
            var (reader, _) = GetPipe();
            reader.Complete();
            var (success, _) = await reader.TryReadAsync();
            Assert.False(success);
        }

        [Fact]
        public async Task TryReadAsync_ShouldReadSuccessfully()
        {
            var (reader, writer) = GetPipe();
            await writer.WriteAsync(new byte[256]);
            var (success, readResult) = await reader.TryReadAsync();
            Assert.True(success);
            Assert.Equal(256, readResult.Buffer.Length);
        }

        //todo: find a better name tho
        [Fact]
        public async Task ToEnumerable_NotThrowOnCancel()
        {
            var (reader, _) = GetPipe();
            using var cts = new CancellationTokenSource();
            var enumerator = reader.ToAsyncEnumerable(cts.Token).GetAsyncEnumerator(cts.Token);
            cts.Cancel();
            Assert.False(await enumerator.MoveNextAsync());
        }

        [Fact]
        public async Task ToEnumerable_Read()
        {
            var (reader, writer) = GetPipe();
            var enumerator = reader.ToAsyncEnumerable().GetAsyncEnumerator();
            var moveNextTask = enumerator.MoveNextAsync();
            await writer.WriteAsync(new byte[256]);
            Assert.True(await moveNextTask);
            Assert.Equal(256, enumerator.Current.Length);
            reader.Complete();
            Assert.False(await enumerator.MoveNextAsync());
        }


        private static (PipeReader reader, PipeWriter writer) GetPipe()
        {
            var pipe = new Pipe();
            return (pipe.Reader, pipe.Writer);
        }
    }
}
