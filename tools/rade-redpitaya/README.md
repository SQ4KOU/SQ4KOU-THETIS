# Thetis-RedPitaya + FreeDV RADE V1/V2

Integration branch: `feature/rade-redpitaya-v1v2`

Immutable RedPitaya base: `e9c95220f4fab9eb829015a0a0d42dbce6fc45ac`.

Pinned RADE reference/vendor: `sv1eia/Thetis-RADE` commit `408f2b5232ff0a2aec9b538a40d4cb1b02627b17` (`v2.10.3.21`).

The integration is deliberately selective. RADE is inserted in ChannelMaster after the WDSP RX audio path and before TXA on the microphone path. The Red Pitaya Protocol 1/2 transport, NetworkIO, PureSignal, Diversity and the SQ4KOU GPU waterfall implementation are protected and are not replaced by SV1EIA files.

`integrate.py` copies only the RADE ChannelMaster glue files from the pinned vendor and patches the small set of required build/splice points. Neural-model sources stay in the pinned submodule so their large generated arrays are preserved bit-for-bit.

The first integration stage provides native V1/V2 RX/TX, independent RX1/RX2 protocol selection, direct ChannelMaster operation without VAC, basic RADE controls/meters, RNNoise/AGC controls and MOX state mirroring. The source-equivalent V1 EOO deferred physical un-key arbiter is a separate validation item and must not be considered complete until hardware TX-release timing is verified on Red Pitaya.
