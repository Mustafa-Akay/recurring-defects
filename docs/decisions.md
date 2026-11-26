# Kararlar

Her satır: tarih · karar · neden.

- 2026-11-26 · Veritabanı PostgreSQL 17 + pgvector · Kayıtlar ve metin vektörleri (embedding) aynı yerde durur; ayrı bir vektör veritabanı gerekmez. AeroBag'den bilinen teknoloji.
- 2026-11-26 · Port 5434, yalnızca `127.0.0.1` · AeroBag 5433'ü kullanıyor; veritabanı dış ağa açılmasın (AeroBag güvenlik bulgusu #2).
- 2026-11-26 · Tek bir `defect_event` tablosu, Spec 2000 Logbook kaydından esinlenen sütunlar · Kaynak (FAA SDR) değişse de analiz kodu değişmesin; bir havayolunun kendi verisi aynı tabloya eşlenebilsin.
- 2026-11-26 · `unique(source, source_ref)` · Aynı dosya iki kez içe alınırsa kayıt sayısı değişmesin (idempotent içe alma).
- 2026-11-26 · `jasc_code` metin (`char(4)`), sayı değil · `0510` gibi kodlar sayıya çevrilince `510` olur ve bölüm bilgisi bozulur.
- 2026-11-26 · `ata_chapter` üretilmiş (generated) sütun · JASC'in ilk iki hanesi her zaman ATA bölümüdür; elle doldurulursa tutarsız kalabilir.
- 2026-11-26 · İki bileşik indeks: (model, jasc, tarih) ve (tescil, jasc, tarih) · İki ana soruya karşılık gelir: filo seviyesinde tekrar ve aynı uçakta tekrar.
- 2026-11-26 · `embedding vector(384)` · Faz 2'de kullanılacak küçük cümle modelleri (ör. all-MiniLM-L6-v2) 384 boyutlu vektör üretir.
- 2026-11-26 · CSV okuma CsvHelper 34 ile, sütunlar başlık adıyla bulunur · Alan içinde tırnak, virgül ve satır kırılımı var; elle `Split(',')` bunları bozar. Sıra yerine ad: FAA sütun eklerse okuyucu bozulmaz.
- 2026-11-26 · Sayı ve tarih açıkça `InvariantCulture` ile, tarih `MM/dd/yyyy` · Bilgisayar Türkçe; Türkçe ayarda ondalık ayıracı virgül ve tarih gün/ay. "06/11/2025" ABD biçiminde 11 Haziran'dır.
- 2026-11-26 · Boş alan -> null, bozuk değer -> satır no ile hata · Bozuk değeri sessizce null yapmak veri sorununu gizler. 2025 dosyasında hiç bozuk değer yok (67.654 kayıt, 0,5 sn).
- 2026-11-26 · Test verisi gerçek dosyadan 4 kayıt; biri yapay (JASC 5210 -> 0510) · 2025'te 0 ile başlayan JASC kodu yok ama başka yıllarda ya da havayolu verisinde olabilir; baştaki sıfırın korunduğunu yine de test ediyoruz.
- 2026-11-26 · `.gitattributes` ile test CSV'si `-text` · Git'in satır sonu dönüşümü alan içindeki CRLF'yi değiştirip testi anlamsızlaştırmasın.
