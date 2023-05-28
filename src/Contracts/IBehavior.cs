using System;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Base Actor Behavior that define State Transition by getting Message and State
    /// </summary>
    public interface IBehavior<TState>
        where TState : class, IEquatable<TState>
    {
        /// <summary>
        /// Get Message and State and return new Sate and modification
        /// </summary>
        TState Handle(TState state, object envelop);
    }
}
