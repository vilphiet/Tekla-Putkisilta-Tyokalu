# Putkisilta Plug-in

## Käännös Visual Studiossa

1. Luo uusi **Class Library (.NET Framework)** -projekti (esim. net48).
2. Lisää viittaukset:
   - Tekla.Structures.dll
   - Tekla.Structures.Model.dll
   - Tekla.Structures.Plugins.dll
   - Tekla.Structures.Geometry3d.dll
   (löytyvät Tekla-asennuksesta `nt\bin\` tai NuGet-paketeista)
3. Kopioi `PutkisiltaPlugin.cs` projektiin.
4. Käännä (Build).
5. Kopioi syntynyt `.dll` kansioon:
   ```
   C:\ProgramData\Trimble\Tekla Structures\<versio>\environments\common\extensions\Putkisilta\
   ```
6. Käynnistä Tekla uudelleen.
7. Etsi **Putkisilta** Applications & components -luettelosta.

## Huomiot

- Testiversio – parametrit kovakoodattuina (sama kuin makrossa).
- Dialogi (profiilivalinnat, mitat) lisätään seuraavassa versiossa.
- Logiikka on identtinen makron kanssa, jotta molempia voidaan kehittää rinnakkain.
