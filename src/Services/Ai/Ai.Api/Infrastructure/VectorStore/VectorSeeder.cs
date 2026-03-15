using Ai.Api.Domain.Embeddings;
using Ai.Api.Domain.Vectors;
using Npgsql;

namespace Ai.Api.Infrastructure.VectorStore;

public class VectorSeeder(
    IVectorStore vectorStore,
    IEmbeddingGenerator embeddingGenerator,
    IConfiguration configuration,
    ILogger<VectorSeeder> logger) : IHostedService
{
    private static readonly string[] SeedDocuments =
    [
        "MarketSpace is an e-commerce marketplace where customers can browse products, add them to their basket, and place orders. Orders go through the following statuses: Created, Processing, ReadyForDelivery, DeliveryInProgress, Delivered, Finalized, Cancelled.",
        "An order in MarketSpace can be cancelled by the customer (CancelledByCustomer) or by the system (Cancelled). Once an order is Finalized, it cannot be modified.",
        "Shipping in MarketSpace is handled after the order reaches ReadyForDelivery status. The delivery process moves through DeliveryInProgress until the order is marked as Delivered.",
        "Payment in MarketSpace is processed after order creation. The order status moves to Processing once payment is confirmed. Payment failures may result in order cancellation.",
        "Customers can track their orders by order ID. The order status reflects the current stage: Created means the order was just placed, Processing means payment is confirmed, ReadyForDelivery means the item is packed, DeliveryInProgress means it is on the way, and Delivered means it arrived."
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await EnsureTableExistsAsync(cancellationToken);
            await SeedIfEmptyAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "VectorSeeder failed during startup. RAG may not work until the database is available.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsureTableExistsAsync(CancellationToken cancellationToken)
    {
        string connectionString = configuration.GetConnectionString("AiDb")
            ?? throw new ArgumentNullException("AiDb connection string is not configured");

        NpgsqlDataSourceBuilder builder = new(connectionString);
        builder.UseVector();
        await using NpgsqlDataSource dataSource = builder.Build();
        await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(cancellationToken);

        const string createTable = """
            CREATE TABLE IF NOT EXISTS documents (
                id          SERIAL PRIMARY KEY,
                content     TEXT        NOT NULL,
                embedding   vector(768) NOT NULL,
                context_id  TEXT,
                metadata    TEXT,
                created_at  TIMESTAMPTZ DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS documents_embedding_idx ON documents USING ivfflat (embedding vector_cosine_ops);
            """;

        await using NpgsqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = createTable;
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        logger.LogInformation("Documents table ensured.");
    }

    private async Task SeedIfEmptyAsync(CancellationToken cancellationToken)
    {
        string connectionString = configuration.GetConnectionString("AiDb")
            ?? throw new ArgumentNullException("AiDb connection string is not configured");

        NpgsqlDataSourceBuilder builder = new(connectionString);
        builder.UseVector();
        await using NpgsqlDataSource dataSource = builder.Build();
        await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(cancellationToken);

        await using NpgsqlCommand countCmd = conn.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM documents";
        long count = (long)(await countCmd.ExecuteScalarAsync(cancellationToken) ?? 0L);

        if (count > 0)
        {
            logger.LogInformation("Documents table already has {Count} rows. Skipping seed.", count);
            return;
        }

        logger.LogInformation("Seeding {Count} documents into vector store...", SeedDocuments.Length);

        foreach (string document in SeedDocuments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float[] embedding = await embeddingGenerator.Generate(document);
            await vectorStore.Upsert(document, embedding, contextId: "marketspace-docs");
        }

        logger.LogInformation("Vector store seeded successfully.");
    }
}
