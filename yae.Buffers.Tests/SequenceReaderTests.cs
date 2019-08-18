using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace yae.Buffers.Tests
{
    //todo: fix the parameterless constructor (or handle it)
    public class SequenceReaderTests
    {
        [Theory]
        [InlineData(typeof(int), 45, 4)]
        public void TryRead_Tests(Type type, object value, int size)
        {
            dynamic val = value;

            Assert.IsType(type, val);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(val);

            /*var sr = new SequenceReader(new System.Buffers.ReadOnlySequence<byte>(ms.ToArray()));
            var canRead = sr.TryRead(src => BinaryPrimitives.ReadInt32LittleEndian(src), out var result);
            Assert.True(canRead);
            Assert.Equal(val, result);
            Assert.Equal(size, sr.Offset);*/
        }

        [Fact]
        public void Test()
        {
            var builder = new SequenceReaderBuilder<TestStore>();
            builder.AddFixed(BinaryPrimitives.ReadInt32LittleEndian);
            /*builder
                .Add(src => BinaryPrimitives.ReadInt16BigEndian(src));*/
        }
    }

    public class TestStore
    {

    }
}
