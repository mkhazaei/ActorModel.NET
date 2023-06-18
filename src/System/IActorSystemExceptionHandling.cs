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
    internal interface IActorSystemExceptionHandling
    {

        /// <summary>
        /// tell actor system there is exception in a actor
        /// </summary>
        void Exception(IActorIdentity identity, Exception exception);
    }
}
