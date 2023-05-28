using System;
using System.Threading.Tasks;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Base Actor model - Without State
    /// </summary>
    public interface IActor : IDisposable
    {
        /// <summary>
        /// Identity of actor
        /// </summary>
        IActorIdentity Identity { get; }

        /// <summary>
        /// Recieving Message
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="sender">identity of sender (if another actor sent the message)</param>
        void Send(object message, IActorIdentity? sender = null);

    }

    /// <summary>
    /// Base Actor model - With State
    /// </summary>
    public interface IActor<TState> : IActor
        where TState : class, IEquatable<TState>
    {
        /// <summary>
        /// Get a copy of state
        /// </summary>
        Task<TState> GetState();

        /// <summary>
        /// Get a specified model of state
        /// </summary>
        /// <param name="selector">Transformation function</param>
        Task<TResult> GetState<TResult>(Func<TState, TResult> selector);

    }
}
