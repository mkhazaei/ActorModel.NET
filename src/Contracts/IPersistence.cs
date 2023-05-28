namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Persistence Layer for Actor (Snapshot Storage)
    /// </summary>
    public interface IPersistence<in TIdentity, TState> where TIdentity : IActorIdentity
    {
        /// <summary>
        /// Restore Actor from Persistence
        /// </summary>
        /// <param name="identity">Identity of actor</param>
        TState Load(TIdentity identity);

        /// <summary>
        /// Save into Persistence
        /// </summary>
        /// <param name="identity">Identity of actor</param>
        /// <param name="state">State of actor</param>
        bool Save(TIdentity identity, TState state);
    }
}
