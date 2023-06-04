namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Persistence Layer for Actor (Snapshot Storage)
    /// </summary>
    public interface IPersistence
    {
        /// <summary>
        /// Restore Actor from Persistence
        /// </summary>
        /// <param name="identity">Identity of actor</param>
        TState Load<TState>(IActorIdentity identity)
            where TState : class;

        /// <summary>
        /// Save into Persistence
        /// </summary>
        /// <param name="identity">Identity of actor</param>
        /// <param name="state">State of actor</param>
        bool Save<TState>(IActorIdentity identity, TState state) 
            where TState : class;
    }
}
