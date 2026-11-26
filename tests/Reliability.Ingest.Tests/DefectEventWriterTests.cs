using Reliability.Ingest.Sdr;

namespace Reliability.Ingest.Tests;

// Gerçek PostgreSQL'e karşı çalışır (docker compose up -d gerekir).
[Collection(DatabaseCollection.Name)]
public sealed class DefectEventWriterTests(TestDatabase db) : IAsyncLifetime
{
    private readonly DefectEventWriter writer = new(db.DataSource);
    private readonly CancellationToken ct = CancellationToken.None;

    private static List<SdrRecord> Fixture()
    {
        using var reader = new StreamReader(Path.Combine(AppContext.BaseDirectory, "Fixtures", "sdr_sample.csv"));
        return SdrCsvReader.Read(reader).ToList();
    }

    public Task InitializeAsync() => db.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task First_import_adds_all() =>
        Assert.Equal(4, await writer.UpsertAsync(Fixture(), ct));

    [Fact]
    public async Task Second_import_adds_nothing()
    {
        Assert.Equal(4, await writer.UpsertAsync(Fixture(), ct));
        Assert.Equal(0, await writer.UpsertAsync(Fixture(), ct));
        Assert.Equal(4L, await db.ScalarAsync<long>("select count(*) from defect_event"));
    }

    [Fact]
    public async Task Duplicate_inside_one_import_is_added_once()
    {
        var twice = Fixture().Concat(Fixture());
        Assert.Equal(4, await writer.UpsertAsync(twice, ct));
    }

    [Fact]
    public async Task Ata_chapter_is_derived()
    {
        await writer.UpsertAsync(Fixture(), ct);
        Assert.Equal("05", await db.ScalarAsync<string>("select ata_chapter from defect_event where jasc_code = '0510'"));
    }

    [Fact]
    public async Task Fields_are_stored()
    {
        await writer.UpsertAsync(Fixture(), ct);
        Assert.Equal("FAA_SDR", await db.ScalarAsync<string>("select distinct source from defect_event"));
        Assert.Equal(new DateOnly(2025, 6, 11),
            await db.ScalarAsync<DateOnly>("select difficulty_date from defect_event where source_ref = 'F3LA202506110247'"));
        Assert.Equal(53568m,
            await db.ScalarAsync<decimal>("select aircraft_total_hours from defect_event where source_ref = 'SCNA20250102001'"));
        Assert.Contains("CORRECTIVE ACTION",
            await db.ScalarAsync<string>("select discrepancy_raw from defect_event where source_ref = 'F3LA202506110247'"));
    }

    [Fact]
    public async Task Empty_values_are_null()
    {
        await writer.UpsertAsync(Fixture(), ct);
        Assert.Equal(1L, await db.ScalarAsync<long>(
            "select count(*) from defect_event where source_ref = '2025FA0000000' " +
            "and aircraft_total_hours is null and aircraft_total_cycles is null and registration is null"));
    }

    [Fact]
    public async Task Many_records_cross_batch_boundary()
    {
        // 2500 kayıt = 1000'lik 3 parti. Parti sınırında kayıp ya da tekrar olmamalı.
        var template = Fixture()[0];
        var many = Enumerable.Range(0, 2500).Select(i => template with { OperatorControlNumber = $"T{i:D5}" });
        Assert.Equal(2500, await writer.UpsertAsync(many, ct));
        Assert.Equal(2500L, await db.ScalarAsync<long>("select count(*) from defect_event"));
    }
}
