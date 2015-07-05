namespace Aenima
{
    public abstract class State : IState
    {
        public string Id { get; private set; } = string.Empty;

        public int Version { get; private set; } = -1;

        public void Mutate(IEvent e)
        {
            // .NET magic to call one of the 'When' handlers with matching signature 
            ((dynamic)this).When(e);
            Version++;
        }
    }
}