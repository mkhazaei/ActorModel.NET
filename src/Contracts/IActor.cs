using System;
using System.Threading.Tasks;

namespace ActorModelNet.Contracts
{

    /// <summary>
    /// Base Actor model - Base
    /// </summary>
    public interface IActor
    {

    }

    /// <summary>
    /// Base Actor model - Without State
    /// </summary>
    public interface IActor<TState> : IActor
        where TState : class, IEquatable<TState>
    {

        /// <summary>
        /// Initial value of State
        /// </summary>
        TState InitialState();

        /// <summary>
        /// Behaviour of Actor - Control the lifetime of behaviour from here (Singletone / Transient)
        /// </summary>
        IActorBehavior<TState> Behaviour();
    }

}
