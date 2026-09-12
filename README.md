# SQ4KOU-THETIS

Dedykowane repozytorium projektu **Thetis / SQ4KOU**.

Od 2026-09-12 repozytorium nie przechowuje już gałęzi projektów FLEX-5000, JTDX ani Red Pitaya Protocol 1 / FPGA. Projekty te mają własne repozytoria:

- `SQ4KOU/PowerSDR_FLEX5000` — FLEX-5000 / PowerSDR / Thetis-FLEX5000
- `SQ4KOU/JTDX_SuperHound` — JTDX / SuperHound / OmniRig
- `SQ4KOU/RedPitaya_Protocol_1` — Red Pitaya Protocol 1 / FPGA / ARM / SD

## Aktywne linie Thetis

- `sq4kou` — domyślna główna linia SQ4KOU; FINAL/PASS `ff62e7ad4222a42ee949d9ab1ca736efc5970cef`
- `Thetis-RedPitaya` — baza linii Thetis dla Red Pitaya / ANAN
- `fix/thetis_3z9am` — aktywna linia 3Z9AM / native Ramdor
- `ci/build-thetis_3z9am` — gałąź techniczna buildów 3Z9AM

## Punkty bazowe

- `baseline/sq4kou-final-pass-20260911` — zamrożony FINAL/PASS SQ4KOU
- `baseline/thetis_3z9am-native-ramdor-20260911` — punkt bazowy 3Z9AM
- `baseline/thetis-redpitaya-base-20260911` — punkt bazowy Thetis-RedPitaya
- `base/ramdor-20260904` — referencja upstream Ramdor
- `base/eu2av-20260904` — referencja upstream EU2AV
- `reference/eu2av-2.10.3.16-final-decompiled` — odzyskana referencja 2.10.3.16 Extended Final

## Konwencja gałęzi

- `base/` i `reference/` — źródła referencyjne, nie do bieżącego rozwoju
- `baseline/` — potwierdzone lub zamrożone punkty bazowe
- `feature/` — rozwój nowych funkcji
- `fix/` — poprawki funkcjonalne
- `test/` — eksperymenty/testy
- `build/` i `ci/` — gałęzie techniczne do kompilacji/diagnostyki
- `release/` — linie wydaniowe
- `archive/` — zachowane, ale nieaktywne eksperymenty

## Zasada projektu

Nie mieszamy już niezależnych projektów w jednym repozytorium. FLEX-5000, JTDX oraz Red Pitaya Protocol 1 / FPGA rozwijane są wyłącznie w swoich dedykowanych repozytoriach. `SQ4KOU-THETIS` pozostaje wyłącznie repozytorium Thetis.
