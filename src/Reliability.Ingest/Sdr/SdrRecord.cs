namespace Reliability.Ingest.Sdr;

/// <summary>
/// FAA SDR dosyasındaki bir satırın bizim kullandığımız alanları.
/// Dosyada 76 sütun var; motor, pervane ve yapısal (gövde istasyonu vb.) alanları şimdilik almıyoruz.
/// </summary>
public sealed record SdrRecord(
    string OperatorControlNumber,
    DateOnly DifficultyDate,
    string? OperatorDesignator,
    string? AircraftMake,
    string? AircraftModel,
    string? RegistryNNumber,
    decimal? AircraftTotalTime,
    int? AircraftTotalCycles,
    string JascCode,
    string? PartName,
    string? PartNumber,
    string? PartCondition,
    string? PartLocation,
    decimal? PartTotalTime,
    string? NatureOfConditionA,
    string? StageOfOperationCode,
    string? HowDiscoveredCode,
    string Discrepancy);
