using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Actor Identity and Type
    /// </summary>
    public record ActorIdentityAndType(IActorIdentity Identity, Type Type);
}
