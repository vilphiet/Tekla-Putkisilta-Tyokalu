# Tekla Putkisilta / Kuljetinsilta -työkalu

Testiversio Tekla Structures Open API -työkalusta, joka mallintaa putkisillan / kuljetinsillan teräsrungon kahden pisteen välille.

## Speksi (lukittu testiversio)

- Käyttäjä valitsee 2 pistettä → keskilinja (oletus keskikorkeus)
- Suorakulmainen poikkileikkaus (leveys × korkeus mm sisänurkkiin)
- Kehäjako: max-jako → tasainen jako ≤ max, päätykehät osuvat pisteisiin
- Paarteet (2 ylä + 2 ala)
- Kehät (pystyt + vaakatuet katossa/lattiassa)
- Seinät: N-ristikko (diagonaalit joka väliin, suunta vaihtuu keskellä)
- Katto & lattia: KT-ristikko (diagonaalit kohtaavat keskellä jokaiseen väliin)
- Vain RHS/SHS-profiilit, vain geometria (ei liitoksia, ei kokoonpanoja)

## Versiot

### 1. Yksinkertainen makro (`.cs`) – helpoin testata
- Tiedosto: `macro/PutkisiltaMacro.cs`
- Kopioi mallikansion `macros\modeling\`-kansioon
- Aja Applications & components -luettelosta
- Parametrit kovakoodattuina tiedoston alussa (muokkaa ennen käyttöä)

### 2. Plug-in (`.dll`) – täysi versio
- Kansio: `plugin/`
- Visual Studio Class Library -projekti
- Kopioi käännetty `.dll` kansioon `environments\common\extensions\`
- Näkyy Applications & components -luettelossa

## Käyttö

1. Avaa Tekla-malli
2. Aja makro / plug-in
3. Valitse kaksi pistettä
4. Työkalu luo rungon

## Huomiot

- Testiversio – parametrit vielä kovakoodattuja makroversiossa
- Profiilit ja mitat muokattavissa koodista
- Ei kokoonpanoja eikä liitoksia tässä versiossa

## Tekijä

Luotu yhteistyössä Grok kanssa speksin perusteella.
