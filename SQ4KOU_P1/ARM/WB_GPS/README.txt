SOURCE AUDIT - SQ4KOU HPSDR + WB + GPS

These files document the GPS post-link component and the exact merge method used
for the tested ARM binary. They are NOT required on the Red Pitaya SD card.

The exact historical Pavel Demin 20190527 HPSDR source body is intentionally not
substituted by a later source. START_FINALIZE_SD.cmd accepts it only when its
SHA-256 equals:
98d6e8ec7b80936e5eedb46c8bbc2738c7c71c3ecd3a714400c14d7026524b15

The final app ZIP contains one decorated sdr-transceiver-hpsdr.c with the exact
historical body plus an inactive (#if 0) appendix containing the GPS worker
source for audit.
