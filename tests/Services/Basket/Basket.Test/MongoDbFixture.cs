using Mongo2Go;
using MongoDB.Driver;

namespace Basket.Test;

public class MongoDbFixture
{
    public MongoDbRunner Runner { get; private set; }
    public MongoClient Client { get; private set; }

    public MongoDbFixture()
    {
        Runner = MongoDbRunner.Start();
        Client = new MongoClient(Runner.ConnectionString);
    }
}