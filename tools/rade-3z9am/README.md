# 3Z9AM RADE integration

This integration is pinned to the confirmed 3Z9AM release base:

- base commit: `ab0071751b0fbe006ddad243361a37cfd59bed46`
- base branch: `Thetis-3Z9AM`
- reference MSI: `SQ4KOU-THETIS-3Z9AM-x64.msi`
- RADE vendor: `sv1eia/Thetis-RADE@408f2b5232ff0a2aec9b538a40d4cb1b02627b17`

Hardware-line rule: this directory must not depend on or import code from any other SQ4KOU hardware branch.
The 3Z9AM hardware implementation remains authoritative. Only RADE DSP/UI/vendor integration is applied.

Protected 3Z9AM paths are verified against the base commit during CI.
