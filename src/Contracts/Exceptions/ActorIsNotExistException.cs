using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.Contracts.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ActorIsNotExistException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public IActorIdentity Identity { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public ActorIsNotExistException(IActorIdentity identity)
        {
            Identity = identity;
        }
    }
}
