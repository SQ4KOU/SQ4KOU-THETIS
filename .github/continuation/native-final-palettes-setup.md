# SQ4KOU native Final palettes and GPU Setup continuation

Destination: SQ4KOU/SQ4KOU-THETIS, feature/eu2av-gpu-waterfall-sq4kou. User explicitly authorized publication and MSI build, and requires all GPU menu controls/events to match recovered Final.

Source baseline a423339defae21e3ba149ee0bda8bc6f3d62cafe. Recovered palette/UI reference: .github/recovered/eu2av-2.10.3.16.

Implemented:
- Exact recovered Oklab WaterfallPalette, Enhanced256=12 and Grayscale256=13; existing enum identities retained. RX1/RX2/TX lists include all native 256 palettes before saved options load. CPU and GPU native LUT routing; no old GPU palette alias or approximation.
- Exact Final control construction in Waterfall Pro, General Waterfall and DirectX Quality groups, including choices/ranges/defaults (native default depth is 16-bit), labels, location, visibility and 31 event connections.
- All 31 event handler bodies match the recovered Final after whitespace normalization. Post-load SyncWaterfallEnhancerFromControls also matches Final exactly.
- Keep existing SQ4KOU ApplyGPUSelection and retry timer internals; these handle device availability safely and must not be replaced with Final's older startup policy.

Critical correction discovered during verification:
- The tested NOGHOST/8-16-bit MSI used generated changes from commit 93783094c9f89f1b5bf8cc356f68616ce2be44d9 in fix/gpu-waterfall-fallback-runtime. Those changes were NOT present in a423339d source. The initial local source-only preservation check could not prove they were retained.
- The exact prior shader-load and color-depth build patch is now materialized in source: seven embedded shaders with resource-first loaders; existing resizeDX2D cloned only for its format argument; in-place ResizeBuffers before full rebuild fallback. Existing E_ACCESSDENIED fallback and GPU retry remain.
- NOGHOST is retained exactly: clear old renderer history on the first valid row of a fresh sequence; GPU-only temporal persistence remains 0. The Temporal UI event matches Final and stores its state, but GPU history blending remains disabled by this explicit protected fix.
- Original tested workflow and patch preserved at .github/recovered/sq4kou-noghost-93783094 for deterministic verification.

Tests:
- .github/scripts/fix-gpu-waterfall-palettes.py verifies exact Final palette source, static Display properties, native enum/list wiring, 31 event connections/bodies, post-load synchronization, and the exact preserved runtime patch against its sources.
- .github/scripts/test-final-palettes.ps1 compares 1,310 compiled palette samples against recovered Final, including all 256 indices, NaN/infinities/bounds. Run by Windows CI before full MSI build.
- Workflow builds the checked-out commit without modifying source, verifies all seven resources in both built Thetis.exe and the executable extracted from MSI.
- Runtime on the user's radio/GPU is not available here. No blanket claim of live hardware validation.

Remaining outside GPU menu:
- Detach Panafall is absent in SQ4KOU UI and backend. It requires native DetachedPanafallForm/DetachedPanafallRenderer plus Console and Display lifecycle integration. Do not add a dead checkbox or claim entire SETUP equals Final 1:1.
