using System;
using System.Reflection;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Message Envelop to keep message and sender
    /// </summary>
    public class MessageEnvelop
    {
        private readonly IActorSystem _actorSystem;
        private readonly ActorIdentityAndType? _sender;
        private readonly ActorIdentityAndType _current;

        /// <summary>
        /// 
        /// </summary>
        public MessageEnvelop(object message, ActorIdentityAndType? sender, ActorIdentityAndType reciever, IActorSystem actorSystem)
        {
            Message = message;
            _sender = sender;
            _current = reciever;
            _actorSystem = actorSystem;
        }

        /// <summary>
        /// 
        /// </summary>
        public object Message { get; init; }

        

        /// <summary>
        /// 
        /// </summary>
        public bool Respond(object message)
        {
            if (_sender == null)
                return false;
            _actorSystem.Send(_sender, message, _current);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Send<TActor>(ActorIdentityAndType receiver, object message)
        {
            _actorSystem.Send(receiver, message, _current);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Send<TActor>(IActorIdentity receiverId, object message)
        {
            _actorSystem.Send(new ActorIdentityAndType(receiverId, typeof(TActor)), message, _current);
        }
    }
}
