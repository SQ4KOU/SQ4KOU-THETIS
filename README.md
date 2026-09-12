# SQ4KOU-THETIS

Dedykowane repozytorium projektu **Thetis / SQ4KOU**.

Od 2026-09-12 repozytorium przechowuje wyłącznie linie związane z Thetis. Projekty FLEX-5000, JTDX oraz Red Pitaya Protocol 1 / FPGA / ARM są rozwijane w osobnych repozytoriach.

## Główna linia

- `sq4kou` — domyślna i główna linia projektu; oficjalny FINAL/PASS: `ff62e7ad4222a42ee949d9ab1ca736efc5970cef`
- `main` — gałąź kontrolna repozytorium i dokumentacja; nie jest linią produkcyjną Thetis

## Aktywne warianty

- `fix/thetis_3z9am` — aktywna linia 3Z9AM / native Ramdor; obecny punkt bazowy: `34da063d3e58b62cd8a7e204ce04ace26e72220d`
- `ci/build-thetis_3z9am` — techniczna gałąź buildów 3Z9AM
- `Thetis-RedPitaya` — źródłowa linia Thetis Red Pitaya / ANAN używana jako odniesienie dla wariantu 3Z9AM

## Baselines / PASS

- `baseline/sq4kou-final-pass-20260911`
- `baseline/PASS-20260911-diversity-visibility-fix`
- `baseline/PASS-20260911-gpu-tx-rx-history`
- `baseline/thetis_3z9am-native-ramdor-20260911`
- `baseline/thetis-redpitaya-base-20260911`

Baselines są punktami zamrożonymi. Nie wykonujemy na nich bieżących zmian.

## Referencje źródłowe

- `base/ramdor-20260904`
- `base/eu2av-20260904`
- `reference/eu2av-2.10.3.16-final-decompiled`

## Wydania i syntezy

- `release/sq4kou-final-pass-clean-20260911` — czysta, squashowana historia dokładnego drzewa FINAL/PASS SQ4KOU
- `release/db-stable-20260906`
- `release/pa3ghm-tl2-4-final`
- `synthesis-clean`
- `synthesis/pa3ghm-tl2-4-full`

## Archiwum historyczne

37 dawnych gałęzi `build/*`, `test/*`, `tmp/*`, `recovery/*`, starych `feature/*`, starych `fix/*`, checkpointów i nieudanych eksperymentów 3Z9AM zostało 2026-09-12 przeniesionych do tagów:

`archive-20260912/<dawna-nazwa-gałęzi>`

Tag zachowuje dokładny commit dawnej gałęzi, ale nie zaśmieca aktywnej listy branchy.

## Zasady od 2026-09-12

1. `sq4kou` pozostaje stabilną linią produkcyjną i nie służy do eksperymentów.
2. Każda większa zmiana powstaje w krótkotrwałej gałęzi `feature/*` albo `fix/*` utworzonej z właściwego baseline.
3. Po zakończeniu testu gałąź jest albo promowana do linii docelowej, albo archiwizowana tagiem i usuwana.
4. Gałęzie `build/*`, `test/*` i `tmp/*` nie pozostają na stałe w repo.
5. `baseline/*`, `base/*`, `reference/*` i `release/*` są trwałymi punktami odniesienia.
6. Nie mieszamy w tym repo projektów FLEX-5000, JTDX ani FPGA/ARM Protocol 1.

Szczegóły polityki gałęzi i archiwizacji znajdują się w `BRANCH_POLICY.md`.
