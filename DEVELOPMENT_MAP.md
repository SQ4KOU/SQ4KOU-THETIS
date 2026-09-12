# SQ4KOU-THETIS — mapa rozwoju

Ten dokument jest kanoniczną mapą pracy w repozytorium `SQ4KOU/SQ4KOU-THETIS`.

## Zasada nadrzędna

Nie przenosimy zmian pomiędzy liniami tylko dlatego, że nazwa funkcji jest podobna. Każda zmiana powstaje z właściwej bazy i jest testowana wyłącznie w tej linii. Kod z `sq4kou` nie jest kopiowany do `thetis_3z9am` bez ponownego audytu na źródłach 3Z9AM i odwrotnie.

## Mapa gałęzi

| Cel | Gałąź | Rola | Z czego tworzyć nowe zmiany |
|---|---|---|---|
| SQ4KOU produkcja | `sq4kou` | główna aktywna linia SQ4KOU | zawsze z `sq4kou` |
| SQ4KOU FINAL/PASS | `baseline/sq4kou-final-pass-20260911` | zamrożony punkt odniesienia | nie rozwijać bezpośrednio |
| SQ4KOU clean release | `release/sq4kou-final-pass-clean-20260911` | czysty checkpoint wydaniowy | nie używać do eksperymentów |
| 3Z9AM aktywna | `fix/thetis_3z9am` | jedyna aktywna linia funkcjonalna 3Z9AM | zawsze z `fix/thetis_3z9am` |
| 3Z9AM build/CI | `ci/build-thetis_3z9am` | techniczna gałąź budowania | nie dodawać zmian funkcjonalnych |
| 3Z9AM baseline | `baseline/thetis_3z9am-native-ramdor-20260911` | zamrożony punkt odniesienia 3Z9AM | nie rozwijać bezpośrednio |
| Thetis-RedPitaya | `Thetis-RedPitaya` | baza/linia referencyjna Red Pitaya / ANAN | tylko gdy zadanie dotyczy tej linii |
| Thetis-RedPitaya baseline | `baseline/thetis-redpitaya-base-20260911` | zamrożona baza | nie rozwijać bezpośrednio |
| Ramdor upstream | `base/ramdor-20260904` | referencja upstream | tylko porównania / cherry-pick po audycie |
| EU2AV upstream | `base/eu2av-20260904` | referencja upstream | tylko porównania / cherry-pick po audycie |
| EU2AV recovered Final | `reference/eu2av-2.10.3.16-final-decompiled` | referencja odzyskanego Extended Final | źródło porównań GPU/SETUP, nie gałąź robocza |
| DB stable | `release/db-stable-20260906` | zamknięty stabilny checkpoint | nie rozwijać bezpośrednio |
| PA3GHM final | `release/pa3ghm-tl2-4-final` | zamknięta linia wydaniowa | nie mieszać z bieżącym SQ4KOU/3Z9AM |
| PA3GHM synthesis | `synthesis/pa3ghm-tl2-4-full` | historyczna synteza | tylko referencja |
| Clean synthesis | `synthesis-clean` | czysta synteza Ramdor + EU2AV | tylko referencja / odrębny eksperyment |
| Repo control | `main` | dokumentacja, polityka, housekeeping | nigdy nie budować Thetis z `main` |

## Workflow SQ4KOU

1. Start z `sq4kou`.
2. Utwórz krótkotrwałą gałąź `feature/sq4kou-<temat>` albo `fix/sq4kou-<temat>`.
3. Build/test odbywa się na tej gałęzi.
4. Po potwierdzeniu PASS: merge/fast-forward do `sq4kou` i utworzenie nowego `baseline/PASS-YYYYMMDD-<temat>` tylko gdy zmiana jest rzeczywiście zaakceptowana.
5. Gałąź testowa po zakończeniu jest usuwana; jeśli ma wartość historyczną, najpierw tag `archive-YYYYMMDD/<stara-nazwa>`.

## Workflow 3Z9AM

1. Start wyłącznie z `fix/thetis_3z9am`.
2. Utwórz krótkotrwałą gałąź `feature/3z9am-<temat>` albo `fix/3z9am-<temat>`.
3. Zmianę implementować po audycie kodu 3Z9AM; nie kopiować automatycznie implementacji z `sq4kou`.
4. Build wykonywać z dokładnego SHA gałęzi roboczej; `ci/build-thetis_3z9am` służy tylko jako mechanizm CI.
5. Po PASS zmiana trafia do `fix/thetis_3z9am`; po potwierdzonym stabilnym etapie można utworzyć nowy `baseline/thetis_3z9am-...`.
6. Nieudane testy nie pozostają jako branch — tag archiwalny i usunięcie gałęzi.

## Reguły dla buildów i testów

- `build/*`, `test/*` i `tmp/*` są zawsze krótkotrwałe.
- Każdy MSI musi być powiązany z dokładnym SHA źródła.
- Workflow CI nie może zmieniać kodu funkcjonalnego.
- Przed publikacją MSI należy sprawdzić, że checkout workflowu wskazuje dokładnie oczekiwaną gałąź/SHA.
- Po zakończeniu testu gałąź techniczna jest usuwana lub archiwizowana tagiem.

## Reguły PASS / baseline

- `baseline/*` jest niemodyfikowalnym punktem odniesienia.
- `PASS` oznacza stan potwierdzony testem, nie tylko poprawny build.
- Nie przesuwamy istniejącego baseline do innego SHA.
- Nowy stabilny etap dostaje nowy baseline zamiast nadpisywania starego.

## Reguły bezpieczeństwa

- `sq4kou` i `fix/thetis_3z9am` nigdy nie są force-pushowane podczas zwykłej pracy.
- `main` nie jest źródłem aplikacji.
- Referencje `base/*`, `reference/*`, `baseline/*` i `release/*` nie służą do eksperymentów.
- Przed przeniesieniem zmiany między liniami trzeba porównać strukturę kodu, eventy, zależności i workflow builda.
- Jeśli implementacja działa w jednej linii, nie zakładamy automatycznie zgodności z drugą.

## Najważniejsze aktualne punkty

- `sq4kou` FINAL/PASS: `ff62e7ad4222a42ee949d9ab1ca736efc5970cef`
- `fix/thetis_3z9am`: `34da063d3e58b62cd8a7e204ce04ace26e72220d`
- `Thetis-RedPitaya`: `de2dc20308c26fe30bba5b3d0b4e2e7a8d61c9c5`

Jeśli README, rozmowa lub stary workflow przeczy tej mapie, obowiązuje `DEVELOPMENT_MAP.md` oraz aktualnie zweryfikowany stan GitHub.
