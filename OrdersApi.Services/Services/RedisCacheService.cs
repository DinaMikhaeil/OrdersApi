using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace OrdersApi.Services.Services
{
    public class RedisCacheService : ICacheService, IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConfiguration config, ILogger<RedisCacheService> logger)
        {
            _logger = logger;

            
            var redisConfig = config["Redis:Configuration"] ?? "localhost:6380"; // البورت اللي شغال عليه
            _redis = ConnectionMultiplexer.Connect(redisConfig);
            _db = _redis.GetDatabase();

            Console.WriteLine($"[Redis] Connected to {redisConfig}");
        }

     
        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            try
            {
                var val = await _db.StringGetAsync(key);
                if (!val.HasValue)
                {
                    Console.WriteLine($"[Cache MISS] Key: {key}");
                    return default;
                }

                Console.WriteLine($"[Cache HIT] Key: {key}");
                return JsonSerializer.Deserialize<T>(val!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Get Error for key {Key}", key);
                return default;
            }
        }

        
        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _db.StringSetAsync(key, json, ttl);
                Console.WriteLine($"[Cache SET] Key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Set Error for key {Key}", key);
            }
        }

        
        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
                Console.WriteLine($"[Cache REMOVE] Key: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis Remove Error for key {Key}", key);
            }
        }

        
        public void Dispose()
        {
            _redis?.Dispose();
            Console.WriteLine("[Redis] Connection disposed");
        }
    
}
}
