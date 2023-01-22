using System.Diagnostics;
using StackExchange.Redis;

namespace RedisLock;

/// <summary>
/// Redis 实现的信号量，带有超时自动释放的机制
/// </summary>
public class RedisSemaphore
{
    private static readonly TimeSpan DefaultAquireTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private static readonly DateTime UnixTimestampStart = new DateTime(1970, 1, 1);

    private readonly IDatabase _database;
    private readonly string _key;
    private readonly TimeSpan _timeout;
    private readonly string _identity;
    private readonly int _size;

    public RedisSemaphore(ConnectionMultiplexer connection, string key, int size) : this(connection, 0, key, size, DefaultTimeout)
    { }

    public RedisSemaphore(ConnectionMultiplexer connection, int database, string key, int size, TimeSpan timeout)
    {
        _database = connection.GetDatabase(database);
        _key = key;
        _timeout = timeout;
        _identity = Guid.NewGuid().ToString();
        _size = size;
    }

    public async Task AquireAsync(TimeSpan? timeout = null)
    {
        timeout ??= DefaultAquireTimeout;
        var score = GetUtcTimestamp();
        var scoreOfTimeout = score - _timeout.TotalMilliseconds;
        var stopWatch = Stopwatch.StartNew();
        var success = await DoAquireAsync(score, scoreOfTimeout);
        while (success is false)
        {
            await Task.Delay(Random.Shared.Next(10, 50));
            if (stopWatch.Elapsed > timeout)
            {
                stopWatch.Stop();
                throw new AquireRedisLockTimeOutException(this);
            }

            success = await DoAquireAsync(score, scoreOfTimeout);
        }
    }

    private static double GetUtcTimestamp() => (DateTime.UtcNow - UnixTimestampStart).TotalMilliseconds;

    public async Task ReleaseAsync()
    {
        var success = await _database.SortedSetRemoveAsync(_key, _identity);
        if (success is false)
            throw new ReleaseRedisLockException(this);
    }

    public async Task RefreshAsync()
    {
        var tran = _database.CreateTransaction();
#pragma warning disable CS4014
        tran.SortedSetRemoveRangeByScoreAsync(_key, 0, GetUtcTimestamp() - _timeout.TotalMilliseconds, Exclude.None); // 删除超时的
        tran.SortedSetRemoveRangeByRankAsync(_key, _size, -1); // 清理无效的
#pragma warning restore CS4014
        tran.AddCondition(Condition.SortedSetContains(_key, _identity));
        var success = await tran.ExecuteAsync();

        if (success is false)
            throw new RefreshRedisLockException(this);
    }

    private async Task<bool> DoAquireAsync(double score, double scoreOfTimeout)
    {
        var tran = _database.CreateTransaction();
#pragma warning disable CS4014
        tran.SortedSetAddAsync(_key, _identity, score); // 生存时间不应该设置的太小，不然可能在通信过程中就已经超时，然后被清理掉
        tran.SortedSetRemoveRangeByScoreAsync(_key, 0, scoreOfTimeout, Exclude.None); // 删除超时的
        tran.SortedSetRemoveRangeByRankAsync(_key, _size, -1); // 清理无效的
#pragma warning restore CS4014
        var existingTask = tran.SortedSetScoreAsync(_key, _identity);
        await tran.ExecuteAsync();

        return (await existingTask) is not null;
    }

}
