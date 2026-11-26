using CsvHelper;

namespace Reliability.Ingest.Sdr;

/// <summary>
/// FAA SDR CSV dosyasını okur. Sütunlar sıraya göre değil başlık adına göre bulunur,
/// böylece FAA sütun ekler ya da sırasını değiştirirse okuyucu bozulmaz.
/// </summary>
public static class SdrCsvReader
{
    public static IEnumerable<SdrRecord> Read(TextReader textReader)
    {
        // Sayılar varsayılan olarak InvariantCulture ile okunur ('.' ondalık ayırıcı). SDR böyle yazılmış.
        using var csv = CsvReader.Create(textReader, leaveOpen: true);
        var col = new Columns(csv);

        while (csv.Read())
        {
            yield return new SdrRecord(
                OperatorControlNumber: Required(csv, col.OperatorControlNumber, "OperatorControlNumber"),
                DifficultyDate: Date(csv, col.DifficultyDate),
                OperatorDesignator: Text(csv, col.OperatorDesignator),
                AircraftMake: Text(csv, col.AircraftMake),
                AircraftModel: Text(csv, col.AircraftModel),
                RegistryNNumber: Text(csv, col.RegistryNNumber),
                AircraftTotalTime: Number<decimal>(csv, col.AircraftTotalTime, "AircraftTotalTime"),
                AircraftTotalCycles: Number<int>(csv, col.AircraftTotalCycles, "AircraftTotalCycles"),
                // Metin olarak kalır: sayıya çevrilseydi "0510" -> 510 olurdu.
                JascCode: Required(csv, col.JascCode, "JASCCode"),
                PartName: Text(csv, col.PartName),
                PartNumber: Text(csv, col.PartNumber),
                PartCondition: Text(csv, col.PartCondition),
                PartLocation: Text(csv, col.PartLocation),
                PartTotalTime: Number<decimal>(csv, col.PartTotalTime, "PartTotalTime"),
                NatureOfConditionA: Text(csv, col.NatureOfConditionA),
                StageOfOperationCode: Text(csv, col.StageOfOperationCode),
                HowDiscoveredCode: Text(csv, col.HowDiscoveredCode),
                // Satır kırılımları burada korunur; temizleme Görev 5'in işi.
                Discrepancy: Required(csv, col.Discrepancy, "Discrepancy"));
        }
    }

    /// <summary>Boş alan -> null. Boşluklar kırpılır.</summary>
    private static string? Text(CsvReader csv, int i)
    {
        var s = csv.GetString(i).Trim();
        return s.Length == 0 ? null : s;
    }

    /// <summary>Zorunlu alan boşsa hangi satır ve sütun olduğunu söyleyerek hata verir.</summary>
    private static string Required(CsvReader csv, int i, string name) =>
        Text(csv, i) ?? throw Bad(csv, name, "boş olamaz");

    /// <summary>Boş -> null. Dolu ama sayı değilse sessizce null yapmak yerine hata verir.</summary>
    private static T? Number<T>(CsvReader csv, int i, string name) where T : struct, ISpanParsable<T>
    {
        var s = csv.GetString(i).Trim();
        if (s.Length == 0) return null;
        return T.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var v)
            ? v
            : throw Bad(csv, name, $"sayı değil: '{s}'");
    }

    /// <summary>SDR tarihleri ABD biçiminde: ay/gün/yıl. "06/11/2025" = 11 Haziran.</summary>
    private static DateOnly Date(CsvReader csv, int i)
    {
        var s = csv.GetString(i).Trim();
        return DateOnly.TryParseExact(s, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var d)
            ? d
            : throw Bad(csv, "DifficultyDate", $"MM/dd/yyyy değil: '{s}'");
    }

    private static FormatException Bad(CsvReader csv, string column, string problem) =>
        new($"SDR kayıt {csv.RowNumber}, {column}: {problem}");

    /// <summary>Sütun numaraları başlıktan bir kez bulunur. Eksik sütun varsa en başta hata verir.</summary>
    private sealed class Columns(CsvReader csv)
    {
        public readonly int OperatorControlNumber = Find(csv, "OperatorControlNumber");
        public readonly int DifficultyDate = Find(csv, "DifficultyDate");
        public readonly int OperatorDesignator = Find(csv, "OperatorDesignator");
        public readonly int AircraftMake = Find(csv, "AircraftMake");
        public readonly int AircraftModel = Find(csv, "AircraftModel");
        public readonly int RegistryNNumber = Find(csv, "RegistryNNumber");
        public readonly int AircraftTotalTime = Find(csv, "AircraftTotalTime");
        public readonly int AircraftTotalCycles = Find(csv, "AircraftTotalCycles");
        public readonly int JascCode = Find(csv, "JASCCode");
        public readonly int PartName = Find(csv, "PartName");
        public readonly int PartNumber = Find(csv, "PartNumber");
        public readonly int PartCondition = Find(csv, "PartCondition");
        public readonly int PartLocation = Find(csv, "PartLocation");
        public readonly int PartTotalTime = Find(csv, "PartTotalTime");
        public readonly int NatureOfConditionA = Find(csv, "NatureOfConditionA");
        public readonly int StageOfOperationCode = Find(csv, "StageOfOperationCode");
        public readonly int HowDiscoveredCode = Find(csv, "HowDiscoveredCode");
        public readonly int Discrepancy = Find(csv, "Discrepancy");

        private static int Find(CsvReader csv, string name)
        {
            var i = csv.GetFieldIndex(name);
            return i >= 0 ? i : throw new FormatException($"SDR dosyasında '{name}' sütunu yok");
        }
    }
}
