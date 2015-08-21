namespace Aenima
{
    public abstract class State : IState
    {
        public string Id { get; protected set; } = string.Empty;

        public int Version { get; protected set; } = -1;

        public void Mutate(object domainEvent)
        {
            // .NET magic to call one of the 'When' handlers with matching signature 
            ((dynamic)this).When((dynamic)domainEvent);
            Version++;
        }
    }
}