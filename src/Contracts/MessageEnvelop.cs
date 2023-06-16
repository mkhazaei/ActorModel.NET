using System;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Message Envelop to keep message and sender
    /// </summary>
    public class MessageEnvelop
    {
        private readonly IActorIdentity _identity;
        private readonly IActorSystem _actorSystem;

        /// <summary>
        /// 
        /// </summary>
        public MessageEnvelop(object message, IActorIdentity? sender, IActorIdentity identity, IActorSystem actorSystem)
        {
            Message = message;
            Sender = sender;
            _identity = identity;
            _actorSystem = actorSystem;
        }

        /// <summary>
        /// 
        /// </summary>
        public object Message { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public IActorIdentity? Sender {  get; init; }

        /// <summary>
        /// 
        /// </summary>
        public void Send<TActor, TState>(IActorIdentity identity, object message)
            where TActor : class, IActor<TState>
            where TState : class, IEquatable<TState>
        {
            _actorSystem.Send<TActor, TState>(identity, message, _identity);
        }
    }
}
