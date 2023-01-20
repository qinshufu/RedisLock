using System.Runtime.Serialization;

namespace RedisLock
{
    [Serializable]
    internal class AquireRedisLockTimeOutException : Exception
    {

        public AquireRedisLockTimeOutException(RedisLock redisLock) : base(redisLock.ToString())
        {
        }

        public AquireRedisLockTimeOutException(string? message) : base(message)
        {
        }

        public AquireRedisLockTimeOutException(RedisSemaphore redisSemaphore) : base(redisSemaphore.ToString())
        {
        }

        public AquireRedisLockTimeOutException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AquireRedisLockTimeOutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
