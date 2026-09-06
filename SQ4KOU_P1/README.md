# SQ4KOU P1 – potwierdzone zmiany FPGA + ARM

Baza tej gałęzi: `pavel-demin/red-pitaya-notes` commit `400ee47947795d9d5e54b9975c8ae56b4c6b107f` (20190527), projekt `projects/sdr_transceiver_hpsdr`.

Zakres jest wyłącznie Protocol 1 / Red Pitaya 125-14. Nie ma tu patchy Thetis, ESP32 ani Protocol 2.

## FPGA – potwierdzony wspólny tor

Pakiet `packages/SQ4KOU_AUTOATT_FPGA_ONECLICK_V5.zip` jest dokładnym zestawem źródeł użytym do zbudowania potwierdzonego bitstreamu WB+GPS/PPS+AutoATT. Skrypt `apply_confirmed_fpga_source.py` nakłada tę samą transformację bez uruchamiania Vivado bezpośrednio na drzewo źródłowe gałęzi.

Funkcje:
- WideBand: pasywny raw IN2/ADC-B -> `axi_wb_capture` -> AXI `0x48000000` -> ARM/EP4; 16384 próbki.
- GPS/PPS: pasywny PPS z `E1 DIO3_N` (`exp_n_tri_io[3]`) do bloku WB/GPS.
- AutoATT: pasywny detektor ADC0/IN1 i ADC1/IN2, próg 8064, hold 1,250,000 cykli (10 ms przy 125 MHz), status bity 164/165.
- Zachowane istniejące tory RX/DDC/Diversity/PureSignal/EP6 oraz oryginalne adresy 20190527.

Potwierdzony wynik bitstreamu AutoATT: `SHA256 66494b64d4abb1b56019383733c9dd9331cc57b402f62d4c5d2bb7d7c3aeef4b`.

## ARM – WB + GPS/PPS/NCO

`packages/RP12514_HPSDR_WB_GPS_SD_FINALIZER.zip` zawiera źródła audytowe i deterministyczny mechanizm włączenia działających workerów WB+GPS do ARM. Potwierdzone sprzętowo: discovery/RX, worker WB, PPS seen/valid/recent, GPS LOCK i frequency discipline. Zmierzony w teście zegar: 124999912 Hz (-0.704 ppm).

## ARM – AutoATT

`packages/SQ4KOU_AUTOATT_ARM_ONECLICK_V3.zip` zawiera minimalny, SHA-gated patch handlera EP6; `packages/SQ4KOU_AUTOATT_FINAL_P1_V1.zip` zachowuje końcowy potwierdzony zestaw ARM+FPGA. Mapowanie HPSDR P1:
- `C0=0x20`, `C1 bit0` = ADC0/IN1 overload,
- `C0=0x20`, `C2 bit0` = ADC1/IN2 overload.

Wejście patcha: `8868bc6a9af2698cc556af012f707a83d6a3eba679c1ed849de33ac5b8cb1407`.
Potwierdzony wynik: `9384231994c309d0c26777cf6db0c742ae38950bd261c52e07ec3720aa19a41f`.

## ARM – CMD07 / GPS_SYNC V2

`packages/RP12514_GPS_DUAL_V8_COMPLETE.zip` zawiera źródła audytowe i zweryfikowaną statycznie poprawkę transportu CMD07: własny `/dev/i2c-0`, `I2C_SLAVE(0x40)`, `write(32)`, descriptor-local, jedna transmisja około 1/s. Ramka 32 B: `07 SEQ 5A 02 ... CRC8/ATM`.

Wynik V8: `2afc54a4f3415b22d1fe1b126f8deb2e0842c24a1f6d6bc14dc96d747180d71f`.

### Ważna granica potwierdzenia

AutoATT (`938423...`) i CMD07 V8 (`2afc54...`) są dwoma osobno zweryfikowanymi wariantami ARM. W naszych artefaktach nie ma osobnego, jednoznacznie sprzętowo potwierdzonego binarium będącego ich późniejszym scaleniem. Dlatego ta gałąź zachowuje oba zestawy zmian i oba dokładne artefakty oddzielnie zamiast tworzyć fikcyjny „potwierdzony” wspólny ELF.

## Selektory ARM dla architektury SQ4KOU

Docelowe pięć argumentów: `1 2 1 1 1`:
1. RX1 -> IN1,
2. drugi tor/Diversity -> IN2,
3. PureSignal feedback -> IN1,
4. TX output 0 -> TX,
5. TX output 1 -> TX.

WideBand pozostaje niezależnym pasywnym tapem raw IN2 i nie zależy od argumentu 3.
