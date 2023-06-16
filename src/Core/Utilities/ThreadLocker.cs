using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorModelNet.Core.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class ThreadLocker
    {
        const int DEFAULT_LOCKERS_COUNTER = 997;
        int _lockersCount;
        object[] _lockers;

        /// <summary>
        /// 
        /// </summary>
        public ThreadLocker(int maxLockersCount)
        {
            if (maxLockersCount < 1) throw new ArgumentOutOfRangeException("maxLockersCount", maxLockersCount, "Counter cannot be less, that 1");
            _lockersCount = maxLockersCount;
            _lockers = Enumerable.Range(0, _lockersCount).Select(_ => new object()).ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        public ThreadLocker() : this(DEFAULT_LOCKERS_COUNTER) { }

        /// <summary>
        /// 
        /// </summary>
        private object GetLockerByHash(int hashCode)
        {
            var idx = (hashCode % _lockersCount + _lockersCount) % _lockersCount; // Hashcode can be negative
            return _lockers[idx];
        }

        /// <summary>
        /// 
        /// </summary>
        public object GetLocker(string key)
        {
            var hash = key.GetHashCode();
            return GetLockerByHash(hash);
        }

        /// <summary>
        /// 
        /// </summary>
        public object GetLocker(Guid key)
        {
            var hash = key.GetHashCode();
            return GetLockerByHash(hash);
        }

        /// <summary>
        /// 
        /// </summary>
        public object GetLocker(object key)
        {
            var hash = key.GetHashCode();
            return GetLockerByHash(hash);
        }
    }
}
