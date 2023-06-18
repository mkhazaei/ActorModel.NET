using ActorModelNet.Contracts;
using System;

namespace ActorModelNet.System
{
    /// <summary>
    /// Long Account Identity
    /// </summary>
    public record GuidActorIdentity : IActorIdentity
    {
        /// <summary>
        /// 
        /// </summary>
        public GuidActorIdentity(Guid identity)
        {
            Identity = identity;
        }

        /// <summary>
        /// 
        /// </summary>
        public Guid Identity { get; }




        bool IEquatable<IActorIdentity>.Equals(IActorIdentity? other)
        {
            if (other == null) return false;
            var castedOther = other as GuidActorIdentity;
            if (castedOther == null) return false;
            return castedOther.Identity == Identity;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode() => Identity.GetHashCode();

        /// <summary>
        /// 
        /// </summary>
        public byte[] ToByeArray() => Identity.ToByteArray();

        /// <summary>
        /// 
        /// </summary>
        public override string ToString() => Identity.ToString("N");
    }
}
