# SQ4KOU-THETIS — mapa rozwoju

Ten dokument jest kanoniczną mapą pracy w repozytorium `SQ4KOU/SQ4KOU-THETIS`.

## Aktualny stan

Repozytorium ma tylko cztery stałe gałęzie robocze:

| Gałąź | Rola | Aktualny SHA |
|---|---|---|
| `Thetis-RedPitaya` | główna aktywna baza Thetis / Red Pitaya, aktualny FINAL/PASS | `e9c95220f4fab9eb829015a0a0d42dbce6fc45ac` |
| `sq4kou` | domyślna gałąź repo; lustrzany wskaźnik na aktualny FINAL/PASS `Thetis-RedPitaya` | `e9c95220f4fab9eb829015a0a0d42dbce6fc45ac` |
| `Thetis-3Z9AM` | osobna aktywna linia 3Z9AM / native Ramdor | `34da063d3e58b62cd8a7e204ce04ace26e72220d` |
| `main` | dokumentacja, polityka i housekeeping; nie jest bazą binarną Thetis | aktualny HEAD `main` |

## Zasada nadrzędna

Nowe prace SQ4KOU/Red Pitaya zaczynają się zawsze z `Thetis-RedPitaya`. `sq4kou` ma wskazywać ten sam zaakceptowany FINAL/PASS i pozostaje gałęzią domyślną repozytorium.

Linia `Thetis-3Z9AM` jest odrębna. Nie kopiujemy do niej zmian z `Thetis-RedPitaya` bez audytu kodu i zależności 3Z9AM.

## Workflow zmian

1. Start z właściwej gałęzi bazowej: `Thetis-RedPitaya` albo `Thetis-3Z9AM`.
2. Utwórz krótkotrwałą gałąź `feature/...`, `fix/...` albo `test/...` wyłącznie dla jednego zadania.
3. Zbuduj i przetestuj dokładny SHA.
4. Dopiero po potwierdzeniu PASS przenieś zaakceptowany wynik do gałęzi bazowej.
5. Po zakończeniu usuń gałąź roboczą. Jeśli ma wartość historyczną, najpierw zachowaj jej HEAD jako tag `archive-YYYYMMDD/<former-branch-name>`.
6. Nie utrzymujemy stałych gałęzi `baseline/*`, `release/*`, `reference/*`, `build/*`, `test/*` ani `synthesis/*`. Zamrożone punkty są tagami, nie branchami.

## FINAL/PASS

Aktualny, potwierdzony funkcjonalnie FINAL/PASS dla Thetis-RedPitaya/SQ4KOU:

`e9c95220f4fab9eb829015a0a0d42dbce6fc45ac`

Potwierdzony MSI dla tego stanu zachowuje działające GPU waterfall, właściwe menu SETUP/Diversity oraz historię waterfall przy przejściu TX -> RX.

## Archiwum

Historia usuniętych gałęzi nie została utracona. Ich dokładne HEAD-y są zachowane jako lekkie tagi `archive-20260912/...`; szczegóły znajdują się w `ARCHIVE_20260912.md`.

Jeżeli stary README, workflow lub rozmowa przeczy tej mapie, obowiązuje aktualny stan GitHub oraz ten dokument.
