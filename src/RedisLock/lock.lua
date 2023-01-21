#!lua name=lock

local function aquire_lock(lock_name, lock_value, life_time)
    return redis.call('set', lock_name, lock_value, 'nx', 'ex', life_time)
end

local function release_lock(lock_name, lock_value)
    if redis.call('get', lock_name) == lock_value then
        redis.call('del', lock_name)
        return true
    end

    return false
end

local function wait_semaphore(sem_name, identity, utc_timestamp, size, life_time)
    local timeout = utc_timestamp - life_time
    redis.call('zremrangebyscore', sem_name, 0, timeout)
    redis.call('zremrangebyrank', sem_name, size, -1)
    redis.call('zadd', sem_name, utc_timestamp, identity)
    return redis.call('zscore', sem_name, identity);
end

local function release_semaphore(sem_name, identity)
    return redis.call('zrem', sem_name, identity)
end

local function cmd_wait_semaphore(keys, args)
    return wait_semaphore(keys[1], args[1], tonumber(args[2]), tonumber(args[3]), tonumber(args[4]))
end

local function cmd_release_semaphore(keys, args)
    return release_semaphore(keys[1], args[1])
end

local function cmd_aquire_lock(keys, args)
    return aquire_lock(keys[1], args[1], args[2])
end

local function cmd_release_lock(keys, args)
    return release_lock(keys[1], args[1])
end



redis.register_function('aquire_lock', cmd_aquire_lock)
redis.register_function('release_lock', cmd_release_lock)
redis.register_function('await_semaphore', cmd_wait_semaphore)
redis.register_function('release_semaphore', cmd_release_semaphore)
