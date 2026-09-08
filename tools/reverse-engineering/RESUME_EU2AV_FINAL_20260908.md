# Punkt wznowienia — EU2AV Thetis 2.10.3.16 Extended Final
Data: 2026-09-08.

## Zakres
Repozytorium SQ4KOU/SQ4KOU-THETIS, gałąź reference/eu2av-2.10.3.16-final-decompiled.
Kontynuować wyłącznie dekompilację i weryfikację. Nie rozpoczynać integracji z SQ4KOU; nie dotykać FLEX5000.
Użytkownik dopuścił pominięcia/obejścia wyłącznie nieistotnych funkcji, z jawnym udokumentowaniem.

## Stan potwierdzony
- Wynik znajduje się w reverse-engineering/EU2AV-Thetis-2.10.3.16-Final/.
- Przebieg 34253294127 pomyślnie zapisał eksport do gałęzi, commit fb90fcf.
- Git potwierdził 10529 plików eksportu, w tym 10120 plików C#; liczba plików w indeksie odpowiadała liczbie na dysku.
- Roslyn: 10120 plików C#, zero błędów składni. To nie dowodzi kompilowalności ani zgodności semantycznej.
- Eksport obejmuje 58 managed PE, 10 native PE, siedem shaderów waterfall.
- waterfall_postproc.bin ma profil ps_5_0, pozostałe sześć cs_5_0.
- 21 funkcji libSkiaSharp.dll ma listingi asemblerowe zamiast poprawnie odzyskanego pseudokodu. Nie traktować ich jako zdekompilowanych do C.
- STRICT_VERIFICATION nie uzyskał PASS. Ostatnia blokada: brak metadata/SEMANTIC_VERIFICATION.json i rzeczywistych dowodów wymaganych przez ten raport.

## Źródło
tools/reverse-engineering/EU2AV_FINAL_SOURCE_LOCK.json jest obowiązującą blokadą źródła.
Pakiet: https://eu2av.net/download/file.php?id=2070
SHA256 pakietu: abe00caf8e217efa86e6f70994743e4518ba6a7f21a87de8fdace9f8b58ef110
SHA256 Thetis.exe: e25bf32f10b1502c6d84dbb5423a41197117bab7904d03c7c945061ac71c7f76

## Następne prace
1. Pracować na zapisanym eksporcie; nie uruchamiać pełnej dekompilacji dla zmian w weryfikatorze.
2. Zweryfikować C# przez kompilację z właściwymi zależnościami, a IL względem oryginalnych managed PE; zachować diagnostykę.
3. Prześledzić kod ładowania, buforów, dispatch/renderowania GPU i wszystkie siedem shaderów; przygotować dowody zgodności.
4. Przeanalizować każdą z 21 funkcji Skia: adresy w metadata/NATIVE_DECOMPILATION_EXCEPTIONS.csv. Sama nazwa biblioteki nie jest dowodem nieistotności. Obecny opis evidence jest ogólny, wymaga zastąpienia dowodami dla konkretnych funkcji.
5. Kontrola listingów awaryjnych obecnie szuka znaczników w całym chunku; należy powiązać je z blokiem danej funkcji i zweryfikować jego pokrycie.
6. Dopiero z rzeczywistych wyników utworzyć SEMANTIC_VERIFICATION.json i końcowy STRICT_VERIFICATION.json z PASS; nie tworzyć zastępczego PASS.

## Procedura i pułapki
- Naprawiono zduplikowane fragmenty PowerShell spowodowane zamianą tekstu. Przy JS replace używać funkcji zwracającej tekst, aby sekwencje dolarowe nie zmieniały replacement.
- Parser PowerShell sprawdzany przed kosztownymi etapami.
- Globalne ignore *.dll/*.exe pomijały całe katalogi eksportu; zapis używa git add --force wyłącznie dla katalogu eksportu.
- W Windows Git wymaga core.longpaths=true dla długich nazw odzyskanych plików.
- Workflow przywraca pełny eksport z przebiegu 34222077364, artefakt 10055840837 (retencja ograniczona). Trwałą bazą jest teraz Git.
- Workflow może zapisać WORKING_DECOMPILATION mimo nieudanego strict gate, zgodnie z późniejszą prośbą użytkownika o kod do dalszej pracy; czerwony wynik Actions nadal oznacza brak pełnej weryfikacji.
- Automatyczne ponowne przywracanie starego artefaktu może nadpisać późniejsze poprawki rekonstrukcji. Przed zmianami odzyskanego kodu dostosować workflow do pracy na aktualnym Git.
- Monitorowanie Weryfikacja EU2AV Final pozostaje aktywne; ID 6a9fa5b614d881919378bade0efe6a6e. Nie wyłączono go jako sukces, bo strict PASS nie istnieje.

## Linki
- https://github.com/SQ4KOU/SQ4KOU-THETIS/actions/runs/34253294127
- https://github.com/SQ4KOU/SQ4KOU-THETIS/tree/reference/eu2av-2.10.3.16-final-decompiled/reverse-engineering/EU2AV-Thetis-2.10.3.16-Final
