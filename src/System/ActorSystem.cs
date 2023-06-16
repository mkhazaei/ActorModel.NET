using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ActorModelNet.Contracts;
using ActorModelNet.Contracts.Messages;
using ActorModelNet.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ActorModelNet.System
{
    /// <summary>
    /// A Type-Safe Simple Actor System.
    /// This Actor System run on a single Host and need to know about Actor Properties.
    /// </summary>
    public class ActorSystem : IActorSystem, IDisposable
    {

        private readonly Dictionary<IActorIdentity, IActorExecuter> _actorsExecuters;
        private readonly ThreadLocker _threadLocker;
        private readonly Dictionary<Guid, Timer> _timers;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, IActor> _actorDefinitions;
        private readonly IPersistence? _persistence;
        #region ctor

        /// <summary>
        /// 
        /// </summary>
        public ActorSystem(ILogger<ActorSystem> logger, IServiceProvider serviceProvider, IPersistence? persistence = null)
        {
            _logger = logger;
            _actorsExecuters = new Dictionary<IActorIdentity, IActorExecuter>();
            _threadLocker = new ThreadLocker();
            _timers = new Dictionary<Guid, Timer>();
            _serviceProvider = serviceProvider;
            _actorDefinitions = new Dictionary<Type, IActor>();
            _persistence = persistence;
        }

        #endregion

        /// <summary>
        /// Register Actor Definition (Initializer, Behaviour)
        /// </summary>
        /// <param name="actor"></param>
        public void Register(IActor actor)
        {
            _actorDefinitions.Add(actor.GetType(), actor);
        }

        /// <summary>
        /// Register Actor Definition (Initializer, Behaviour)
        /// </summary>
        public void Register<TActor>()
            where TActor : IActor, new()
        {
            var actorType = typeof(TActor);
            _actorDefinitions.Add(actorType, (IActor)(Activator.CreateInstance(actorType) ?? throw new ArgumentException("Given TActor is not valid. It should Inherited IActor")));
        }

        /// <summary>
        /// 
        /// </summary>
        public void Send<TActor, TState>(IActorIdentity identity, object message, IActorIdentity? sender = null)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>
        {
            var actor = GetOrSpawn<TActor, TState>(identity);
            actor.Send(message, sender);
        }

        /// <summary>
        /// Get Actor Reference
        /// </summary>
        public Task<TState> UnsafeGetState<TActor, TState>(IActorIdentity identity)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>
        {
            var actor = GetOrSpawn<TActor, TState>(identity);
            var theActor = actor as ActorExecuter<TState> ?? throw new ArgumentException($"Type argument TState: {nameof(TState)} is not valid");
            return theActor.UnsafeGetState();
        }


        /// <summary>
        /// Get Actor State
        /// </summary>
        public Task<TResult> GetState<TActor, TState, TResult>(IActorIdentity identity, Func<TState, TResult> selector)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>
        {
            var actor = GetOrSpawn<TActor, TState>(identity);
            var theActor = actor as ActorExecuter<TState> ?? throw new ArgumentException($"Type argument TState: {nameof(TState)} is not valid");
            return theActor.GetState(selector);
        }

        /// <summary>
        /// Spawn (Create) Actor by initial state
        /// </summary>
        public ActorReference<TActor, TState> Spawn<TActor, TState>(IActorIdentity identity, TState? initState = null)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>
        {
            _ = GetOrSpawn<TActor, TState>(identity, initState);
            return new ActorReference<TActor, TState>(this, identity);
        }

        /// <summary>
        /// Get Actor Reference
        /// </summary>
        public ActorReference<TActor, TState> Get<TActor, TState>(IActorIdentity identity)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>
        {
            return new ActorReference<TActor, TState>(this, identity);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _logger.LogDebug("Disposing...");
            foreach (var item in _actorsExecuters)
            {
                item.Value.Dispose();
            }
        }


        #region Private Functions


        internal void Exception(IActorIdentity identity, Exception exception)
        {
            _logger.LogError($"Actor/{identity.ToString()}: Exception", exception);
            Sleep(identity, force: true);
        }



        private IActorExecuter GetOrSpawn<TActor, TState>(IActorIdentity identity, TState? initState = null)
            where TActor : IActor<TState>
            where TState : class, IEquatable<TState>
        {
            lock (_threadLocker.GetLocker(identity))
            {
                if (!_actorsExecuters.TryGetValue(identity, out var actorExecuter))
                {
                    actorExecuter = AddActor<TActor, TState>(identity, initState);
                    actorExecuter.Send(new ActorStartedMessage());
                }
                return actorExecuter;
            }
        }

        private void Sleep(IActorIdentity identity, bool force = false)
        {
            lock (_threadLocker.GetLocker(identity))
            {
                if (!_actorsExecuters.TryGetValue(identity, out var actorExecuter)) return;
                if (!actorExecuter.TrySleep(force)) return;
                _actorsExecuters.Remove(identity);
                actorExecuter.Dispose();
                _logger.LogDebug($"Sleep : Actor/{identity.ToString()} slept");
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IActorExecuter AddActor<TActor, TState>(IActorIdentity identity, TState? initialState = null)
            where TState : class, IEquatable<TState>
        {
            if (!_actorDefinitions.TryGetValue(typeof(TActor), out var actor))
                throw new ArgumentException("actorType is not valid");
            var actorExecuter = new ActorExecuter<TState>(identity, (IActor<TState>)actor, _logger, initialState);
            _actorsExecuters.Add(identity, actorExecuter);
            return actorExecuter;
        }


        #endregion



    }
}
