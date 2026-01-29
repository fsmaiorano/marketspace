using RabbitMQ.Client;
using Xunit;
using FluentAssertions;
using Order.Test.Fixtures;

namespace Order.Test.Integration;

public class RabbitMqIntegrationTests
{
  [Fact]
  public void RabbitMqContainer_ShouldBeReachable_AndAllowPublishAndDeclareQueue()
  {
    // Ensure container is running
    RabbitMqTestcontainerFixture.Start();

    if (!RabbitMqTestcontainerFixture.IsAvailable)
      return; // Docker is not available here; skip test.

    string connectionString = RabbitMqTestcontainerFixture.GetConnectionString();

    var factory = new ConnectionFactory()
    {
      Uri = new Uri(connectionString)
    };

    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    string queueName = "test-queue";

    // Declare a queue
    var queueDeclare = channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
    queueDeclare.Should().NotBeNull();

    // Publish a simple message
    var body = System.Text.Encoding.UTF8.GetBytes("hello");
    channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

    // Get message
    var result = channel.BasicGet(queueName, autoAck: true);
    result.Should().NotBeNull();
    var message = System.Text.Encoding.UTF8.GetString(result.Body.ToArray());
    message.Should().Be("hello");
  }
}