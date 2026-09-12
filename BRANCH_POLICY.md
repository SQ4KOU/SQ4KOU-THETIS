# SQ4KOU-THETIS branch policy

## Canonical branches

- `sq4kou` — stable production line. Current FINAL/PASS: `ff62e7ad4222a42ee949d9ab1ca736efc5970cef`.
- `main` — repository control/documentation branch only.
- `fix/thetis_3z9am` — active 3Z9AM/native-Ramdor variant.
- `ci/build-thetis_3z9am` — technical build branch for the 3Z9AM variant.
- `Thetis-RedPitaya` — source/reference lineage for Red Pitaya / ANAN related Thetis work.

## Permanent namespaces

- `base/*` — immutable upstream source snapshots.
- `reference/*` — recovered or external reference trees.
- `baseline/*` — verified checkpoints/PASS states.
- `release/*` — release-quality or deliberately frozen product states.
- `synthesis/*` — deliberate long-lived synthesis lines.

## Temporary namespaces

- `feature/*` — one feature only.
- `fix/*` — one correction only, except explicitly designated long-lived variants such as `fix/thetis_3z9am`.
- `test/*`, `build/*`, `tmp/*`, `recovery/*` — temporary only.

Temporary branches must not accumulate. When work ends:

1. promote/merge the accepted result into its canonical branch, or
2. preserve the exact head as a tag `archive-YYYYMMDD/<former-branch-name>`, then delete the branch.

## Safety rules

- Never force-move `sq4kou` without an explicit decision that changes the FINAL/PASS baseline.
- Before modifying a long-lived branch, verify its exact HEAD SHA.
- Preserve known-working functionality; do not rebuild unrelated subsystems as part of a narrow fix.
- Build/CI-only commits must not become the new source baseline by accident.
- Do not use `main` as a source baseline for Thetis binaries.

## Project separation

This repository contains Thetis only. Other project families belong in:

- `SQ4KOU/PowerSDR_FLEX5000`
- `SQ4KOU/JTDX_SuperHound`
- `SQ4KOU/RedPitaya_Protocol_1`

## Archive convention

The 2026-09-12 cleanup preserved 37 transient branch heads as lightweight tags under `archive-20260912/...`. The exact mapping is recorded in `ARCHIVE_20260912.md`.
