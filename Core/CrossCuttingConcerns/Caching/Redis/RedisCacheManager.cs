using System;
using Core.Utilities.Results;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Core.CrossCuttingConcerns.Caching.Redis
{
    public class RedisCacheManager : ICacheManager
    {
        IDatabase _database;

        public RedisCacheManager()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            _database = redis.GetDatabase();
        }


        public object Get(string key)
        {
            return _database.StringGet(key);
        }

        public void Set(string key, object returnValue, int duration)
        {
            _database.StringSet(key, JsonConvert.SerializeObject(returnValue) , TimeSpan.FromHours(duration));
        }


        public bool IsExists(string key)
        {
            return _database.KeyExists(key);
        }

        public void Remove(string key)
        {
            _database.KeyDelete(key);
        }
    }
}