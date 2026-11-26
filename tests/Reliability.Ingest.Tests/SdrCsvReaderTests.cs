using Reliability.Ingest.Sdr;

namespace Reliability.Ingest.Tests;

// Fixtures/sdr_sample.csv: SDR-2025.csv'den alınmış 4 gerçek kayıt (başlık satırı dahil, birebir).
// [0] SCNA20250102001  normal kayıt, 01/01/2025
// [1] F3LA202506110247 Discrepancy iki satır (alan içinde CRLF), virgül ve tırnak içeriyor
// [2] AALA202501026752 gerçek kayıt, JASCCode 5210 -> 0510 olarak DEĞİŞTİRİLDİ (2025'te 0'la başlayan kod yok)
// [3] 2025FA0000000    AircraftTotalTime ve AircraftTotalCycles boş
public class SdrCsvReaderTests
{
    private static List<SdrRecord> Read()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "sdr_sample.csv");
        using var reader = new StreamReader(path);
        return SdrCsvReader.Read(reader).ToList();
    }

    [Fact]
    public void Reads_all_records() => Assert.Equal(4, Read().Count);

    [Fact]
    public void Multiline_discrepancy_is_one_record()
    {
        var d = Read()[1].Discrepancy;
        Assert.StartsWith("AIRCRAFT WAS NOT GROUNDED", d);
        Assert.Contains("CORRECTIVE ACTION: REPLACED EPSU BATTERY", d); // ikinci satır kaybolmamış
    }

    [Fact]
    public void Jasc_keeps_leading_zero() => Assert.Equal("0510", Read()[2].JascCode);

    [Fact]
    public void Empty_numeric_is_null()
    {
        var r = Read()[3];
        Assert.Null(r.AircraftTotalTime);
        Assert.Null(r.AircraftTotalCycles);
    }

    [Fact]
    public void Empty_text_is_null() => Assert.Null(Read()[3].RegistryNNumber);

    [Fact]
    public void Date_is_parsed_month_first()
    {
        Assert.Equal(new DateOnly(2025, 1, 1), Read()[0].DifficultyDate);
        Assert.Equal(new DateOnly(2025, 6, 11), Read()[1].DifficultyDate); // 06/11 = 11 Haziran, 6 Kasım değil
    }

    [Fact]
    public void Numbers_are_read()
    {
        var r = Read()[0];
        Assert.Equal(53568m, r.AircraftTotalTime);
        Assert.Equal(29760, r.AircraftTotalCycles);
        Assert.Equal("7378F2", r.AircraftModel);
        Assert.Equal("825SY", r.RegistryNNumber);
    }
}
