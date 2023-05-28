using System;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Base Actor Behavior that define State Transition by getting Message and State
    /// </summary>
    public interface IActorBehavior<TState>
        where TState : class, IEquatable<TState>
    {
        /// <summary>
        /// Get Message and State and return new Sate and modification
        /// </summary>
        TState Handle(MessageEnvelop envelop, TState state);
    }
}
