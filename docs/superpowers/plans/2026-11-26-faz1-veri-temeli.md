# Faz 1: Veri Temeli — Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** FAA SDR kayıtlarını standart bir olay tablosuna almak, arıza metinlerini temizlemek ve ölçüm için etiketli bir kayıt çifti seti üretmek. Bu, tasarımın 1. haftası ve 2. haftanın iskeletidir.

**Architecture:** PostgreSQL + pgvector Docker'da çalışır. .NET konsol uygulaması SDR CSV dosyasını okur ve `defect_event` tablosuna tekrar çalıştırılabilir (idempotent) şekilde yazar. Python paketi aynı tabloyu okur, metni temizleyip `discrepancy_clean` sütununa yazar ve etiketleme çiftlerini örnekler. .NET ile Python yalnızca veritabanı üzerinden konuşur.

**Tech Stack:** .NET 10, Npgsql, CsvHelper, xUnit · PostgreSQL 17 + pgvector (`pgvector/pgvector:pg17` imajı) · Python 3.12, pandas, psycopg 3, pytest · Docker Compose · Git + GitHub Actions

**Spec:** `docs/superpowers/specs/2026-11-26-tekrarlayan-ariza-design.md`

**Sonraki planlar:** Faz 2 (gruplama + embedding + ölçüm), Faz 3 (uyarı seviyesi + API + React pano), Faz 4 (cilalama, sunum). Her biri bir önceki fazın bulgularıyla yazılır.

## Global Constraints

- Öğrenme biçimi C: **[KULLANICI]** etiketli görevleri kullanıcı yazar, Claude yol gösterir ve inceler. **[CLAUDE]** etiketli görevleri Claude yazar ve önemli yerleri anlatır. Her görevin başında "ne, neden" açıklaması yapılır.
- Kod klasörü: `C:\Users\Hp\Documents\havayolu-projeleri\recurring-defects\`, kendi Git deposu (GitHub: Mustafa-Akay/recurring-defects).
- Ham veri Git'e girmez: `data/raw/` `.gitignore`'da.
- PostgreSQL yalnızca `127.0.0.1` üzerinde yayımlanır (AeroBag güvenlik bulgusu #2).
- Kişisel veri yok. SDR herkese açık. Tescil numarası uçağı tanımlar, kişiyi tanımlamaz.
- Dış API yok. Her şey yerelde çalışır.
- Her kararın "neden"i ilgili görevin commit mesajına ya da `docs/decisions.md`'ye bir satır olarak yazılır.

## Veri hakkında bilinenler (2025 dosyası, 26 Kasım 2026'da ölçüldü)

67.654 kayıt, 76 sütun. Doluluk oranları: `JASCCode` %100, `Discrepancy` %100, `RegistryNNumber` %99,5, `AircraftModel` %99,9, `AircraftTotalTime` %99,1, `AircraftTotalCycles` %98,4, `PartName` %100, `PartNumber` %35,8, `PartTotalTime` %7,3. Tarih biçimi `MM/dd/yyyy`. `Discrepancy` alanı tırnaklı ve çok satırlı olabilir. Sonuç: **aynı uçakta tekrar** ve **uçak saatine göre normalleştirme** yapılabilir. Parça seviyesinde kestirimci bakım verisi zayıf (%7).

## Review Focus

1. **Aynı dosyayı iki kez içe almak** → kayıt sayısı değişmemeli. (Görev 3'te test.)
2. **Çok satırlı ve virgüllü `Discrepancy`** → tek kayıt olarak okunmalı, satır kırılımı korunmamalı ama metin kaybolmamalı. (Görev 3'te test.)
3. **Başında sıfır olan JASC kodu (ör. `0510`)** → sayıya çevrilip `510` olmamalı. (Görev 3'te test.)
4. **Boş sayısal alanlar (`AircraftTotalTime` boş)** → içe alma çökmemeli, `NULL` yazılmalı. (Görev 3'te test.)
5. **Kısaltma sözlüğünde kelime içi eşleşme** (`INOP` sözlükte varken `INOPERATIVE` bozulmamalı) → yalnızca tam kelime eşleşmeli. (Görev 5'te test.)

---

### Görev 0: Okuma ve alan öğrenme [KULLANICI, Claude anlatır]

Kod yok. Sonraki görevlerin "neden"leri buradan gelir.

- [ ] **Adım 1:** JASC/ATA bölümlerini öğren. Çıktı: `docs/notes/jasc.md` içinde en sık görülecek 10 bölümün (21, 27, 32, 52 vb.) tek satırlık açıklaması.
- [ ] **Adım 2:** FAA AC 120-17B'nin veri analizi ve uyarı seviyesi bölümünü oku. Çıktı: `docs/notes/alert-level.md` içinde uyarı seviyesinin formülü, hangi oranla (1000 uçuş saati başına vb.) hesaplandığı ve bizim veride neyin karşılandığı.
- [ ] **Adım 3:** ATA Spec 2000 Bölüm 11'in herkese açık kayıt tiplerini (Logbook, LRU Removal) oku. Çıktı: `docs/notes/spec2000.md` içinde `defect_event` tablomuzun hangi alanlarının hangi kayıt tipine karşılık geldiği.
- [ ] **Adım 4:** Rakipler (Veryon, Avilytics, Skywise) için `docs/notes/rakipler.md`: her biri ne yapıyor, biz neyi yapmıyoruz.

### Görev 1: Depo iskeleti ve veritabanı [CLAUDE]

**Files:**
- Create: `recurring-defects/docker-compose.yml`, `.gitignore`, `README.md`, `docs/decisions.md`
- Create: `db/migrations/0001_defect_event.sql`

**Interfaces:**
- Produces: `defect_event` tablosu (aşağıda). Bağlantı: `Host=127.0.0.1;Port=5434;Database=reliability;Username=reliability;Password=reliability_dev` (port 5434, AeroBag'in 5433'üyle çakışmasın diye).

Tablo sütunları (Spec 2000 Logbook kaydından esinlenir):

| Sütun | Tip | Kaynak |
|---|---|---|
| `id` | `bigserial` PK | — |
| `source` | `text not null` | sabit `'FAA_SDR'` |
| `source_ref` | `text not null` | `OperatorControlNumber`. `unique(source, source_ref)` |
| `difficulty_date` | `date not null` | `DifficultyDate` |
| `operator_code` | `text` | `OperatorDesignator` |
| `aircraft_make`, `aircraft_model` | `text` | aynı adlı alanlar |
| `registration` | `text` | `RegistryNNumber` |
| `aircraft_total_hours` | `numeric` | `AircraftTotalTime` |
| `aircraft_total_cycles` | `integer` | `AircraftTotalCycles` |
| `jasc_code` | `char(4) not null` | `JASCCode` |
| `ata_chapter` | `char(2) generated always as (left(jasc_code,2)) stored` | — |
| `part_name`, `part_number`, `part_condition`, `part_location` | `text` | aynı adlı alanlar |
| `part_total_hours` | `numeric` | `PartTotalTime` |
| `nature_of_condition` | `text` | `NatureOfConditionA` |
| `stage_of_operation`, `how_discovered` | `text` | kod alanları |
| `discrepancy_raw` | `text not null` | `Discrepancy` |
| `discrepancy_clean` | `text` | Görev 5 doldurur |
| `embedding` | `vector(384)` | Faz 2 doldurur |
| `ingested_at` | `timestamptz not null default now()` | — |

İndeksler: `(aircraft_model, jasc_code, difficulty_date)`, `(registration, jasc_code, difficulty_date)`.

- [ ] **Adım 1:** `docker-compose.yml`: `pgvector/pgvector:pg17`, `127.0.0.1:5434:5432`, migration klasörü `/docker-entrypoint-initdb.d`'ye bağlı. `create extension if not exists vector;` migration'ın ilk satırı.
- [ ] **Adım 2:** Çalıştır: `docker compose up -d` ardından `docker compose exec db psql -U reliability -d reliability -c "\d defect_event"`. Beklenen: tablo ve `ata_chapter` üretilmiş sütunu görünür.
- [ ] **Adım 3:** `git init`, commit: `chore: repo iskeleti ve defect_event tablosu`.

### Görev 2: SDR CSV okuyucu [CLAUDE]

**Files:**
- Create: `src/Reliability.Ingest/Reliability.Ingest.csproj`, `Sdr/SdrRecord.cs`, `Sdr/SdrCsvReader.cs`
- Test: `tests/Reliability.Ingest.Tests/SdrCsvReaderTests.cs`, `tests/Reliability.Ingest.Tests/Fixtures/sdr_sample.csv`

**Interfaces:**
- Produces: `public sealed record SdrRecord(string OperatorControlNumber, DateOnly DifficultyDate, string? OperatorDesignator, string? AircraftMake, string? AircraftModel, string? RegistryNNumber, decimal? AircraftTotalTime, int? AircraftTotalCycles, string JascCode, string? PartName, string? PartNumber, string? PartCondition, string? PartLocation, decimal? PartTotalTime, string? NatureOfConditionA, string? StageOfOperationCode, string? HowDiscoveredCode, string Discrepancy)`
- Produces: `public static IEnumerable<SdrRecord> SdrCsvReader.Read(TextReader reader)`

- [ ] **Adım 1:** Fixture: 2025 dosyasının başlığı + 4 satır: normal bir kayıt, çok satırlı `Discrepancy`'li bir kayıt, JASC'i `0510` olan bir kayıt, `AircraftTotalTime`'ı boş bir kayıt.
- [ ] **Adım 2: Başarısız testler**

```csharp
[Fact] public void Reads_all_records() => Assert.Equal(4, Read().Count);
[Fact] public void Multiline_discrepancy_is_one_record() =>
    Assert.Contains("PER AMM", Read()[1].Discrepancy);
[Fact] public void Jasc_keeps_leading_zero() => Assert.Equal("0510", Read()[2].JascCode);
[Fact] public void Empty_numeric_is_null() => Assert.Null(Read()[3].AircraftTotalTime);
[Fact] public void Date_is_parsed_month_first() =>
    Assert.Equal(new DateOnly(2025, 1, 1), Read()[0].DifficultyDate);
```

- [ ] **Adım 3:** Çalıştır: `dotnet test --filter SdrCsvReaderTests`. Beklenen: derleme hatası ya da FAIL.
- [ ] **Adım 4:** `SdrCsvReader.Read` uygula. CsvHelper, `InvariantCulture`, başlığa göre eşleme, `DifficultyDate` için `MM/dd/yyyy`, boş sayısal → `null`, JASC metin olarak.
- [ ] **Adım 5:** Testler geçer. Commit: `feat: SDR CSV okuyucu`.

### Görev 3: Veritabanına içe alma [CLAUDE]

**Files:**
- Create: `src/Reliability.Ingest/DefectEventWriter.cs`, `Program.cs`
- Test: `tests/Reliability.Ingest.Tests/DefectEventWriterTests.cs` (gerçek PostgreSQL'e karşı, Görev 1'deki Compose)

**Interfaces:**
- Consumes: `SdrCsvReader.Read`, `defect_event`
- Produces: `public Task<int> DefectEventWriter.UpsertAsync(IEnumerable<SdrRecord> records, CancellationToken ct)` — eklenen yeni satır sayısını döner
- Produces: CLI `dotnet run --project src/Reliability.Ingest -- <csv-yolu>` → `Okunan: N, Eklenen: M`

- [ ] **Adım 1: Başarısız testler**

```csharp
[Fact] public async Task Second_import_adds_nothing() {
    Assert.Equal(4, await writer.UpsertAsync(fixture, ct));
    Assert.Equal(0, await writer.UpsertAsync(fixture, ct));
    Assert.Equal(4, await CountRows());
}
[Fact] public async Task Ata_chapter_is_derived() =>
    Assert.Equal("05", await Scalar("select ata_chapter from defect_event where jasc_code='0510'"));
```

- [ ] **Adım 2:** Çalıştır, FAIL gör.
- [ ] **Adım 3:** `UpsertAsync` uygula. `insert ... on conflict (source, source_ref) do nothing`, 1000'lik partiler halinde, tek işlem (transaction) içinde. Hız için `COPY` yerine parti insert yeterli (ölçüm: 68 bin kayıt).
- [ ] **Adım 4:** Testler geçer.
- [ ] **Adım 5:** Gerçek veri: 2020-2025 dosyalarını `data/raw/`'a indir, içe al. Beklenen: 2025 için `Okunan: 67654`. Süreyi `docs/decisions.md`'ye yaz.
- [ ] **Adım 6:** Commit: `feat: SDR içe alma, tekrar çalıştırılabilir`.

### Görev 4: Veri keşif raporu [KULLANICI]

**Files:**
- Create: `analysis/pyproject.toml`, `analysis/notebooks/01_kesif.ipynb`, `docs/notes/veri-raporu.md`

**Interfaces:**
- Consumes: `defect_event` (psycopg 3 ile)

- [ ] **Adım 1:** Uçak tipine göre kayıt sayısı. En çok kayıt üreten 10 `aircraft_model`. Model adlarının aileye nasıl toplanacağını not et (ör. `7378F2`, `737-8H4` → B737NG).
- [ ] **Adım 2:** En sık 15 `jasc_code` ve Görev 0'daki notlarla anlamları.
- [ ] **Adım 3:** Aynı `registration` + aynı `jasc_code` için 30 gün içinde birden fazla kayıt olan durumların sayısı. Bu, "aynı uçakta tekrar"ın veride ne kadar var olduğunu gösterir.
- [ ] **Adım 4:** Rastgele 50 `discrepancy_raw` oku, gördüğün kısaltmaları listele (Görev 5'in girdisi).
- [ ] **Adım 5:** Bulguları `docs/notes/veri-raporu.md`'ye yaz. **Karar noktası:** tasarımdaki "tekrar" tanımı ve tip ailesi eşlemesi burada kesinleşir. Commit: `docs: veri keşif raporu`.

### Görev 5: Metin temizleme ve kısaltma sözlüğü [KULLANICI]

**Files:**
- Create: `analysis/src/reliability/normalize.py`, `analysis/data/abbreviations.csv` (sütunlar: `abbr,expansion`)
- Create: `analysis/src/reliability/jobs/clean_discrepancies.py`
- Test: `analysis/tests/test_normalize.py`

**Interfaces:**
- Produces: `load_abbreviations(path: Path) -> dict[str, str]`
- Produces: `clean_text(raw: str, abbreviations: dict[str, str]) -> str`
- Produces: komut `python -m reliability.jobs.clean_discrepancies` → `discrepancy_clean` sütununu doldurur

- [ ] **Adım 1: Başarısız testler**

```python
ABBR = {"INOP": "inoperative", "L/H": "left hand", "R/H": "right hand", "ACFT": "aircraft"}

def test_expands_abbreviation():
    assert clean_text("L/H PACK INOP", ABBR) == "left hand pack inoperative"

def test_whole_word_only():
    assert clean_text("PACK INOPERATIVE", ABBR) == "pack inoperative"

def test_strips_reference_boilerplate():
    assert clean_text("REPLACED VALVE PER AMM 21-51-00.", ABBR) == "replaced valve"

def test_collapses_whitespace_and_newlines():
    assert clean_text("DOOR\n  DAMPER   LOW", ABBR) == "door damper low"
```

- [ ] **Adım 2:** Çalıştır: `pytest analysis/tests/test_normalize.py -v`. Beklenen: FAIL.
- [ ] **Adım 3:** `clean_text` uygula. Sıra: büyük harf → referans kalıplarını sil (`PER AMM ..`, `FIM TASK ..`, `IAW ..`) → kısaltmaları tam kelime olarak aç → küçük harf → boşlukları sadeleştir. Kalıpları neden sildiğimizi `docs/decisions.md`'ye yaz.
- [ ] **Adım 4:** Testler geçer.
- [ ] **Adım 5:** `abbreviations.csv`'yi Görev 4 Adım 4'teki listeyle doldur (en az 40 kısaltma).
- [ ] **Adım 6:** İşi çalıştır, 20 rastgele kaydın önce/sonra halini oku. Commit: `feat: metin temizleme ve kısaltma sözlüğü`.

### Görev 6: Etiketleme seti [KULLANICI]

**Files:**
- Create: `analysis/src/reliability/pairs.py`, `analysis/src/reliability/jobs/label_pairs.py`
- Create: `analysis/data/labels.csv` (sütunlar: `left_id,right_id,same`, `same` ∈ {0,1})
- Test: `analysis/tests/test_pairs.py`

**Interfaces:**
- Consumes: `defect_event.discrepancy_clean`
- Produces: `sample_pairs(df: pd.DataFrame, n: int, seed: int) -> pd.DataFrame` (sütunlar `left_id, right_id, aircraft_model, jasc_code`)
- Produces: komut `python -m reliability.jobs.label_pairs --n 300` → çiftleri terminalde gösterir, `y/n/s(kip)` alır, `labels.csv`'ye ekler, kaldığı yerden devam eder

- [ ] **Adım 1: Başarısız testler**

```python
def test_pairs_share_model_and_jasc(df):
    p = sample_pairs(df, n=50, seed=1)
    merged = p.merge(df, left_on="left_id", right_on="id").merge(df, left_on="right_id", right_on="id")
    assert (merged.jasc_code_x == merged.jasc_code_y).all()

def test_no_self_pairs(df):
    assert (sample_pairs(df, 50, 1).left_id != sample_pairs(df, 50, 1).right_id).all()

def test_same_seed_same_pairs(df):
    assert sample_pairs(df, 50, 1).equals(sample_pairs(df, 50, 1))

def test_draws_from_top_five_models(df):
    assert sample_pairs(df, 50, 1).aircraft_model.nunique() <= 5
```

- [ ] **Adım 2:** Çalıştır, FAIL gör.
- [ ] **Adım 3:** `sample_pairs` uygula. En çok kaydı olan 5 model ailesinden, aynı model + aynı JASC içinde rastgele çiftler. Yarısı aynı `part_name`'e sahip (büyük olasılıkla benzer), yarısı farklı (büyük olasılıkla farklı) olsun ki set dengeli olsun.
- [ ] **Adım 4:** Testler geçer.
- [ ] **Adım 5:** Etiketle: 300 çift (~3 saat, birkaç oturuma bölünebilir). Kararsız kaldığın örnekleri `docs/notes/etiketleme.md`'ye yaz. Bunlar "tekrar" tanımının sınırlarıdır.
- [ ] **Adım 6:** Commit: `feat: etiketleme aracı ve 300 etiketli çift`.

### Görev 7: CI ve hafta sonu anlatımı [CLAUDE + KULLANICI]

**Files:**
- Create: `.github/workflows/ci.yml`

- [ ] **Adım 1 [CLAUDE]:** CI: PostgreSQL servisiyle `dotnet test` ve `pytest`. `permissions: contents: read` (AeroBag bulgusu #7).
- [ ] **Adım 2:** Push, CI'ın yeşil olduğunu gör.
- [ ] **Adım 3 [KULLANICI]:** "Kendi cümlelerinle anlat" oturumu: Neden olay tablosu? Neden içe alma tekrar çalıştırılabilir? Neden referans kalıplarını siliyoruz? Neden aynı JASC içinde çift seçiyoruz? Veri raporunda ne çıktı? Claude eksikleri tamamlar, cevaplar `docs/notes/anlatim-faz1.md`'ye girer.
