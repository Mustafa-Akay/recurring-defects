# Uyarı Seviyesi (Alert Level) — FAA AC 120-17B Notları

Kaynak: FAA AC 120-17B, *Reliability Program Methods — Standards for Determining Time Limitations*, 19.12.2018. Okunan bölümler: Bölüm 2 (tanımlar), 3 (veri), 4 (performans standartları), 5.1 (analiz).

## 1. Belge ne için var?

Part 121 havayolları bakım aralıklarını (ör. "bu kontrolü 600 saatte bir yap") kendi güvenilirlik programlarıyla ayarlayabilir. AC 120-17B bu programın nasıl kurulacağını anlatır: veri topla → performans standardı koy → sapmayı yakala → araştır → düzelt → bakım programını güncelle.

Bizim proje bu zincirin **"sapmayı yakala"** halkasına ve onu besleyen **"aynı arızayı tanı"** adımına odaklanır.

## 2. Belgenin söylediği (doğrudan)

**Performans standardı (4.1):**
- Sayısal ifade edilir: sayı, oran, yüzde.
- Bir çalışma dönemindeki olay sayısıdır. Dönem uçuş çevrimi (cycle), uçuş saati, çalışma saati ya da takvim zamanı olabilir.
- Kontrol limitleri ve uyarı değerleri "genel kabul görmüş istatistiksel yöntemlere" dayanmalıdır. Belge örnek olarak **standart sapma** ya da **Poisson dağılımı** verir. Ortalama/taban çizgisi yöntemi de kabul edilir.
- Standartlar düzenli gözden geçirilmelidir. Filo yaşı, mevsim ve çevre koşullarına göre ayarlanabilir olmalıdır. Yeni bir tipte yeterli veri birikene kadar ayrı bir izleme yöntemi gerekir.
- Standart belirli bir uçak sistemine bağlanmalıdır. Bizde bu bağı JASC kodu kurar.

**Uyarı seviyesi (4.2.1):**
- Amacı, daha önce kabul edilmiş performanstan anlamlı sapmayı yakalamaktır.
- Çok yüksek olmamalıdır, yoksa büyük artışı kaçırır. Çok düşük de olmamalıdır, yoksa normal dalgalanma sürekli uyarı üretir.
- Seviye, o sistemin arıza oranlarındaki "saçılmaya" (dağılıma) göre belirlenir.
- **Uyarı seviyesi bir uçuşa elverişlilik sınırı değildir.** Sadece "bu normal değil, bak" der. Her uyarı araştırılır ama her uyarı gerçek bir sorun çıkarmaz.
- **Belge tek bir formül vermez.** "Birkaç tanınmış yöntem var, hangisini seçersen programında tam olarak tanımla" der.

**Zaman pencereleri (4.2.2):** Örnek olarak son 1 ay, 3 ay ve 12 ay performansının birlikte izlenmesi verilir. Belgeye göre ölçü birimi seçimi kritik değildir. Önemli olan, birimin program boyunca sabit kalması ve olayın türüne uygun olmasıdır.

**Sapma tespit teknikleri (Tablo 4-1):** Uyarı tabanlı, trend izleme, olay tabanlı ve indeks tabanlı (sistemleri en kötüden iyiye sıralama). Uyarı tabanlı örnekler arasında doğrudan **"chronic aircraft system alerts"** geçer.

**Tekrarlayan arızalar (5.1.1):** Belge şu türleri ayırır:
- **NFF (No Fault Found):** Arıza raporlanmış ama testte tekrarlanamamış.
- **Rogue unit:** Aynı seri numaralı bir LRU'da (hat-değiştirilebilir parça) *aynı* arızanın tekrarlanması.
- **Chronic unit:** Aynı seri numaralı bir LRU'da *farklı* arızaların tekrarlanması.
- **Chronic system/aircraft:** Belirli bir uçakta, **aynı sistem/alt sistemde, operatörün tanımladığı bir süre içinde** tekrarlayan plansız bakım arızaları.

**Veri kaynakları (2 ve 3):** Belge **SDR**'yi ve "repeat items reports"u operasyonel veri olarak açıkça sayar.

## 3. Sektörde yaygın uygulama (belgede değil, ayrı tutulmalı)

Havayolu güvenilirlik programlarında sık görülen uygulama şöyledir:

```
oran_ay   = o aydaki olay sayısı / o ayın uçuş saati × 1000
UCL       = son 12 ayın aylık oranlarının ortalaması + k × standart sapma   (k genelde 2 veya 3)
uyarı     = son ayın oranı UCL'yi aşarsa (bazı programlarda: üst üste 2-3 ay aşarsa)
```

Bu AC 120-17B'nin izin verdiği bir yöntemdir, ama belgenin kendisi bu formülü vermez. Mülakatta "AC 120-17B'de formül bu" dememek gerekir. Doğrusu: **"AC yöntem seçimini operatöre bırakıyor. Biz standart sapma tabanlı yöntemi seçtik ve tanımı şu."**

Olay sayısı düşükse (ör. ayda 0-3 olay), normal dağılım varsayımı zayıflar. Bu durumda belgenin de andığı **Poisson** tabanlı eşik daha uygundur. SDR'de tek bir model + JASC kırılımında aylık sayılar büyük olasılıkla küçük olacak, bu yüzden bu seçim Faz 3'te veriyle yapılmalı.

## 4. Bizim veride ne karşılanıyor?

| AC'nin beklediği | SDR'de var mı? | Not |
|---|---|---|
| Sisteme bağlı olay (JASC/ATA) | ✅ `JASCCode` %100 | Gruplamanın temeli |
| Olay tarihi | ✅ `DifficultyDate` | Aylık seriler kurulabilir |
| Uçak kimliği (chronic aircraft için) | ✅ `RegistryNNumber` %99,5 | "Aynı uçakta tekrar" yapılabilir |
| Filo uçuş saati/çevrimi (oranın paydası) | ❌ Yok | `AircraftTotalTime` uçağın o andaki toplam ömrüdür, aylık filo uçuş saati değildir |
| Parça seri numarası (rogue/chronic unit için) | ❌ Zayıf | `PartNumber` %35,8, seri numarası yok. Parça seviyesi kapsam dışı |
| Tüm arızalar (tech log) | ❌ Kısmen | SDR sadece raporlanması zorunlu ciddi olaylardır (bkz. `jasc.md` Soru 3) |

## 5. Bizim için sonuçlar (kararlar)

1. **Uyarı yöntemi:** Model ailesi + JASC kırılımında aylık olay sayısı serisi. Eşik, son 12 ayın ortalaması + k × standart sapma. Düşük sayılarda Poisson eşiği kullanılır. k ve yöntem Faz 3'te yanlış uyarı ölçümüyle seçilir. Seçim `docs/decisions.md`'ye yazılır.
2. **Payda sorunu:** Filo uçuş saati olmadığı için oran 1000 uçuş saati başına hesaplanamaz. AC birim seçimini serbest bıraktığı için bu bir kural ihlali değildir, ama filo büyümesini kötüleşme sanma riski taşır. Önlem olarak her serinin yanında **raporlayan farklı uçak sayısı** gösterilir ve bölünmüş bir oran (olay / raporlayan uçak) ikinci gösterge olarak hesaplanır. Bu sınır README'de açıkça yazılır.
3. **"Tekrar" tanımı:** Belgedeki **chronic system/aircraft** tanımını temel alıyoruz: aynı tescil + aynı JASC alt sistemi + tanımlı bir süre (taslak: 30 gün, Görev 4'te veriyle kesinleşir). Bizim katkımız, farklı kelimelerle yazılmış aynı arızayı metinden tanımak. JASC kodu farklı girilmiş olsa bile.
4. **Doğru dil:** "AC 120-17B'ye uygun" yerine **"AC 120-17B'nin uyarı tabanlı ve chronic aircraft yaklaşımını izleyen"** ifadesini kullanıyoruz. Paydanın eksik olduğunu da her yerde belirtiyoruz.
