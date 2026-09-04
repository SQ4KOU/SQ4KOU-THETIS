# SQ4KOU-THETIS clean synthesis

This branch is the clean synthesis candidate combining the current pinned
source snapshots of `ramdor/Thetis` and `eu2av/OpenHPSDR-Thetis-Enhanced`.

## Stage 1 policy

- no SQ4KOU functional patches;
- no SQ4KOU hardware adaptations;
- preserve upstream functionality unless an audit proves it is hardware-specific,
  conflicting, obsolete, or less reliable than the alternative implementation;
- preserve upstream attribution and licensing;
- x64 buildability is a mandatory gate before further integration work.

## Pinned references

- ramdor/Thetis: `852bf0ef0b4f3886a13fc2846489aee16f361872`
- eu2av/OpenHPSDR-Thetis-Enhanced: `567a7ecd88bfb2ca41316dab9258fff0efeae7bf`

The EU2AV tree is used as the initial technical snapshot because the tree audit
shows that it contains every active Ramdor source path plus additional source
files. The synthesis audit then decides, feature by feature, which implementation
is retained.
