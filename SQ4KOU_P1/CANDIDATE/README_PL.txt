SQ4KOU P1 FINAL - FPGA + ARM - TEST Z RAM
=========================================

Cel: jeden kandydat Protocol 1 zawierajacy:
- FPGA: WideBand IN2 + GPS/PPS + AutoATT ADC0/ADC1,
- ARM: WB + GPS/PPS/NCO discipline + CMD07 GPS_SYNC V8 + AutoATT C0=0x20.

Pliki docelowe:
FPGA SHA256: 66494b64d4abb1b56019383733c9dd9331cc57b402f62d4c5d2bb7d7c3aeef4b
ARM  SHA256: 11f0f28332d5dcbdcf19a5c4cbb72a9eb98e2d20e346185c287b0363e79820f6

Runtime selector mapping: 1 2 1 1 1.

Pakiet RAM-test laduje oba kandydaty tylko z /tmp, bez zapisu SD. Przy bledzie uruchamia ponownie dzialajaca aplikacje z karty SD.

Po PASS nalezy sprawdzic w Thetis: RX1, Diversity, PureSignal, WideBand, GPS, CMD07/ESP32 i Auto Attenuate.
