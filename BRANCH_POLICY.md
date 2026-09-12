# SQ4KOU-THETIS branch policy

## Stałe gałęzie

W repozytorium utrzymujemy tylko cztery stałe gałęzie:

- `Thetis-RedPitaya` — główna aktywna linia Thetis / Red Pitaya i źródło kolejnych zmian SQ4KOU. Aktualny FINAL/PASS: `e9c95220f4fab9eb829015a0a0d42dbce6fc45ac`.
- `sq4kou` — gałąź domyślna repozytorium; ma wskazywać ten sam zaakceptowany FINAL/PASS co `Thetis-RedPitaya`.
- `Thetis-3Z9AM` — odrębna aktywna linia 3Z9AM / native Ramdor.
- `main` — wyłącznie dokumentacja, polityka i housekeeping; nie budujemy z niej Thetis.

## Gałęzie tymczasowe

`feature/*`, `fix/*`, `test/*`, `build/*`, `tmp/*`, `recovery/*`, `baseline/*`, `release/*`, `reference/*` i `synthesis/*` nie są stałymi gałęziami.

Po zakończeniu zadania gałąź tymczasowa musi zostać:

1. przeniesiona do właściwej bazy po potwierdzeniu PASS, a następnie usunięta, albo
2. zachowana jako dokładny tag `archive-YYYYMMDD/<former-branch-name>` i usunięta.

## Zamrożone punkty

Baseline, release, referencje upstream i historyczne syntezy przechowujemy jako tagi, nie jako branch. Dzięki temu historia pozostaje dostępna bez zaśmiecania listy gałęzi.

## Reguły bezpieczeństwa

- Przed zmianą stałej gałęzi zawsze weryfikujemy jej dokładny HEAD SHA.
- Nie przesuwamy `Thetis-RedPitaya` ani `sq4kou` bez potwierdzonego wyniku testu użytkownika.
- `sq4kou` i `Thetis-RedPitaya` mają być zsynchronizowane na tym samym zaakceptowanym FINAL/PASS.
- `Thetis-3Z9AM` pozostaje niezależna; nie kopiujemy do niej automatycznie zmian z głównej linii.
- Build/CI nie może samodzielnie zostać nową bazą funkcjonalną.
- Każdy MSI musi być powiązany z dokładnym SHA źródła.
- Nie używamy `main` jako bazy kodu aplikacji.

## Separacja projektów

To repozytorium zawiera wyłącznie Thetis. Inne projekty pozostają w oddzielnych repozytoriach:

- `SQ4KOU/PowerSDR_FLEX5000`
- `SQ4KOU/JTDX_SuperHound`
- `SQ4KOU/RedPitaya_Protocol_1`

## Archiwum

Porządkowanie z 2026-09-12 zachowało usuwane branche jako lekkie tagi `archive-20260912/...`. Pełny wykaz znajduje się w `ARCHIVE_20260912.md`.
