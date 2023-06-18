using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActorModelNet.Contracts;

namespace ActorModelNet.System
{
    /// <summary>
    /// 
    /// </summary>
    internal record ActorScheduleState(Guid TimerId, IActorIdentity ActorIdentity, object Message, Type ActorType);
}
