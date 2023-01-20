using System.Runtime.Serialization;

namespace RedisLock
{
    [Serializable]
    internal class ReleaseRedisLockException : Exception
    {
        private RedisLock redisLock;

        public ReleaseRedisLockException()
        {
        }

        public ReleaseRedisLockException(RedisLock redisLock) : base(redisLock.ToString())
        {
            this.redisLock = redisLock;
        }

        public ReleaseRedisLockException(string? message) : base(message)
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
