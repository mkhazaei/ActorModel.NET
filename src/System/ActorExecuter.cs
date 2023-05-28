using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActorModelNet.Contracts;
using ActorModelNet.Contracts.Exceptions;
using Microsoft.Extensions.Logging;

namespace ActorModelNet.System
{
    /// <summary>
    /// Simple Actor model
    /// There is no parent-child hierarchies and Clustering configuration.
    /// Actors will be deactivated and stored in DB and restored on demands.
    /// </summary>
    public class ActorExecuter<TState> : IDisposable
        where TState : class, IEquatable<TState>
    {

        #region Private Fields
        private static readonly int _batchExecuteSize = 100;

        private readonly ConcurrentQueue<(object Message, IActorIdentity? Sender)> _messageQueue; // MailBox
        private readonly IActorIdentity _identity;
        private readonly Func<IActorBehavior<TState>> _behaviourFactory; // Instanse of 
        private readonly ILogger _logger;

        private TState _state; // Isolated State

        #endregion

        #region ctor & Factory

        /// <summary>
        /// Ctor
        /// </summary>
        public ActorExecuter(IActorIdentity identity,
            IActor<TState> actor,
            ILogger logger,
            TState? initState = null)
        {
            _identity = identity;
            _behaviourFactory = actor.Behaviour;
            _state = initState ?? actor.InitialState();
            _messageQueue = new ConcurrentQueue<(object Message, IActorIdentity? Sender)>();
            _logger = logger;
        }


        #endregion


        #region Public Contracts


        /// <inheritdoc />
        public IActorIdentity Identity => _identity;


        /// <summary>
        /// Receive user messages
        /// </summary>
        public void Send(object message, IActorIdentity? sender = null)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        public Task<TState> GetState()
        {
            throw new NotImplementedException();
        }


        #endregion


    }
}
