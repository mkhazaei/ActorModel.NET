using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// 
    /// </summary>
    public class ActorSystemConfiguration
    {
        /// <summary>
        /// Bach Size of Actor Processing Message
        /// </summary>
        public int BatchExecutionSize { get; set; } = 100;

        /// <summary>
        /// Actor Auto Persistence Timeout
        /// </summary>
        public int PersistenceTimeoutInSecond { get; set; } = 60;

    }
}
