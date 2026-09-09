using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
namespace CloudShopping.Infrastructure.Persistence;

// Explicit CLI/test entry point: the API never migrates a production database on startup.
public static class SchemaMigrator
{
    public static async Task Apply(string connectionString, string directory, CancellationToken ct = default)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(ct);
        if (string.IsNullOrWhiteSpace(connection.Database)) throw new ArgumentException("Informe Database explicitamente.");
        await using var acquire = new MySqlCommand("SELECT GET_LOCK(SHA2(CONCAT(DATABASE(), ':cloudshopping_migrate'),256), 30)", connection);
        if (Convert.ToInt32(await acquire.ExecuteScalarAsync(ct)) != 1) throw new InvalidOperationException("Outra migração está em execução.");
        try
        {
            await new MySqlCommand("CREATE TABLE IF NOT EXISTS schema_migrations (Name varchar(150) PRIMARY KEY, Checksum char(64) NOT NULL, CompletedAt datetime(6) NULL)", connection).ExecuteNonQueryAsync(ct);
            foreach (var path in Directory.GetFiles(directory, "*.sql").Order())
            {
                var name = Path.GetFileName(path); var sql = await File.ReadAllTextAsync(path, ct);
                var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
                await using var query = new MySqlCommand("SELECT Checksum,CompletedAt FROM schema_migrations WHERE Name=@name", connection);
                query.Parameters.AddWithValue("@name", name);
                bool exists = false;
                await using (var reader = await query.ExecuteReaderAsync(ct))
                {
                    if (await reader.ReadAsync(ct))
                    {
                        exists = true;
                        if (reader.GetString(0) != checksum) throw new InvalidOperationException($"Checksum alterado: {name}.");
                        if (reader.IsDBNull(1)) throw new InvalidOperationException($"Migração incompleta: {name}. Inspecione o schema e restaure/reconcilie antes de retomar.");
                    }
                }
                if (exists) continue;
                await using var start = new MySqlCommand("INSERT INTO schema_migrations(Name,Checksum) VALUES(@name,@checksum)", connection);
                start.Parameters.AddWithValue("@name", name); start.Parameters.AddWithValue("@checksum", checksum);
                await start.ExecuteNonQueryAsync(ct);
                // MySQL DDL commits implicitly. Mark incomplete BEFORE applying, fail closed on interruption.
                await using var apply = new MySqlCommand(sql, connection) { CommandTimeout = 120 };
                await apply.ExecuteNonQueryAsync(ct);
                await using var finish = new MySqlCommand("UPDATE schema_migrations SET CompletedAt=UTC_TIMESTAMP(6) WHERE Name=@name", connection);
                finish.Parameters.AddWithValue("@name", name); await finish.ExecuteNonQueryAsync(ct);
            }
        }
        finally { await new MySqlCommand("SELECT RELEASE_LOCK(SHA2(CONCAT(DATABASE(), ':cloudshopping_migrate'),256))", connection).ExecuteScalarAsync(CancellationToken.None); }
    }
}

