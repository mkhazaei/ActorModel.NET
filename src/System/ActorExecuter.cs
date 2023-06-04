﻿using System;
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
    public class ActorExecuter<TState> : IActorExecuter, IDisposable, IThreadPoolWorkItem
        where TState : class, IEquatable<TState>
    {

        #region Private Fields
        private static readonly int _batchExecuteSize = 100;

        private readonly ConcurrentQueue<(object Message, IActorIdentity? Sender)> _messageQueue; // MailBox
        private readonly IActorIdentity _identity;
        private readonly Func<IActorBehavior<TState>> _behaviourFactory; // Instanse of 
        private readonly ILogger _logger;

        private TState _state; // Isolated State
        private int _processStatus;
        private bool _dirty; // Indicate that state of Actor changed or not.

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
            _processStatus = ActorStatus.Idle;
            _messageQueue = new ConcurrentQueue<(object Message, IActorIdentity? Sender)>();
            _dirty = true; // Need to persist!
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
            if (_processStatus == ActorStatus.Stopped) throw new ActorAlreadyStoppedException();
            _messageQueue.Enqueue(new (message, sender));
            ProcessQueues();
        }


        /// <summary>
        /// 
        /// </summary>
        public Task<TState> GetState()
        {
            if (_processStatus == ActorStatus.Stopped) throw new ActorAlreadyStoppedException();
            return Task.FromResult(_state); // FIXME: Unsafe
        }


        /// <summary>
        /// 
        /// </summary>
        public bool TrySleep(bool force)
        {
            if (_processStatus is not (ActorStatus.Idle or ActorStatus.Stopped) || _dirty || _messageQueue.Any())
                return false;

            _processStatus = ActorStatus.Stopped;
            return true;
        }

        /// <summary>
        /// IDisposable.Dispose
        /// </summary>
        public void Dispose()
        {
            _processStatus = ActorStatus.Stopped;
        }

        #endregion

        #region IThreadPoolWorkItem

        /// <summary>
        /// IThreadPoolWorkItem.Execute - Executed in new thread and handle Actor messages
        /// </summary>
        public void Execute()
        {
            bool anyModified = false;
            for (int i = 0; i < _batchExecuteSize; i++)
            {
                if (_processStatus == ActorStatus.Stopped) break;
                if (!_messageQueue.TryDequeue(out var envelop)) break;

                var newState = _behaviourFactory().Handle(new MessageEnvelop(envelop.Message, envelop.Sender), _state);
                var modified = !_state.Equals(newState);
                if (modified)
                {
                    _state = newState;
                    _dirty = _dirty || modified;
                    anyModified = anyModified || modified;
                }
            }

            // Continue?
            if (_processStatus == ActorStatus.Stopped)
            {
                return;
            }
            _processStatus = ActorStatus.Idle;
            if (_messageQueue.Any()) ProcessQueues();
        }

        #endregion

        #region Private Functions

        private void ProcessQueues()
        {
            // If _status == Idle => _status = Occupied; return Idle (init value)
            if (Interlocked.CompareExchange(ref _processStatus, ActorStatus.Occupied, ActorStatus.Idle) == ActorStatus.Idle)
            {
                ThreadPool.UnsafeQueueUserWorkItem(this, false);
            }
        }

        #endregion


    }
}
