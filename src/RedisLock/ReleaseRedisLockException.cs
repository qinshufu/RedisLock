using System.Runtime.Serialization;

namespace RedisLock
{
    [Serializable]
    internal class ReleaseRedisLockException : Exception
    {

        public ReleaseRedisLockException(RedisLock redisLock) : base(redisLock.ToString())
        {
        }

        public ReleaseRedisLockException(string? message) : base(message)
        {
        }

        public ReleaseRedisLockException(RedisSemaphore redisSemaphore) : base(redisSemaphore.ToString())
        {
        }

        public ReleaseRedisLockException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReleaseRedisLockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
