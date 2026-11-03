# Putkisilta Plug-in (täysi versio)

## Ominaisuudet
- Dialogi parametreille (leveys, korkeus, max-jako, materiaali, korkeusasema)
- Profiilivalinnat kaikille osille (pudotusvalikko + vapaa kirjoitus)
- Päätykehille erilliset profiilit
- Korkeusasema: keskikorkeus / alapinta / yläpinta
- N-ristikko seinissä, KT-ristikko katossa ja lattiassa

## Käännös
1. Luo Visual Studiossa **Class Library (.NET Framework)** -projekti.
2. Lisää viittaukset:
   - Tekla.Structures.dll
   - Tekla.Structures.Model.dll
   - Tekla.Structures.Plugins.dll
   - Tekla.Structures.Geometry3d.dll
   - Tekla.Structures.Dialog.dll
3. Lisää tiedostot projektiin:
   - PutkisiltaPlugin.cs
   - PutkisiltaForm.cs
   - PutkisiltaForm.Designer.cs
4. Käännä.
5. Kopioi `.dll` kansioon:
   ```
   C:\ProgramData\Trimble\Tekla Structures\<versio>\environments\common\extensions\Putkisilta\
   ```
6. Käynnistä Tekla uudelleen.
7. Etsi **Putkisilta** Applications & components -luettelosta.

## Käyttö
1. Avaa dialogi.
2. Aseta mitat ja profiilit.
3. Paina OK / Käytä.
4. Valitse kaksi pistettä mallista.
5. Silta generoituu.
