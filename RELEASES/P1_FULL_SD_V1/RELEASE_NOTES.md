# SQ4KOU P1 FULL SD RELEASE V1

Status: **draft — awaiting exact full SD ZIP exported from the verified working card**.

Base: Pavel Demin `red-pitaya-notes` 20190527 (`400ee47947795d9d5e54b9975c8ae56b4c6b107f`).

Confirmed payload:
- FPGA SHA256: `66494b64d4abb1b56019383733c9dd9331cc57b402f62d4c5d2bb7d7c3aeef4b`
- ARM SHA256: `11f0f28332d5dcbdcf19a5c4cbb72a9eb98e2d20e346185c287b0363e79820f6`
- selectors: `1 2 1 1 1`
- Protocol 1
- WideBand raw IN2
- GPS/PPS + NCO discipline
- CMD07/GPS_SYNC V8
- AutoATT ADC0/ADC1
- Diversity/PureSignal topology preserved

The release asset must be generated from the real, hardware-verified SD card. It is intentionally not rebuilt from a different Alpine image.

Public image defaults:
- Ethernet DHCP with fallback `192.168.1.18/24`
- user `root`
- password `changeme`
- SSH host keys and machine IDs are regenerated on first boot

Installation: format a microSD as one FAT32 partition and extract the contents of the release ZIP to the card root.
