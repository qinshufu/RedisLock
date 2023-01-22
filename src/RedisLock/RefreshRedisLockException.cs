using System.Runtime.Serialization;

namespace RedisLock
{
    [Serializable]
    internal class RefreshRedisLockException : Exception
    {

        public RefreshRedisLockException()
        {
        }

        public RefreshRedisLockException(RedisLock redisLock) : this(redisLock.ToString())
        {
        }

        public RefreshRedisLockException(string? message) : base(message)
        {
        }

        public RefreshRedisLockException(RedisSemaphore redisSemaphore) : base(redisSemaphore.ToString())
        {
        }

        public RefreshRedisLockException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected RefreshRedisLockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
