# SQ4KOU-THETIS

Dedykowane repozytorium projektu **Thetis / SQ4KOU**.

Od 2026-09-12 repozytorium przechowuje wyłącznie linie związane z Thetis. Projekty FLEX-5000, JTDX oraz Red Pitaya Protocol 1 / FPGA / ARM są rozwijane w osobnych repozytoriach.

## Aktualne stałe gałęzie

- `Thetis-RedPitaya` — główna aktywna linia Thetis / Red Pitaya; aktualny FINAL/PASS: `e9c95220f4fab9eb829015a0a0d42dbce6fc45ac`
- `sq4kou` — domyślna gałąź repozytorium; wskazuje ten sam zaakceptowany FINAL/PASS co `Thetis-RedPitaya`
- `Thetis-3Z9AM` — odrębna aktywna linia 3Z9AM / native Ramdor; obecny punkt bazowy: `34da063d3e58b62cd8a7e204ce04ace26e72220d`
- `main` — dokumentacja, polityka i housekeeping; nie jest linią produkcyjną Thetis

## Zasady

1. Nowe prace Red Pitaya/SQ4KOU zaczynamy z `Thetis-RedPitaya`.
2. Prace 3Z9AM zaczynamy wyłącznie z `Thetis-3Z9AM`.
3. Każda zmiana powstaje w krótkotrwałej gałęzi roboczej i po zakończeniu jest usuwana.
4. Potwierdzone lub historyczne punkty zachowujemy jako tagi, nie jako kolejne stałe branche.
5. `sq4kou` i `Thetis-RedPitaya` mają wskazywać ten sam zaakceptowany FINAL/PASS.
6. Nie kopiujemy zmian pomiędzy `Thetis-RedPitaya` i `Thetis-3Z9AM` bez osobnego audytu kodu.
7. Każdy MSI musi być powiązany z dokładnym SHA źródła.

## Archiwum historyczne

Usunięte gałęzie zostały zachowane jako lekkie tagi:

`archive-20260912/<dawna-nazwa-gałęzi>`

Tag zachowuje dokładny commit dawnej gałęzi, ale nie zaśmieca aktywnej listy branchy.

Szczegóły znajdują się w `DEVELOPMENT_MAP.md`, `BRANCH_POLICY.md` i `ARCHIVE_20260912.md`.
