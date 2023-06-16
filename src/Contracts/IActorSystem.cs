using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
