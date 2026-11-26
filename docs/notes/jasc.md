# JASC / ATA Bölümleri — Notlar

> Sıralama alan bilgisine dayalı tahmindir; Görev 4 Adım 2'de veriyle doğrulanacak.

## Kısa tanım

- **ATA bölümü (iSpec 2200, eski ATA 100):** Uçak sistemlerinin sektör standardı numaralandırması. Biçim `XX-YY-ZZ` (bölüm-alt bölüm-konu). AMM, IPC, MEL ve tech log bu numaraları kullanır. Örnek: `AMM 21-51-00` → klima, soğutma paketleri.
- **JASC (Joint Aircraft System/Component code):** FAA'nın ATA'dan türettiği 4 haneli kod. İlk 2 hane bölüm, son 2 hane alt bölüm. Örnek: `3240` = iniş takımı / tekerlek ve frenler. ATA ile birebir aynı değildir (ör. pistonlu motor bölümleri eklenmiştir).
- SDR'de `JASCCode` %100 doludur; bu yüzden "aynı tür arıza" gruplamasının ilk anahtarıdır.

## Bölüm aileleri

| Aralık | Konu |
|---|---|
| 05–12 | Genel: zaman limitleri, kontroller, servis |
| 21–49 | Gövde sistemleri |
| 51–57 | Yapı |
| 61–65 | Pervane / rotor |
| 71–85 | Motor (güç ünitesi) |

## Sık görülmesi beklenen bölümler

| Bölüm | Ad | Burada ne tür arıza olur? |
|---|---|---|
| 21 | Klima ve basınçlandırma | Pack trip, kabin basıncı kontrol hatası, aşırı sıcak kanal |
| 24 | Elektrik gücü | Jeneratör düşmesi, bus arızası, IDG sorunları |
| 25 | Kabin donanımı | Koltuk, galley, kaçış kaydırağı, kabin içi hasar |
| 26 | Yangın koruma | Yanlış duman/yangın uyarısı, dedektör ve söndürücü arızası |
| 27 | Uçuş kumandaları | Flap asimetrisi, spoiler/aileron arızası, kumanda kablosu aşınması |
| 28 | Yakıt | Sızıntı, yakıt miktar göstergesi hatası, pompa arızası |
| 29 | Hidrolik | Basınç kaybı, sızıntı, pompa arızası |
| 32 | İniş takımı | Fren, lastik, takımın açılmaması/toplanmaması, yönlendirme |
| 52 | Kapılar | Kapı açık uyarısı, kilit/menteşe mekanizması hasarı |
| 53 | Gövde yapısı | Korozyon, çatlak, deri ve çerçeve hasarı |
| 57 | Kanat | Yapısal çatlak, korozyon, bağlantı hasarı |
| 72 | Motor (türbin) | Kanatçık hasarı, titreşim, uçuşta motor kapatma |

Alt bölüm örnekleri (4 hanenin farkı): `3230` açılma/toplanma, `3240` tekerlek ve frenler, `3250` yönlendirme; `2610` algılama, `2620` söndürme; `2750` flaplar.

## Sorular

**1. `0510` neden metin olarak saklanmalı?**
JASC bir sayı değil, bir koddur. Sayıya çevrilince baştaki sıfır düşer ve `510` olur. Bu durumda ilk iki hane `51` (yapı) gibi okunur ve kayıt yanlış bölüme düşer. Bu yüzden `char(4)` olarak saklıyoruz ve CSV okuyucusunda metin olarak alıyoruz (Review Focus #3).

**2. Gruplamada neden 2 hane yerine 4 haneyi tercih ediyoruz?**
2 hane çok geniştir. Örneğin `32` grubunda fren aşınması ile takımın toplanmaması bir arada olur, ama bunlar farklı arızalardır. 4 hane (`3240`, `3230`) aynı alt sistemi gösterir, yani "aynı arıza tekrar ediyor mu" sorusuna daha yakındır. Yine de `ata_chapter`'ı da tutuyoruz, çünkü bölüm seviyesinde özet ve oran hesabı için 2 hane yeterlidir.

**3. SDR verisi bir havayolunun tüm arıza kayıtlarını neden temsil etmez?**
SDR, 14 CFR 121.703 kapsamında yalnızca belirli ciddi olayların raporlanmasıdır: yangın, uçuşta motor kapatma, yapısal çatlak ve korozyon, iniş takımı arızası gibi. Günlük tech log'daki hafif ve sık arızalar (kabin, eğlence sistemi vb.) çoğunlukla raporlanmaz. Bu yüzden SDR'de yapı (53, 57) ve iniş takımı (32) gerçekte olduğundan daha ağırlıklı görünür. Yöntem aynı kalır, ama bir havayolunun kendi verisinde eşikler yeniden ayarlanmalıdır.
