namespace Microsoft.CodeAnalysis.CSharp;

internal enum NullableAnnotation : byte
{
	NotAnnotated,
	Oblivious,
	Annotated,
	Ignored
}
