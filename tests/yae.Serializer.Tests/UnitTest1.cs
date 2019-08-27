using System;
using System.Buffers.Binary;
using System.IO;
using Xunit;

namespace yae.Serializer.Tests
{
    class ProtocolRequired
    {
        public int RequiredVersion { get; set; }
        public int CurrentVersion { get; set; }
    }

    class TestStore
    {
    }

    public class BinaryWriterFormatter : IFormatter<BinaryWriter, int>
    {
        public int Deserialize(BinaryWriter state)
        {
            throw new NotImplementedException();
        }

        public void Serialize(BinaryWriter state, int value)
        {
            state.Write(value);
        }
    }
    class TestSerializerBuilder : SerializerBuilder<BinaryWriter, TestStore>
    {
        public TestSerializerBuilder()
        {
            Register(new BinaryWriterFormatter());
        }
    }


    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var pr = new ProtocolRequired {RequiredVersion = 147, CurrentVersion = 148};
            Span<byte> dst = new byte[8];
            BinaryPrimitives.WriteInt32LittleEndian(dst, pr.RequiredVersion);
            BinaryPrimitives.WriteInt32LittleEndian(dst.Slice(4), pr.CurrentVersion);

            var builder = new TestSerializerBuilder();
            var serializer = builder.Build();
            var memoryOwner = serializer.Serialize(pr);
            Assert.Equal(dst.Length, memoryOwner.Memory.Span.Length); //convert to array for tests
        }

        //public static void (Serializer<BinaryWriter, >)
    }


}
