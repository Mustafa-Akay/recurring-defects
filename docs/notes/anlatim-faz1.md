# Faz 1 anlatımı: kendi cümlelerimle

Mülakattan önce okunacak. Her soruda: kısa cevap (30 saniyede söylenecek), sonra "üstüne sorarlarsa".
İlk oturum: 26 Kasım 2026 (Görev 0-3 ve CI sonrası). Görev 4-6'dan sonra yeni sorular eklenecek.

---

## 1. "Bu projeyi iki cümleyle anlat."

**Kısa cevap:** Uçak bakım raporlarındaki serbest metinden, aynı uçakta ya da aynı filoda tekrar eden arızaları bulan bir sistem. Hedef kullanıcı havayolunun güvenilirlik (reliability) mühendisi: "hangi arıza tekrar ediyor, hangisi kötüleşiyor, nereye müdahale edelim" sorusunu soruyor.

**Üstüne sorarlarsa:**
- *Neden gerçek veri?* İlk projem AeroBag'in en büyük zayıflığı yalnızca simülasyon verisi olmasıydı. FAA SDR herkese açık, gerçek ve dağınık: teknisyenlerin kısaltmalarla yazdığı 376 bin kayıt. Gerçek veride çıkan sorunları (kısaltmalar, çok satırlı metin, eksik alanlar) çözmek, temiz simülasyonda çalışmaktan daha inandırıcı.
- *Sınıflandırma mı yapıyor?* Asıl iş sınıflandırma değil, **tekrar tespiti**: aynı sorunu anlatan raporları gruplayıp tekrar sıklığı bir eşiği (uyarı seviyesi, FAA AC 120-17B) aşınca uyarmak. Ve bunun doğruluğunu elle etiketlenmiş setle **ölçmek**.
- *Bir havayolu için mi yapıldı?* Hayır; herkese açık veriyle yapılmış bir prototip. Tablo yapısı Spec 2000'den esinlendiği için bir havayolunun kendi verisi aynı tabloya eşlenebilir.

## 2. "Aynı dosyayı iki kez içe alırsan ne olur?"

**Kısa cevap:** Hiçbir şey eklenmez. Tabloda `(source, source_ref)` için benzersizlik kuralı var ve içe alma `on conflict do nothing` kullanıyor: kayıt zaten varsa **atlanır** (üzerine yazılmaz). Gerçek veride ölçtüm: ilk seferde 67.654 eklendi, ikinci seferde 0.

**Üstüne sorarlarsa:**
- *Neden önemli?* Bu projede iki kez sayılan bir kayıt, **sahte bir tekrar arızası** demek. Aynı rapor iki kez girerse sistem "bu uçakta bu arıza tekrar etti" der ve yanlış uyarı üretir. Yani idempotent olmamak burada sadece "tablo şişer" değil, sonucu bozar.
- *Ne zaman tekrar çalıştırırsın?* FAA dosyayı güncellediğinde (yeni kayıtlar sonradan eklenir) ya da içe alma yarıda kesildiğinde. Programı korkmadan yeniden çalıştırabilmek gerekiyor.
- *Yarıda kesilirse?* Tüm partiler tek transaction içinde: ya dosyanın tamamı girer ya hiçbiri. Tablo yarım kalmaz.
- *Neden `COPY` değil?* En hızlı yol ama `on conflict` desteklemiyor. 68 bin kayıt 5 saniye; bize hız değil doğruluk lazım.

## 3. "JASC kodu 4 haneli sayı, neden metin sakladın? Tarihte ne sorun çıkabilirdi?"

**Kısa cevap:** JASC bir sayı değil, bir **kod**. Sayıya çevrilirse `0510` → `510` olur ve ilk iki hane (ATA bölümü `05`) kaybolur. Tarihlerde ise SDR ABD biçiminde (ay/gün/yıl) yazılmış; bilgisayar Türkçe olduğu için gün/ay okursa `06/11/2025` 11 Haziran yerine 6 Kasım olur.

**Üstüne sorarlarsa:**
- *Tarih yanlışsa ne olur?* Tekrar tanımımız "30 gün içinde aynı uçakta aynı arıza". Tarih 5 ay kayarsa tekrarlar kaçar ya da olmayan tekrarlar görünür. Bu yüzden `InvariantCulture` ve `MM/dd/yyyy` açıkça yazılı ve testi var.
- *Veride 0'la başlayan JASC var mı?* 2020-2025'te yok. Yine de test ettim; başka yıllarda ya da bir havayolunun verisinde olabilir. Test verisindeki o kaydın yapay olduğunu açıkça not ettim.
- *Genel kural:* Üzerinde aritmetik yapmayacağın "sayı gibi görünen" her şey (posta kodu, telefon, parça no, JASC) metindir.

## 4. "İçe almadan önce neyi kontrol ettin?"

**Kısa cevap:** Tekillik kuralı `OperatorControlNumber`'a dayanıyor. Kontrol ettiğim şey: altı yılın tamamında **farklı iki kayıt aynı numarayı taşıyor mu?** Taşısaydı `on conflict do nothing` ikinciyi hata vermeden atlardı ve veri kaybettiğimizi hiç fark etmezdik. Sonuç: 376.325 kaydın hepsi benzersiz, çakışma 0.

**Üstüne sorarlarsa:**
- *Aynı kaydın iki kez gelmesi sorun mu?* Hayır, o zaten istediğimiz durum (atlanır). Tehlikeli olan **farklı** kayıtların aynı numarayı paylaşması.
- *Ders:* Sistem bir varsayıma dayanıyorsa, varsayımı veriyle doğrula ve sonucunu yaz (`docs/decisions.md`). Aynı kontrolde bozuk tarih ve sayı da aradım: 0.
- *Okuyucu bozuk değere ne yapıyor?* Sessizce `null` yapmıyor; satır numarası ve sütun adıyla hata veriyor. Boş alan ise normal (bilinmiyor) ve `null` oluyor.

## 5. "Testler neden ayrı veritabanında? CI ne işe yarıyor? Neden mock kullanmadın?"

**Kısa cevap:** Testler her çalıştığında tabloyu boşaltıyor; gerçek veritabanında çalışsalar içe aldığım 376 bin kaydı silerlerdi. Bu yüzden kendi `reliability_test` veritabanlarını aynı migration dosyasından kuruyorlar, böylece şema gerçeğiyle aynı kalıyor. CI her push'ta temiz bir GitHub makinesinde derleyip testleri çalıştırıyor: "benim bilgisayarımda çalışıyor" yerine "her yerde çalışıyor" kanıtı.

**Üstüne sorarlarsa:**
- *Neden mock değil?* Test ettiğim davranışların çoğu PostgreSQL'in kendisinde oluyor: `on conflict do nothing`, üretilmiş `ata_chapter` sütunu, `NULL` yazımı, tarih tipi. Sahte bir veritabanı bunları taklit edemez; test geçer ama gerçekte bozuk olabilir. Hız farkı küçük: 14 test 1-7 saniye.
- *CI'da veritabanı nereden geliyor?* `services:` ile yereldekiyle aynı imaj (`pgvector/pgvector:pg17`) açılıyor; adres bir ortam değişkeniyle (`RELIABILITY_TEST_ADMIN`) değişiyor, kod değişmiyor.
- *Güvenlik?* Workflow `permissions: contents: read`: depoya yazamaz. Veritabanı portu yerelde yalnızca `127.0.0.1`'e açık.
- *TDD?* Testleri önce yazdım, kırmızı gördüm, sonra kodu yazdım. Önce kırmızıyı görmek testin gerçekten bir şeyi ölçtüğünü kanıtlar.
