namespace ActorModelNet.System
{
    /// <summary>
    /// Actor Life-Cycle
    /// </summary>
    public static class ActorStatus
    {
        /// <summary>
        /// Idle
        /// </summary>
        public const int Idle = 1;

        /// <summary>
        /// Actor is Processing on Background Thread
        /// </summary>
        public const int Occupied = 2;

        /// <summary>
        /// Actor is Stopping
        /// </summary>
        public const int Stopped = 3;

    }
}
