using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace RedisSubscriber;

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

        Console.WriteLine("Hello, Redis Subscriber!");

        var redisConnectionString = GetRedisConnectionString();

        redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);

        var sub = redis.GetSubscriber();
        await sub.SubscribeAsync("SimpleChannel", handler: async (channel, value) => { await HandleMessage(value); });
        Console.ReadLine();
    }

    private static async Task HandleMessage(RedisValue value)
    {
        Console.WriteLine("New Message");
        Console.WriteLine(value.ToString());
        Console.WriteLine("--");
    }
}