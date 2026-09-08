# EU2AV Thetis 2.10.3.16 Extended Final - reverse-engineering reference

This directory is a reconstructed/decompiled reference snapshot of the publicly distributed EU2AV Thetis 2.10.3.16 Extended Final package. It is not the original EU2AV source tree and does not claim to recover original comments, identifiers lost during compilation, build scripts, or exact original C/C++/HLSL text.

## Provenance

- Distribution URL: https://eu2av.net/download/file.php?id=2070
- Downloaded package SHA-256: abe00caf8e217efa86e6f70994743e4518ba6a7f21a87de8fdace9f8b58ef110
- Downloaded package size: 75987169 bytes
- Thetis.exe SHA-256: e25bf32f10b1502c6d84dbb5423a41197117bab7904d03c7c945061ac71c7f76

Exact per-file hashes and installed paths are in metadata/MANIFEST.csv. Tool versions are in metadata/PROVENANCE.txt.

## Coverage

- Managed PE assemblies discovered: 58. Every managed assembly is passed through ILSpy. managed/ contains reconstructed C# project/source output and an IL dump when supported.
- Native PE modules discovered: 10. Every native EXE/DLL is analyzed by Ghidra. native/<module>/ contains chunked C-like decompiler output, functions.csv, decompile-stats.txt, and PE metadata when dumpbin is available.
- Waterfall DXBC shaders discovered: 7. dxbc/ contains actual DXBC bytecode disassembly from FXC or DXC, not original HLSL.
- Full installed payload inventory: metadata/MANIFEST.csv.
- Per-native-module Ghidra status: metadata/NATIVE_DECOMPILE_STATUS.csv.
- Tool logs: logs/.

## Interpretation rule

Use this branch as a binary-grounded implementation reference. Managed C# is decompiler reconstruction; Ghidra output is pseudocode; DXBC files are bytecode disassembly. For exact behavior, cross-check reconstructed control flow with IL, PE metadata, shader disassembly, hashes, and runtime tests.

## GPU-waterfall components required by the verification gate

The pipeline requires successful recovery of Thetis.exe, ChannelMaster.dll, wdsp.dll and the seven EU2AV waterfall shaders: bit-reverse, magnitude, stage A/B, stage B/A, postprocess, resolve and row.

The branch is intentionally isolated from sq4kou and is for reference/audit only.
