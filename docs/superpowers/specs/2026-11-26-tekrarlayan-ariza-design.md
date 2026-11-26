# Tekrarlayan Arıza Tespiti: Tasarım Belgesi

26 Kasım 2026 · Durum: **onaylandı (26 Kasım 2026)** · Çalışma adı henüz yok

> **Çalışma biçimi (kullanıcının şartı):** Proje baştan sona bir öğrenme süreci olarak yürütülür. Her adımda neyi, neden ve nasıl yaptığımız açıklanır. Amaç yalnızca çalışan bir sistem değil, kullanıcının her kararı kendi cümleleriyle savunabilmesi (AeroBag değerlendirmesindeki "her tasarım kararını kendi cümlelerinle savunabilmen gerekir" riski).
>
> **Öğrenme biçimi: karışık (C).** Kullanıcı için yeni olan kısımları (metin temizleme, embedding, pgvector, uyarı seviyesi hesabı) **kullanıcı yazar**, Claude yol gösterir ve inceler. Bilinen kısımları (.NET API, React, Docker) **Claude yazar** ve önemli yerleri anlatır. Her haftanın sonunda "kendi cümlelerinle anlat" oturumu yapılır: kullanıcı o haftanın kararlarını anlatır, Claude eksikleri tamamlar (mülakat provası).

## 1. Amaç ve neden

**Ne:** Gerçek uçak bakım kayıtlarında (FAA Service Difficulty Reports, SDR) farklı kelimelerle yazılmış aynı arızaları gruplayan, uçak tipine göre ayıran ve sektörün kullandığı istatistiksel uyarı seviyesi yöntemiyle kötüleşen arıza kümelerini güvenilirlik mühendisine gösteren bir sistem.

**Neden:** Bu, havayolu sektörüne girmek ve yetkililerle bağlantı kurmak için hazırlanan ikinci portföy projesi. Amaç ürün satmak değil, bakım ve güvenilirlik tarafındaki yetkililere (THY Teknik, Pegasus Teknik vb.) "bu kişi bizim problemimizi anlıyor ve gerçek veride ölçtü" dedirtmek. İlk proje AeroBag (bagaj takibi) yalnızca simülasyon verisiyle çalışıyor. Bu projenin ondan en büyük farkı **gerçek veri üzerinde ölçüm**.

**Öğrenme hedefi:** Kolay yol değil, sektörde gerçekten kullanılan şeyler. Teknoloji bilinen yığından seçilir. Yenilik **havacılık alanının standartları ve yöntemleri** ile **metin benzerliği (embedding)** tarafındadır.

**Kullanıcı:** Güvenilirlik (reliability) mühendisi. Haftalık ya da aylık filo incelemesinde "hangi arızalar tekrar ediyor, hangisi kötüleşiyor, neye müdahale edelim" sorusunu sorar.

## 2. Hedefler

1. SDR kayıtlarını içe almak, dağınık arıza metinlerini temizlemek (kısaltma sözlüğü).
2. Aynı arızayı anlatan kayıtları **uçak tipine** ve **ATA/JASC bölümüne** göre gruplamak (birden fazla tip desteklenir, bir havayolunun karma filosu gibi).
3. Kötüleşen arıza kümeleri için **FAA AC 120-17B** çizgisinde istatistiksel uyarı seviyesi (alert level) hesaplamak.
4. Gruplamanın ve uyarıların doğruluğunu elle etiketlenmiş bir set üzerinde **ölçmek** ve sonuçları sınırlarıyla birlikte dürüstçe yazmak.
5. Güvenilirlik mühendisinin kullanacağı bir pano: tip seçimi → arıza kümeleri → uyarı seviyesi grafiği → örnek kayıtlar.
6. Olay tablosunu **ATA Spec 2000 Bölüm 11** kayıt yapısına göre kurmak. Böylece ileride **parça arızası tahmini / kestirimci bakım** bunun üzerine eklenebilir.

## 3. Hedef olmayanlar (bu ay yapılmayacak)

- Hat teknisyenine anlık uyarı
- Parça arızası tahmin **modelinin** kendisi (veri modeli hazırlanır, model sonraki aşamaya kalır)
- Yapay zekâ sohbet asistanı
- Gerçek bir havayolu sistemiyle (AMOS, Skywise vb.) entegrasyon
- Databricks / Spark gibi büyük veri platformları

## 4. Başarısızlık sayılacak durumlar

- Ölçüm olmadan "yapay zekâ arızaları buluyor" demek.
- SDR'nin sınırlarını (yalnızca bildirilmesi zorunlu ciddi arızalar, çok şirketten gelen veri) gizlemek.
- Bir ayı aşmak ya da altyapıya gömülüp asıl analizi yetiştirememek.
- Uyarının, artış sadece filo büyümesinden kaynaklanıyorsa bile "kötüleşme" demesi (bkz. 9. bölüm).
- Sektör standardı gibi sunulan bir yöntemin standarttan sapması (AC 120-17B'yi okumadan "AC 120-17B'ye uygun" demek).

## 5. Kısıtlar

- Süre: 1 ay, haftada 15-20 saat (toplam ~60-80 saat).
- Tek geliştirici.
- Veri: FAA SDR yıllık CSV dosyaları (herkese açık). 2025 dosyası indirildi (~33 MB, 67.789 satır), henüz incelenmedi.
- ATA Spec 2000 belgesi ücretli. Yalnızca herkese açık açıklamalarından yararlanılır, uyumluluk iddia edilmez.

## 6. Yaklaşımlar

| Yaklaşım | Artısı | Eksisi |
|---|---|---|
| **A. Bilinen yığın: .NET + PostgreSQL/pgvector + Python + React** (seçilen) | Kurulum maliyeti düşük, enerji alana gider, havayolu BT'sinde yaygın, 1 ay korunur | AeroBag'le aynı teknolojiler, teknoloji tarafında yeni beceri az |
| B. Databricks lakehouse (PySpark, Delta, MLflow) | Skywise'a ve Türkiye ilanlarına en yakın | Veri mühendisliği rolüne yönelik, öğrenme bedeli ~10-15 saat |
| C. Açık kaynak veri yığını (Airflow, dbt, FastAPI) | Yerel, bağımsız | Parçaları bağlamak zaman alır |
| D. Python + Streamlit | En hızlısı | "Kolaya kaçmak", sektör görünümü zayıf |

**Seçim: A.** Kullanıcı onayladı (26 Kasım 2026). Gerekçe: Teknoloji bilinen yığından seçilir ki öğrenme enerjisi havacılık alanına ve embedding'e gitsin.

## 7. Kararlaştırılanlar

- **Kullanıcı:** güvenilirlik mühendisi. Hat teknisyeni sonraki aşamaya kaldı.
- **Kapsam:** tek uçak ailesiyle sınırlanmayacak. Birden fazla uçak tipi desteklenir, her şey tipe göre gruplanır. *(Kullanıcının kararı.)*
- **Kestirimci bakıma hazırlık:** Her kayıt standart bir **olay tablosuna** yazılır: uçak tipi, ATA/JASC bölümü, parça numarası (varsa), tarih, arıza kümesi, (varsa) tescil, parça çalışma saati/çevrimi. Yapı ATA Spec 2000 Bölüm 11'in herkese açık kayıt tiplerinden (Logbook, LRU Removal) esinlenir. *(Kullanıcının kararı. Tablo yapısı benim önerim.)*
- **Alan standartları:** ATA/JASC kodları, FAA AC 120-17B uyarı seviyesi yöntemi, ATA Spec 2000 Bölüm 11. *(Kullanıcının "sektörde kullanılan" talebi.)*
- **Teknoloji yığını:**

| Katman | Teknoloji | Görevi |
|---|---|---|
| Veri alma ve API | .NET | SDR içe alma, temizleme, olay tablosuna yazma, panoya veri sunma |
| Veritabanı | PostgreSQL + pgvector | Olay tablosu ve arıza metinlerinin anlam vektörleri |
| Analiz ve NLP | Python (scikit-learn, sentence-transformers) | Gruplama, uyarı seviyesi hesabı, ölçüm |
| Pano | React | Tip → kümeler → uyarı grafiği → örnek kayıtlar |
| Altyapı | Docker Compose, Git + GitHub Actions, testler | Tek komutla açılan demo |

## 8. Yol haritası

| Hafta | İş | Çıktı | Karar noktası |
|---|---|---|---|
| 1 (~15 sa) | Veri keşfi (alanlar, tescil, JASC, metin kalitesi). AC 120-17B'nin uyarı seviyesi bölümünü okuma. ATA Spec 2000 Ch.11 kayıt tiplerini öğrenme. Rakip inceleme (Veryon, Avilytics, Skywise). Ölçüm setinin etiketlenmesi | Veri raporu, kısaltma sözlüğü v1, olay tablosu şeması, ~200-300 etiketli kayıt çifti, uyarı yönteminin yazılı tanımı | Veri beklediğimiz gibi mi? Kapsam burada ayarlanır |
| 2 (~20 sa) | Proje iskeleti (Docker Compose, PostgreSQL + pgvector, .NET içe alma). Metin temizleme. Önce basit gruplama (kelime benzerliği + JASC), sonra embedding. İkisinin ölçülmesi | İçe alınmış çok yıllı veri (örn. 2020-2025), doğruluk / kaçırma tablosu | Basit yöntem yetiyor mu? |
| 3 (~20 sa) | Uyarı seviyesi hesabı ve yanlış uyarı ölçümü. .NET API uçları. React pano | Çalışan pano | — |
| 4 (~15 sa) | Testler ve CI, README (TR + EN), tek sayfalık özet, sınırlar, demo senaryosu ve video, ulaşma mesajı | Gösterime hazır proje | — |

## 9. Uygulayıcıya bırakılanlar ve benim kararlarım

Aşağıdakiler 1. haftada veriye ve AC 120-17B'ye bakınca netleşecek. Parantez içindekiler varsayılan önerimdir:

- **"Tekrar" tanımı:** (aynı uçak tipi + aynı JASC alt bölümü + aynı anlam kümesi). Tescil alanı doluysa aynı uçakta belirli bir süre içinde tekrar da ayrıca işaretlenir.
- **Uyarı seviyesi:** AC 120-17B'den okunacak. (Sektörde yaygın uygulama, geçmiş dönemin ortalamasına standart sapmanın belirli bir katının eklenmesidir. Ama belgeden doğrulanmadan kullanılmaz.) Uçuş saati verisi olmadığı için oran "1000 uçuş saati başına" yerine kayıt sayısı üzerinden hesaplanır. Filo büyümesi etkisini görebilmek için **raporlayan farklı uçak/şirket sayısı** yanında gösterilir.
- **.NET ile Python arasındaki iş bölümü:** (İçe alma, temizleme ve API .NET'te. Embedding üretimi ve ölçüm Python'da. Python sonuçları PostgreSQL'e yazar, .NET okur. Aralarında doğrudan servis çağrısı olmaz.)
- **Embedding modeli:** (Yerelde çalışan küçük bir İngilizce model, örneğin `all-MiniLM-L6-v2`. Dış API kullanılmaz.)
- **Etiketlemeyi kim yapar:** Kullanıcı yapar (~3 saat). Örnek çiftleri araç seçer. Aynı JASC'den benzer ve benzemez çiftler dengeli seçilir.
- **Hangi tipler:** Hepsi işlenir. Ölçüm seti kayıt sayısı en yüksek ~5 tipten örneklenir.
- **Proje yeri:** Bu klasörün altında ayrı bir kod klasörü ve ayrı bir Git deposu.

## 10. Açık riskler

- ~~SDR'de tescil numarası ya da parça çalışma saati alanı boş olabilir.~~ **Ölçüldü (2025 dosyası, 67.654 kayıt):** tescil %99,5, uçak toplam saati %99,1, çevrim %98,4 dolu. Yani "aynı uçakta tekrar" ve uçak saatine göre normalleştirme yapılabilir. Parça çalışma saati ise yalnızca %7,3 dolu, bu yüzden parça seviyesinde kestirimci bakım verisi zayıf. Bu sınır sonraki aşamada dürüstçe yazılacak.
- AC 120-17B uçuş saati bazlı oran bekliyorsa, kayıt sayısı bazlı uyarı yöntemden sapma olur. Bu sapma açıkça yazılır.
- Yeterli kayıt yoksa embedding yöntemi basit yöntemi geçemeyebilir. Bu da dürüstçe yazılacak bir sonuçtur.
- Üç dil (C#, Python, TypeScript) tek kişi ve tek ay için yük oluşturur. 3. haftada zaman daralırsa ilk kesilecek şey panonun kapsamı olur, ölçüm değil.
