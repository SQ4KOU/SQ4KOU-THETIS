from pathlib import Path

DISPLAY = Path("Project Files/Source/Console/display.cs")
MARKER = "SQ4KOU_DXGI_EACCESSDENIED_FALLBACK"


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig")


def write(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8-sig", newline="")


text = read(DISPLAY)

if MARKER in text:
    required = [
        "unchecked((int)0x80070005)",
        "desc.SwapEffect != SwapEffect.Discard",
        "desc.SwapEffect = SwapEffect.Discard",
        "desc.BufferCount = 1",
        "_bUseLegacyBuffers = true",
    ]
    missing = [item for item in required if item not in text]
    if missing:
        raise RuntimeError("DirectX fallback marker exists but implementation is incomplete: " + ", ".join(missing))
    print("DIRECTX_EACCESSDENIED_FALLBACK=ALREADY_PRESENT")
    raise SystemExit(0)

old = """                    _factory1.MakeWindowAssociation(displayTarget.Handle, WindowAssociationFlags.IgnoreAll);\n\n                    _swapChain = new SwapChain(_factory1, _device, desc);\n                    _swapChain1 = _swapChain.QueryInterface<SwapChain1>();\n"""

new = """                    _factory1.MakeWindowAssociation(displayTarget.Handle, WindowAssociationFlags.IgnoreAll);\n\n                    // SQ4KOU_DXGI_EACCESSDENIED_FALLBACK\n                    // Keep the normal flip-model path. Some Windows/driver combinations reject\n                    // DXGI CreateSwapChain with E_ACCESSDENIED (0x80070005). In that exact case\n                    // retry once with the already-supported legacy bitblt swap effect. No other\n                    // DirectX/GPU-waterfall failure is hidden by this fallback.\n                    try\n                    {\n                        _swapChain = new SwapChain(_factory1, _device, desc);\n                    }\n                    catch (SharpDX.SharpDXException ex)\n                        when (ex.HResult == unchecked((int)0x80070005) && desc.SwapEffect != SwapEffect.Discard)\n                    {\n                        Debug.WriteLine(\"DXGI CreateSwapChain returned E_ACCESSDENIED; retrying legacy Discard swap chain.\");\n                        _bUseLegacyBuffers = true;\n                        bFlipPresent = false;\n                        swapEffect = SwapEffect.Discard;\n                        _nBufferCount = 1;\n                        desc.SwapEffect = SwapEffect.Discard;\n                        desc.BufferCount = 1;\n                        desc.Flags = SwapChainFlags.None;\n                        _swapChain = new SwapChain(_factory1, _device, desc);\n                    }\n                    _swapChain1 = _swapChain.QueryInterface<SwapChain1>();\n"""

count = text.count(old)
if count != 1:
    raise RuntimeError(f"CreateSwapChain anchor mismatch: expected 1, found {count}")

text = text.replace(old, new, 1)
write(DISPLAY, text)

verify = read(DISPLAY)
for token in (
    MARKER,
    "unchecked((int)0x80070005)",
    "desc.SwapEffect != SwapEffect.Discard",
    "desc.SwapEffect = SwapEffect.Discard",
    "desc.BufferCount = 1",
    "_bUseLegacyBuffers = true",
    "SwapEffect.FlipDiscard",
    "DetectGPUCapabilitiesFromD2D();",
):
    if token not in verify:
        raise RuntimeError(f"Post-patch verification failed: {token}")

print("DIRECTX_EACCESSDENIED_FALLBACK=PASS")
