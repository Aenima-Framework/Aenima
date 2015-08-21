namespace Aenima
{
    public interface IState
    {
        string Id { get; }
        int Version { get; }
        void Mutate(object e);
    }
}