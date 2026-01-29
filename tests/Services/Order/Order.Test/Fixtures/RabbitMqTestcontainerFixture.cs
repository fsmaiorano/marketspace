using MarketSpace.TestFixtures;

namespace Order.Test.Fixtures;

public static class RabbitMqTestcontainerFixture
{
  public static void Start() => MarketSpace.TestFixtures.RabbitMqTestcontainerFixture.Start();

  public static bool IsAvailable => MarketSpace.TestFixtures.RabbitMqTestcontainerFixture.IsAvailable;

  public static string GetConnectionString() => MarketSpace.TestFixtures.RabbitMqTestcontainerFixture.GetConnectionString();

  public static void Stop() => MarketSpace.TestFixtures.RabbitMqTestcontainerFixture.Stop();
}