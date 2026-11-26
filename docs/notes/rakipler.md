# Rakipler — Ne yapıyorlar, biz neyi yapmıyoruz

26.11.2026'da firmaların kendi sayfalarından okundu. Başarı rakamları firmaların kendi iddialarıdır, bağımsız olarak doğrulanmamıştır.

## Veryon — Defect Analysis (eski adı ATP ChronicX, Veryon AIRE)

- **Ne yapıyor:** Bakım kayıtlarındaki arıza metnini NLP ve makine öğrenmesiyle gruplayıp tekrarlayan ("chronic") arızaları buluyor. Eş anlamlı kelimeleri, yazım hatalarını ve kısaltmaları tanıdığını söylüyor.
- **Önemli iddia:** Gruplama ATA koduna bağlı değil, çünkü teknisyenler aynı arızayı farklı kodlayabiliyor. Tekrarı **ikinci olayda** yakaladığını söylüyor.
- **Ek özellikler:** Kısa ömürlü parçalar, rogue komponentler, mevsimsel trendler.
- **İddia edilen sonuçlar:** Tekrarlayan arızada %33'e, plansız sökümde %20'ye, iptal/gecikmede %10'a varan azalma. Örnek müşterileri American Airlines ve bir bölgesel havayolu.
- **Bizim için anlamı:** Fikrimizin doğrudan ticari karşılığı. Bu iyi bir haber: sorunun gerçek olduğunu ve para ödendiğini gösteriyor.

## EXSYN — Avilytics

- **Ne yapıyor:** Havayolu ve MRO'lar için hazır güvenilirlik yönetimi platformu. Güvenilirlik raporunu otomatik üretiyor, uçak sistemlerinde olası chronic arızaları tespit ediyor, geçmiş veriden arıza trendi tahmin ediyor. Komponent arızası kaynaklı AOG (uçağın yerde kalması) riski için ayrı bir modülü var.
- **Açık nokta:** Uyarı seviyesini hangi yöntemle hesapladığını sayfalarında yazmıyor.
- **Bizim için anlamı:** Güvenilirlik mühendisinin aylık raporu ve uyarı seviyesi tarafı. Panomuzun karşılaştırılacağı yer burası.

## Airbus — Skywise Reliability / Skywise Health Monitoring

- **Ne yapıyor:** Skywise Reliability (2018) güvenilirlik analizi yapıyor: MTBUR (plansız sökümler arası ortalama süre) gibi göstergelerde uyarı ve trend. Skywise Health Monitoring uçaktan gelen sensör verisi ve logbook kayıtlarıyla tekrarları otomatik yakalamaya yönelik.
- **Metin tarafı:** Kabin arızaları için logbook metnini NLP ile inceleyen ayrı bir Skywise uygulaması var. Reliability ürününün metin işleyip işlemediği kamuya açık belgelerde net değil.
- **Bizim için anlamı:** Üreticinin gücü sensör verisi ve tüm filo verisi. Bizde ikisi de yok.

## Ayrıca: Lufthansa Technik — AVIATAR "Technical Repetitives Examination"

Plan listesinde yoktu, aramada çıktı. AVIATAR'ın güvenilirlik paketine eklenen bu yapay zekâ aracı, tüm teknik logbook kayıtlarındaki arıza metinlerini inceleyip tekrarları işaretliyor. Veryon'la birlikte en yakın rakip. THY Teknik ve Pegasus gibi bir MRO bakış açısıyla bakıldığında özellikle önemli.

## Karşılaştırma

| | Veryon | Avilytics | Skywise | AVIATAR | **Biz** |
|---|---|---|---|---|---|
| Arıza metninden tekrar tespiti | ✅ | ? | Kısmen (kabin) | ✅ | ✅ |
| Uyarı seviyesi / trend | ✅ | ✅ | ✅ | ✅ | ✅ |
| Sensör / uçak sağlık verisi | ❌ | ❌ | ✅ | ✅ | ❌ |
| Parça seviyesi (rogue unit) | ✅ | ✅ | ✅ | ? | ❌ |
| Ölçülmüş doğruluğu açık | ❌ | ❌ | ❌ | ❌ | ✅ (hedef) |
| Açık kaynak / açık veri | ❌ | ❌ | ❌ | ❌ | ✅ |

## Biz neyi yapmıyoruz (bilerek)

- Ürün değiliz. Müşteri verisi, entegrasyon, MRO yazılımı bağlantısı (AMOS, TRAX) yok.
- Sensör ve uçak sağlık verisi yok.
- Parça seviyesi (seri numarası) analizi yok. SDR'de bu veri yok.
- Planlı bakım optimizasyonu yok.

## Biz neyi farklı yapıyoruz (mülakat cümlesi)

> "Veryon ve AVIATAR bu sorunu ticari olarak çözüyor. Ama doğruluklarını kamuya açık veriyle ölçmüyorlar. Ben aynı yaklaşımı herkese açık FAA verisiyle kurdum ve elle etiketlenmiş bir set üzerinde ne kadar doğru olduğunu, nerede yanıldığını açıkça ölçtüm. Uyarı yöntemini de AC 120-17B'nin çerçevesine göre tanımladım."

Kaynaklar:
- https://www.veryon.com/chronicx
- https://veryon.com/solutions/business-and-general-aviation/defect-analysis
- https://veryon.com/case-study-american-airlines
- https://exsyn.squarespace.com/products/avilytics
- https://exsyn.squarespace.com/products/avilytics/reliability-analysis
- https://www.airbus.com/en/newsroom/press-releases/2018-10-airbus-presents-its-new-skywise-reliability-analysis-solution-at
- https://www.aircraft.airbus.com/en/newsroom/news/2022-12-intermittent-repetitive-failure
- https://www.dommagazine.com/node/304194
