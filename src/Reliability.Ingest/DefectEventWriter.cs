using Npgsql;
using NpgsqlTypes;
using Reliability.Ingest.Sdr;

namespace Reliability.Ingest;

/// <summary>
/// SDR kayıtlarını defect_event tablosuna yazar. Tekrar çalıştırılabilir (idempotent):
/// aynı kayıt (source + source_ref) zaten varsa atlanır, hata verilmez.
/// </summary>
public sealed class DefectEventWriter(NpgsqlDataSource db)
{
    public const string Source = "FAA_SDR";
    private const int BatchSize = 1000;

    // Her sütun için bir dizi gönderilir; unnest bu dizileri yan yana açıp satırlara çevirir.
    // Böylece 1000 kayıt tek bir SQL komutuyla, 18 parametreyle gider (1000 x 18 = 18.000 parametre yerine).
    private const string InsertSql = """
        insert into defect_event (
            source, source_ref, difficulty_date, operator_code, aircraft_make, aircraft_model,
            registration, aircraft_total_hours, aircraft_total_cycles, jasc_code,
            part_name, part_number, part_condition, part_location, part_total_hours,
            nature_of_condition, stage_of_operation, how_discovered, discrepancy_raw)
        select @source, * from unnest(
            @source_ref, @difficulty_date, @operator_code, @aircraft_make, @aircraft_model,
            @registration, @aircraft_total_hours, @aircraft_total_cycles, @jasc_code,
            @part_name, @part_number, @part_condition, @part_location, @part_total_hours,
            @nature_of_condition, @stage_of_operation, @how_discovered, @discrepancy_raw)
        on conflict (source, source_ref) do nothing
        """;

    /// <returns>Yeni eklenen satır sayısı. Zaten var olan kayıtlar sayılmaz.</returns>
    public async Task<int> UpsertAsync(IEnumerable<SdrRecord> records, CancellationToken ct)
    {
        await using var conn = await db.OpenConnectionAsync(ct);
        // Tek işlem (transaction): yarıda hata olursa hiçbir parti yazılmaz, tablo yarım kalmaz.
        await using var tx = await conn.BeginTransactionAsync(ct);

        var inserted = 0;
        foreach (var batch in records.Chunk(BatchSize))
            inserted += await InsertBatchAsync(conn, tx, batch, ct);

        await tx.CommitAsync(ct);
        return inserted;
    }

    private static async Task<int> InsertBatchAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx, SdrRecord[] b, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(InsertSql, conn, tx);
        var p = cmd.Parameters;
        p.Add(new NpgsqlParameter<string>("source", Source));
        Text(p, "source_ref", b.Select(r => (string?)r.OperatorControlNumber));
        p.Add(new NpgsqlParameter<DateOnly[]>("difficulty_date", NpgsqlDbType.Array | NpgsqlDbType.Date)
            { TypedValue = b.Select(r => r.DifficultyDate).ToArray() });
        Text(p, "operator_code", b.Select(r => r.OperatorDesignator));
        Text(p, "aircraft_make", b.Select(r => r.AircraftMake));
        Text(p, "aircraft_model", b.Select(r => r.AircraftModel));
        Text(p, "registration", b.Select(r => r.RegistryNNumber));
        Numeric(p, "aircraft_total_hours", b.Select(r => r.AircraftTotalTime));
        p.Add(new NpgsqlParameter<int?[]>("aircraft_total_cycles", NpgsqlDbType.Array | NpgsqlDbType.Integer)
            { TypedValue = b.Select(r => r.AircraftTotalCycles).ToArray() });
        Text(p, "jasc_code", b.Select(r => (string?)r.JascCode));
        Text(p, "part_name", b.Select(r => r.PartName));
        Text(p, "part_number", b.Select(r => r.PartNumber));
        Text(p, "part_condition", b.Select(r => r.PartCondition));
        Text(p, "part_location", b.Select(r => r.PartLocation));
        Numeric(p, "part_total_hours", b.Select(r => r.PartTotalTime));
        Text(p, "nature_of_condition", b.Select(r => r.NatureOfConditionA));
        Text(p, "stage_of_operation", b.Select(r => r.StageOfOperationCode));
        Text(p, "how_discovered", b.Select(r => r.HowDiscoveredCode));
        Text(p, "discrepancy_raw", b.Select(r => (string?)r.Discrepancy));

        // "on conflict do nothing" atlanan satırları saymaz; dönen sayı yalnızca yeni eklenenlerdir.
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void Text(NpgsqlParameterCollection p, string name, IEnumerable<string?> values) =>
        p.Add(new NpgsqlParameter<string?[]>(name, NpgsqlDbType.Array | NpgsqlDbType.Text) { TypedValue = values.ToArray() });

    private static void Numeric(NpgsqlParameterCollection p, string name, IEnumerable<decimal?> values) =>
        p.Add(new NpgsqlParameter<decimal?[]>(name, NpgsqlDbType.Array | NpgsqlDbType.Numeric) { TypedValue = values.ToArray() });
}
