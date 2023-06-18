using System;

namespace ActorModelNet.Contracts
{
    /// <summary>
    /// Base Identity of Actor
    /// </summary>
    public interface IActorIdentity : IEquatable<IActorIdentity>
    {
        /// <summary>
        /// Get string representation of Identity of Actor
        /// </summary>
        byte[] ToByeArray();

    }
}
