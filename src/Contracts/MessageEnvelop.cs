using System;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Message Envelop to keep message and sender
    /// </summary>
    public class MessageEnvelop
    {

        /// <summary>
        /// 
        /// </summary>
        public MessageEnvelop(object message, IActorIdentity? sender)
        {
            Message = message;
            Sender = sender;
        }

        /// <summary>
        /// 
        /// </summary>
        public object Message { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public IActorIdentity? Sender {  get; init; }


    }
}
