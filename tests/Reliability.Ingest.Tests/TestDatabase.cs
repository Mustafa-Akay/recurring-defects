using Npgsql;

namespace Reliability.Ingest.Tests;

/// <summary>
/// Testler için ayrı bir veritabanı (reliability_test). Gerçek veriyi tutan 'reliability' veritabanına dokunmaz.
/// Test çalışması başında silinip db/migrations'taki SQL ile baştan kurulur, böylece şema hep gerçekle aynıdır.
/// Bağlantı: RELIABILITY_TEST_ADMIN ortam değişkeni (CI için), yoksa yerel Docker Compose.
/// </summary>
public sealed class TestDatabase : IAsyncLifetime
{
    private const string TestDbName = "reliability_test";

    private static readonly string AdminConnectionString =
        Environment.GetEnvironmentVariable("RELIABILITY_TEST_ADMIN")
        ?? "Host=127.0.0.1;Port=5434;Database=reliability;Username=reliability;Password=reliability_dev";

    public NpgsqlDataSource DataSource { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await using (var admin = NpgsqlDataSource.Create(AdminConnectionString))
        {
            await admin.CreateCommand($"drop database if exists {TestDbName} with (force)").ExecuteNonQueryAsync();
            await admin.CreateCommand($"create database {TestDbName}").ExecuteNonQueryAsync();
        }

        var testCs = new NpgsqlConnectionStringBuilder(AdminConnectionString) { Database = TestDbName }.ConnectionString;
        DataSource = NpgsqlDataSource.Create(testCs);

        foreach (var file in Directory.GetFiles(FindMigrationsDir(), "*.sql").Order())
            await DataSource.CreateCommand(await File.ReadAllTextAsync(file)).ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync() => await DataSource.DisposeAsync();

    public async Task ResetAsync() =>
        await DataSource.CreateCommand("truncate defect_event restart identity").ExecuteNonQueryAsync();

    public async Task<T?> ScalarAsync<T>(string sql) =>
        (T?)await DataSource.CreateCommand(sql).ExecuteScalarAsync();

    /// <summary>Test çıktı klasöründen yukarı çıkarak repo kökündeki db/migrations'ı bulur.</summary>
    private static string FindMigrationsDir()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "db", "migrations");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("db/migrations bulunamadı");
    }
}

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<TestDatabase>
{
    public const string Name = "database";
}
