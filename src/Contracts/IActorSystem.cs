using ActorModelNet.Contracts.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Basic contract of Actor system
    /// </summary>
    public interface IActorSystem
    {

        /// <summary>
        /// 
        /// </summary>
        void Send<TActor, TState>(IActorIdentity identity, object message, IActorIdentity? sender = null)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>;

        /// <summary>
        /// Get Actor Reference
        /// </summary>
        Task<TState> UnsafeGetState<TActor, TState>(IActorIdentity identity)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>;
        /// <summary>
        /// Get Actor State
        /// </summary>
        public Task<TResult> GetState<TActor, TState, TResult>(IActorIdentity identity, Func<TState, TResult> selector)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>;

        /// <summary>
        /// schedulled send message / new time or update
        /// </summary>
        Guid ScheduledSend(IActorIdentity identity, Type actorType, object message, TimeSpan period, Guid? timerId = null);

        /// <summary>
        /// schedulled send message / new time or update
        /// </summary>
        Guid ScheduledSend<TActor>(IActorIdentity identity, object message, TimeSpan period, Guid? timerId = null)
            where TActor : IActor;

    }
}
