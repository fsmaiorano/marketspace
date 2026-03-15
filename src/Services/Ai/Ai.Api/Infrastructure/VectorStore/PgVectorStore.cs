using Ai.Api.Domain.Vectors;
using Npgsql;
using Pgvector;

namespace Ai.Api.Infrastructure.VectorStore;

public class PgVectorStore : IVectorStore
{
    private readonly string _connectionString;

    public PgVectorStore(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AiDb")
            ?? throw new ArgumentNullException("AiDb connection string is not configured");
    }

    public async Task<IEnumerable<string>> Search(float[] vector, string? contextId = null, int limit = 5)
    {
        var embedding = new Vector(vector);
        List<string> results = [];

        await using NpgsqlDataSource dataSource = CreateDataSource();
        await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync();

        string sql = contextId is not null
            ? "SELECT content FROM documents WHERE context_id = $1 ORDER BY embedding <=> $2 LIMIT $3"
            : "SELECT content FROM documents ORDER BY embedding <=> $1 LIMIT $2";

        await using NpgsqlCommand cmd = conn.CreateCommand();

        if (contextId is not null)
        {
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue(contextId);
            cmd.Parameters.AddWithValue(embedding);
            cmd.Parameters.AddWithValue(limit);
        }
        else
        {
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue(embedding);
            cmd.Parameters.AddWithValue(limit);
        }

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(reader.GetString(0));

        return results;
    }

    public async Task Upsert(string content, float[] embedding, string? contextId = null, string? metadata = null)
    {
        var vector = new Vector(embedding);

        await using NpgsqlDataSource dataSource = CreateDataSource();
        await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync();

        const string sql = """
            INSERT INTO documents (content, embedding, context_id, metadata)
            VALUES ($1, $2, $3, $4)
            """;

        await using NpgsqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue(content);
        cmd.Parameters.AddWithValue(vector);
        cmd.Parameters.AddWithValue((object?)contextId ?? DBNull.Value);
        cmd.Parameters.AddWithValue((object?)metadata ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    private NpgsqlDataSource CreateDataSource()
    {
        NpgsqlDataSourceBuilder builder = new(_connectionString);
        builder.UseVector();
        return builder.Build();
    }
}