namespace yae.Serializer
{
    public interface IFormatter<in TState, T>
    {
        T Deserialize(TState state);
        void Serialize(TState state, T value);
    }
}