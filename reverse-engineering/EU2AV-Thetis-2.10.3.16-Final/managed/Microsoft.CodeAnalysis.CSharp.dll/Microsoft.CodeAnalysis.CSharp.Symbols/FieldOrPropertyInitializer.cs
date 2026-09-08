namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal readonly struct FieldOrPropertyInitializer(FieldSymbol fieldOpt, SyntaxNode syntax)
{
	internal readonly FieldSymbol FieldOpt = fieldOpt;

	internal readonly SyntaxReference Syntax = syntax.GetReference();
}
