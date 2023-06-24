using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActorModelNet.Contracts;
using ActorModelNet.Contracts.Exceptions;
using ActorModelNet.Contracts.Messages;
using Microsoft.Extensions.Logging;

namespace ActorModelNet.System
{
    /// <summary>
    /// Simple Actor model based
    /// There is no parent-child hierarchies and Clustering configuration.
    /// Actors will be deactivated and stored in DB and restored on demands.
    /// </summary>
    internal class ActorExecuter<TState> : IActorExecuter, IDisposable, IThreadPoolWorkItem
        where TState : class, IEquatable<TState>
    {

        #region Private Fields

        private readonly ConcurrentQueue<(object Message, ActorIdentityAndType? Sender)> _messageQueue; // MailBox
        private readonly IActorSystem _actorSystem;
        private readonly IActorSystemExecuterContract _actorSystemExecuterContract;
        private readonly IActorIdentity _identity;
        private readonly Type _actorType;
        private readonly Func<IActorBehavior<TState>> _behaviourFactory; // Instanse of 
        private readonly IPersistence? _persistence;
        private readonly ILogger _logger;
        private readonly ActorSystemConfiguration _configuration;

        private TState _state; // Isolated State
        private int _processStatus;
        private bool _dirty; // Indicate that state of Actor changed or not.
        private Guid? _persistenceTimer;

        private readonly Timer _sleepTimer;
        private DateTime _lastRecievedMessage;

        #endregion

        #region ctor & Factory

        /// <summary>
        /// Ctor
        /// </summary>
        public ActorExecuter(IActorIdentity identity,
            IActor<TState> actor,
            IActorSystem actorSystem,
            IActorSystemExecuterContract actorSystemExecuterContract,
            ILogger logger,
            ActorSystemConfiguration configuration,
            TState? initState = null,
            IPersistence? persistence = null)
        {
            _identity = identity;
            _actorType = actor.GetType();
            _behaviourFactory = actor.Behaviour;
            _state = initState ?? actor.InitialState();
            _actorSystem = actorSystem;
            _actorSystemExecuterContract = actorSystemExecuterContract;
            _processStatus = ActorStatus.Idle;
            _messageQueue = new ConcurrentQueue<(object Message, ActorIdentityAndType? Sender)>();
            _persistence = persistence;
            _dirty = true; // Need to persist!
            _persistenceTimer = null;
            _logger = logger;
            _configuration = configuration;

            _lastRecievedMessage = DateTime.UtcNow;
            _sleepTimer = new Timer(_ => _actorSystemExecuterContract.Sleep(_identity, false), null, TimeSpan.FromSeconds(_configuration.SleepTimeoutInSecond), Timeout.InfiniteTimeSpan);

        }


        #endregion


        #region Public Contracts


        /// <inheritdoc />
        public IActorIdentity Identity => _identity;

        /// <summary>
        /// Receive system messages
        /// </summary>
        public void SendSysMsg(ISystemMessage message)
        {
            if (_processStatus == ActorStatus.Stopped) throw new ActorAlreadyStoppedException();
            ResetSleepTimer();
            _messageQueue.Enqueue(new (message, null));
            ProcessQueues();
        }

        /// <summary>
        /// Receive user messages
        /// </summary>
        public void Send(object message, ActorIdentityAndType? sender = null)
        {
            if (_processStatus == ActorStatus.Stopped) throw new ActorAlreadyStoppedException();
            ResetSleepTimer();
            _messageQueue.Enqueue(new (message, sender));
            ProcessQueues();
        }


        /// <summary>
        /// Unsafe because of returning Sate object
        /// </summary>
        public Task<TState> UnsafeGetState()
        {
            if (_processStatus == ActorStatus.Stopped) throw new ActorAlreadyStoppedException();
            if (_processStatus == ActorStatus.Idle)
                return Task.FromResult(_state);
            var task = new TaskCompletionSource<TState>();
            SendSysMsg(new GetStateSystemMessage<TState>(task));
            return task.Task;
        }


        /// <summary>
        /// 
        /// </summary>
        public async Task<TResult> GetState<TResult>(Func<TState, TResult> selector)
        {
            if (_processStatus == ActorStatus.Stopped) throw new ActorAlreadyStoppedException();
            ResetSleepTimer();
            TResult result;
            if (_processStatus == ActorStatus.Idle)
            {
                result = selector(_state);
            }
            else
            {
                var task = new TaskCompletionSource<TState>();
                SendSysMsg(new GetStateSystemMessage<TState>(task));
                result = selector(await task.Task);
            }
            if (ReferenceEquals(_state, result)) throw new Exception("Selector Function must return new instance of state");
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        public bool TrySleep(bool force)
        {
            _logger.LogDebug($"Actor/{_identity.ToString()}: TrySleep()");
            // Check if there is new mesage between timeout and current time
            if (!force && DateTime.UtcNow.Subtract(_lastRecievedMessage) < TimeSpan.FromSeconds(_configuration.SleepTimeoutInSecond - _configuration.SleepTimeoutSlidingWindowsInSecond))
            {
                _sleepTimer.Change(TimeSpan.FromSeconds(_configuration.SleepTimeoutInSecond), Timeout.InfiniteTimeSpan);
                return false;
            }

            if (_processStatus is not (ActorStatus.Idle or ActorStatus.Stopped) || _dirty || _messageQueue.Any())
                return false;

            _logger.LogDebug($"Actor/{_identity.ToString()}: Sleeping...");
            _processStatus = ActorStatus.Stopped;
            return true;
        }

        /// <summary>
        /// IDisposable.Dispose
        /// </summary>
        public void Dispose()
        {
            _processStatus = ActorStatus.Stopped;
            _sleepTimer.Change(Timeout.Infinite, Timeout.Infinite);
            Persist();
            _sleepTimer.Dispose();
        }

        #endregion

        #region IThreadPoolWorkItem

        /// <summary>
        /// IThreadPoolWorkItem.Execute - Executed in new thread and handle Actor messages
        /// </summary>
        public void Execute()
        {
            bool anyModified = false;
            for (int i = 0; i < _configuration.BatchExecutionSize; i++)
            {
                if (_processStatus == ActorStatus.Stopped) break;
                if (!_messageQueue.TryDequeue(out var envelop)) break;
                if (envelop.Message is ISystemMessage sysMessage)
                {
                    HandleSystemMessage(sysMessage);
                }
                else
                {
                    var newState = _behaviourFactory().Handle(new MessageEnvelop(envelop.Message, envelop.Sender, new ActorIdentityAndType(_identity, _actorType), _actorSystem), _state);
                    var modified = !_state.Equals(newState);
                    if (modified)
                    {
                        _state = newState;
                        _dirty = _dirty || modified;
                        anyModified = anyModified || modified;
                    }
                }
            }

            if (_dirty && anyModified)
            {
                TimeoutPersist();
            }

            // Continue?
            if (_processStatus == ActorStatus.Stopped)
            {
                Persist();
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

        private void HandleSystemMessage(ISystemMessage message)
        {
            switch (message)
            {
                case RestoreSystemMessage _:
                    Restore();
                    break;
                case StoreSystemMessage _:
                    Persist();
                    break;
                case GetStateSystemMessage<TState> getStateSystemMessage:
                    getStateSystemMessage.TaskCompletionSource.SetResult(_state);
                    break;
            }
        }

        private void TimeoutPersist()
        {
            if (_persistence == null || _persistenceTimer != null) return;
            _logger.LogDebug($"Actor/{_identity.ToString()}: Execute.TimeoutPersist Set Schedule");
            _persistenceTimer = Guid.NewGuid();
            _actorSystem.ScheduledSend(_identity, _actorType, new StoreSystemMessage(), TimeSpan.FromSeconds(_configuration.PersistenceTimeoutInSecond), _persistenceTimer);
        }

        private void Persist()
        {
            if (!_dirty || _persistence == null) return;
            _logger.LogDebug($"Actor/{_identity.ToString()}: Persist()");
            try
            {
                if (_persistence.Save(_identity, _state))
                {
                    _dirty = false;
                    _persistenceTimer = null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Actor/{_identity.ToString()}: Exception on Persist", e);
            }

        }

        private void Restore()
        {
            if (_persistence == null) return;
            _logger.LogDebug($"Actor/{_identity.ToString()}: Restore()");
            try
            {
                var state = _persistence.Load<TState>(_identity);
                _state = state;
                _dirty = false;
            }
            catch (Exception e)
            {
                InternalError(new Exception("Error of Restoring Actor", e));
                return;
            }
        }

        private void InternalError(Exception exception)
        {
            _processStatus = ActorStatus.Stopped;
            foreach (var envelop in _messageQueue)
            {
                if (envelop.Message is GetStateSystemMessage<TState> getState)
                    getState.TaskCompletionSource.SetException(exception);
            }
            _messageQueue.Clear();
            _dirty = false;
            _actorSystemExecuterContract.Exception(_identity, exception);
        }

        private void ResetSleepTimer()
        {
            var utc = DateTime.UtcNow;
            if (utc.Subtract(_lastRecievedMessage) > TimeSpan.FromSeconds(_configuration.SleepTimeoutSlidingWindowsInSecond))
            {
                _sleepTimer.Change(TimeSpan.FromSeconds(_configuration.SleepTimeoutInSecond), Timeout.InfiniteTimeSpan);
            }
            _lastRecievedMessage = utc;
        }
        #endregion


    }
}
