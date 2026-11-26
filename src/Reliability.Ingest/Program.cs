using System.Diagnostics;
using System.Text;
using Npgsql;
using Reliability.Ingest;
using Reliability.Ingest.Sdr;

// Kullanım: dotnet run --project src/Reliability.Ingest -- data/raw/SDR-2025.csv [başka dosyalar...]
// Bağlantı: RELIABILITY_DB ortam değişkeni, yoksa yerel Docker Compose veritabanı.

if (args.Length == 0)
{
    Console.Error.WriteLine("Kullanım: Reliability.Ingest <sdr-csv> [<sdr-csv> ...]");
    return 1;
}

var connectionString = Environment.GetEnvironmentVariable("RELIABILITY_DB")
    ?? "Host=127.0.0.1;Port=5434;Database=reliability;Username=reliability;Password=reliability_dev";

await using var db = NpgsqlDataSource.Create(connectionString);
var writer = new DefectEventWriter(db);
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

foreach (var path in args)
{
    var sw = Stopwatch.StartNew();
    var read = 0;
    using var reader = new StreamReader(path, Encoding.UTF8);
    var records = SdrCsvReader.Read(reader).Select(r => { read++; return r; });

    var inserted = await writer.UpsertAsync(records, cts.Token);
    Console.WriteLine($"{Path.GetFileName(path)}  Okunan: {read}, Eklenen: {inserted}, Süre: {sw.Elapsed.TotalSeconds:F1} sn");
}

return 0;
