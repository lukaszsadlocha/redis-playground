using System.ComponentModel;
using System.Threading.Channels;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisPlaygorund;

internal class Program
{
    private static ConnectionMultiplexer redis;

    internal static string GetRedisConnectionString()
    {
        var configurationBuilder = new ConfigurationBuilder();
        IConfiguration configuration = configurationBuilder.AddUserSecrets<Program>().Build();
        return configuration.GetConnectionString("Redis");
    }

    public static async Task Main(string[] args)
    {
        // Connection to free Redis created within free Subscription (logged with Google Account) on https://app.redislabs.com/#/
        // Below there are few examples of 
        //
        // Remember to add Redis ConnectionString to UserSecrets!

        Console.WriteLine("Hello, Redis Playground!");

        var redisConnectionString = GetRedisConnectionString();

        redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);

        //To keep different keys
        var ticks = DateTime.Now.Ticks;

        var db = redis.GetDatabase();

        //Adding Value:
        await db.StringSetAsync(new RedisKey("stringKey_" + ticks), "Hello Redis");

        try
        {
            //Adding Value that exists:
            await db.StringSetAsync(new RedisKey("stringKey_" + ticks), "Hello Redis - overwrite");
        }
        catch (Exception e)
        {
            // It will not be an exception thrown
            // Value will be overwrite in Redis
            Console.WriteLine(e);
        }

        // Transaction that will add 3 values
        var transaction = db.CreateTransaction();

        _ = transaction.StringSetAsync(new RedisKey("Transaction_StringKey_1_" + ticks), "Transaction Value 1");
        _ = transaction.StringSetAsync(new RedisKey("Transaction_StringKey_2_" + ticks), "Transaction Value 2");
        _ = transaction.StringSetAsync(new RedisKey("Transaction_StringKey_3_" + ticks), "Transaction Value 3");

        await transaction.ExecuteAsync();

        await db.PublishAsync(new RedisChannel("SimpleChannel", RedisChannel.PatternMode.Literal), "Please be nice");

        while (true)
        {
            Console.WriteLine("Type new message to send. Type 'exit' to leave");
            var input = Console.ReadLine();
            if (input == "exit")
            {
                return;
            }

            await db.PublishAsync(new RedisChannel("SimpleChannel", RedisChannel.PatternMode.Literal), input);
        }
    }
}