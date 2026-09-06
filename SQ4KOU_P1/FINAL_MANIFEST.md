# SQ4KOU Red Pitaya HPSDR Protocol 1 — confirmed FPGA/ARM branch

Base: Pavel Demin `red-pitaya-notes` 20190527, commit `400ee47947795d9d5e54b9975c8ae56b4c6b107f`.

Confirmed FPGA layers: WideBand raw IN2/ADC-B -> EP4, GPS/PPS on E1 DIO3_N with PPS counter/status, and AutoATT overload telemetry for ADC0/IN1 and ADC1/IN2 on status bits 164/165. Existing RX/DDC/Diversity/PureSignal paths are not intentionally altered.

Confirmed integrated WB+GPS FPGA SHA256: `2940919ab6928ef029073c26c9fbb0b6e40bf6692f0cc34c87ba0e65ea89ff39`.
Confirmed AutoATT FPGA SHA256: `66494b64d4abb1b56019383733c9dd9331cc57b402f62d4c5d2bb7d7c3aeef4b`.

ARM layers preserved in source form:
- WB + GPS/PPS/NCO discipline.
- CMD07 GPS_SYNC V2, V8 transport using private `/dev/i2c-0`, `I2C_SLAVE(0x40)`, `write(32)`; V8 ELF SHA256 `2afc54a4f3415b22d1fe1b126f8deb2e0842c24a1f6d6bc14dc96d747180d71f`.
- AutoATT C0=0x20 mapping: C1 bit0=ADC0/IN1 overload, C2 bit0=ADC1/IN2 overload; output ARM SHA256 `9384231994c309d0c26777cf6db0c742ae38950bd261c52e07ec3720aa19a41f` from base ARM `8868bc6a9af2698cc556af012f707a83d6a3eba679c1ed849de33ac5b8cb1407`.

Important: no separately hardware-confirmed single ELF combining the later AutoATT ARM binary and CMD07 V8 binary was produced. Both confirmed transformations are therefore preserved separately; this branch does not invent an unverified merged ELF.

Final proven runtime selector topology for the integrated P1 generation: `1 2 1 1 1` = RX1 IN1, Diversity/RX2 IN2, PureSignal feedback IN1, TX out0 TX, TX out1 TX. WideBand remains an independent passive IN2 tap.
