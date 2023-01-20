using System.Diagnostics;
using StackExchange.Redis;

namespace RedisLock;

/// <summary>
/// Redis 分布式锁
/// </summary>
/// <example>
/// 可以这样使用
/// <code>
/// /// using (var theLock = RedisLock.Create(connection, key))
/// {
///     // 作用域结束后锁将自动释放
/// }
///
/// // 或者手动开始
/// var theLock = new RedisLock(connection, key);
/// await theLock.AquireAsync();
/// // 执行代码
/// await theLock.ReleaseAsync();
/// </code>
/// <example>
public class RedisLock : IDisposable
{
    public static readonly TimeSpan DefaultAquireTimeout = TimeSpan.FromSeconds(2);
    public static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(30);

    private readonly IDatabase _database;
    private readonly string _key;
    private readonly string _identity;
    private readonly TimeSpan _timeout;
    private bool _disposed;

    /// <summary>
    /// 创建分布式锁，但是没有获取
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="key"></param>
    public RedisLock(ConnectionMultiplexer connection, string key) : this(connection, 0, key, DefaultLockTimeout)
    { }

    /// <summary>
    /// 创建分布式锁，但是没有获取
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="key"></param>
    public RedisLock(ConnectionMultiplexer connection, int database, string key, TimeSpan timeout)
    {
        _database = connection.GetDatabase(database);
        _key = key;
        _identity = Guid.NewGuid().ToString();
        _timeout = timeout;
    }

    /// <summary>
    /// 创建并获取分布式锁
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static RedisLock Create(ConnectionMultiplexer connection, string key)
        => Create(connection, key, DefaultLockTimeout);

    /// <summary>
    /// 创建并获取分布式锁
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static RedisLock Create(ConnectionMultiplexer connection, string key, TimeSpan timeout)
        => Create(connection, 0, key, timeout);

    /// <summary>
    /// 创建并获取分布式锁
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static RedisLock Create(ConnectionMultiplexer connection, int database, string key, TimeSpan timeout)
    {
        var redislock = new RedisLock(connection, database, key, timeout);
        redislock.AquireAsync().Wait();
        return redislock;
    }

    public async Task AquireAsync(TimeSpan? timeout = null)
    {
        timeout ??= DefaultAquireTimeout;
        var stopWatch = Stopwatch.StartNew();
        var success = await _database.StringSetAsync(_key, _identity, _timeout, When.NotExists);
        while (success is false)
        {
            if (stopWatch.Elapsed > timeout)
            {
                stopWatch.Stop();
                throw new AquireRedisLockTimeOutException(this);
            }

            await Task.Delay(Random.Shared.Next(10, 50));
            success = await _database.StringSetAsync(_key, _identity, _timeout, When.NotExists);
        }

        stopWatch.Stop();
    }

    public async Task ReleaseAsync()
    {
        var tran = _database.CreateTransaction();
        tran.AddCondition(Condition.StringEqual(_key, _identity));
        await tran.KeyDeleteAsync(_key);
        var success = await tran.ExecuteAsync();
        _disposed = true;

        if (success is false)
            throw new ReleaseRedisLockException(this);

    }

    public void Dispose()
    {
        if (_disposed is true)
            return;

        ReleaseAsync().Wait();
    }

    public override bool Equals(object? obj)
    {
        return obj is RedisLock @lock &&
               EqualityComparer<IDatabase>.Default.Equals(_database, @lock._database) &&
               _key == @lock._key &&
               _identity == @lock._identity &&
               _timeout.Equals(@lock._timeout) &&
               _disposed == @lock._disposed;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_database, _key, _identity, _timeout, _disposed);
    }

    public override string? ToString()
    {
        return $"RedisLock(key: {_key}, timeout: {_timeout})";
    }
}
