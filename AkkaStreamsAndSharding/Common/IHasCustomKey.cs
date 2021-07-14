namespace AkkaStreamsAndSharding.Common
{
    public interface IHasCustomKey
    {
        object Key { get; }
    }
}