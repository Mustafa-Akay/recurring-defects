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
