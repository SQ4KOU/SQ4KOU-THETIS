using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

[Flags]
internal enum FlowAnalysisAnnotations
{
	None = 0,
	AllowNull = 1,
	DisallowNull = 2,
	MaybeNullWhenTrue = 4,
	MaybeNullWhenFalse = 8,
	MaybeNull = MaybeNullWhenTrue | MaybeNullWhenFalse,
	NotNullWhenTrue = 0x10,
	NotNullWhenFalse = 0x20,
	NotNull = NotNullWhenTrue | NotNullWhenFalse,
	DoesNotReturnIfFalse = 0x40,
	DoesNotReturnIfTrue = 0x80,
	DoesNotReturn = DoesNotReturnIfFalse | DoesNotReturnIfTrue
}
