﻿using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using yae.Framing.Sample.BasicFrame;

namespace yae.Framing.Tests
{
    public class BasicFrameEncoderTests
    {

        [Fact]
        public async Task WriteAsync_WithPayload()
        {
            var (encoder, reader) = GetEncoder();
            var frame = FrameProvider.GetFrame(4);

            await encoder.EncodeAsync(frame, new byte[1024]);
            var result = await reader.ReadAsync();
            Assert.Equal(FrameProvider.HeaderSize + FrameProvider.FrameSize, result.Buffer.Length);
        }

        private static (BasicFrameEncoder encoder, PipeReader reader) GetEncoder()
        {
            var pipe = new Pipe();
            var encoder = new BasicFrameEncoder();
            encoder.Reset(pipe.Writer);
            return (encoder, pipe.Reader);
        }
    }
}
