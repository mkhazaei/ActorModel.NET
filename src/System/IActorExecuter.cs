using ActorModelNet.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.System
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IActorExecuter : IDisposable
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
        void Send(object message, ActorIdentityAndType? sender = null);

        /// <summary>
        /// 
        /// </summary>
        void SendSysMsg(ISystemMessage message);

        /// <summary>
        /// Request Actor to stop
        /// </summary>
        public bool TrySleep(bool force);

    }
}
