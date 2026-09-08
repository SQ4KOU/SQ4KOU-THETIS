# EU2AV Thetis 2.10.3.16 Final — working decompilation

This snapshot is available for code inspection and further reconstruction.
It is not a verified rebuild of the original application and has no full STRICT PASS.

- managed/: recovered C# and IL
- native/: native pseudocode, function indexes and assembly fallbacks
- dxbc/: all seven waterfall shader disassemblies
- metadata/: source provenance, manifests, syntax results and exception register

C# syntax was checked independently. Compilation, IL equivalence, GPU behavior,
and function-specific relevance of 21 Skia fallbacks remain unverified.
The original export README is preserved as EXPORT_README.md.
