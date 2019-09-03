using System;

namespace yae.Binary
{
    public interface IReadFormatter<T>
    {
        T Read();
    }

    public interface IWriteFormatter<T>
    {
        void Write(T value);
    }

    public interface IBinaryFormatter<T> : IReadFormatter<T>, IWriteFormatter<T>
    {

    }

    public interface IBinaryReader
    {
        long Position { get; set; }
        long Length { get; }
        long Remaining { get; }

        ReadOnlyMemory<byte> Buffer { get; }
    }

    class MyReader : IBinaryReader, IBinaryFormatter<int>
    {
        public long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public long Length => throw new NotImplementedException();

        public long Remaining => throw new NotImplementedException();

        public ReadOnlyMemory<byte> Buffer => throw new NotImplementedException();

        public int Read()
        {
            throw new NotImplementedException();
        }

        public void Write(int value)
        {
            throw new NotImplementedException();
        }

        public T Read<T>()
        {
            if (this is IReadFormatter<T> formatter)
                return formatter.Read();
            throw new NotImplementedException();
        }

        public void Write<T>(T value)
        {
            if(this is IWriteFormatter<T> formatter)
                formatter.Write(value);
            throw new NotImplementedException();
        }
    }
}
