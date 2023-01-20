using System.Runtime.Serialization;

namespace RedisLock
{
    [Serializable]
    internal class AquireRedisLockTimeOutException : Exception
    {
        private RedisLock redisLock;

        public AquireRedisLockTimeOutException()
        {
        }

        public AquireRedisLockTimeOutException(RedisLock redisLock) : base(redisLock.ToString())
        {
            this.redisLock = redisLock;
        }

        public AquireRedisLockTimeOutException(string? message) : base(message)
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
