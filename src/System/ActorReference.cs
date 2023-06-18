using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActorModelNet.Contracts;

namespace ActorModelNet.System
{
    /// <summary>
    /// Proxy to access Actor by just its Id.
    /// </summary>
    public class ActorReference<TActor, TState>
        where TActor : class, IActor<TState>
        where TState : class, IEquatable<TState>
    {
        private readonly IActorSystem _actorSystem;
        private readonly IActorIdentity _actorIdentity;

        /// <summary>
        /// 
        /// </summary>
        public ActorReference(IActorSystem actorSystem, IActorIdentity actorIdentity)
        {
            _actorSystem = actorSystem;
            _actorIdentity = actorIdentity;
        }

        /// <summary>
        /// Get actor Identity
        /// </summary>
        public IActorIdentity Identity => _actorIdentity;

        /// <summary>
        /// Receive user messages
        /// </summary>
        public void Send(object message, IActorIdentity? sender = null)
        {
            _actorSystem.Send<TActor, TState>(_actorIdentity, message);
        }

        /// <summary>
        /// Get State of actor - Unsafe because of return state object
        /// </summary>
        public Task<TState> UnsafeGetState()
        {
            return _actorSystem.UnsafeGetState<TActor, TState>(_actorIdentity);
        }

        /// <summary>
        /// Get State of actor
        /// </summary>
        public Task<TResult> GetState<TResult>(Func<TState, TResult> selector)
        {
            return _actorSystem.GetState<TActor, TState, TResult>(_actorIdentity, selector);
        }
    }
}
