using System;
using Testcontainers.RabbitMq;

namespace MarketSpace.TestFixtures;

public static class RabbitMqTestcontainerFixture
{
    private static RabbitMqContainer? _container;
    private static bool _started;
    private static bool _dockerAvailable = true;

    public static void Start()
    {
        if (_started) return;

        try
        {
            _container ??= new RabbitMqBuilder("rabbitmq:3-management")
                .WithName($"rabbitmq-test-{Guid.NewGuid():N}")
                .WithUsername("guest")
                .WithPassword("guest")
                .WithPortBinding(5672, true)
                .WithPortBinding(15672, true)
                .Build();

            _container.StartAsync().GetAwaiter().GetResult();
            _started = true;
        }
        catch (Exception)
        {
            _dockerAvailable = false;
            _started = false;
        }
    }

    public static bool IsAvailable => _started && _dockerAvailable;

    public static string GetConnectionString()
    {
        if (!_started) Start();
        if (!_started || !_dockerAvailable)
            throw new InvalidOperationException("RabbitMQ test container is not available.");

        var host = _container!.Hostname;
        var port = _container.GetMappedPublicPort(5672);
        return $"amqp://guest:guest@{host}:{port}";
    }

    public static void Stop()
    {
        if (!_started || _container == null) return;
        _container.StopAsync().GetAwaiter().GetResult();
        _started = false;
    }
}