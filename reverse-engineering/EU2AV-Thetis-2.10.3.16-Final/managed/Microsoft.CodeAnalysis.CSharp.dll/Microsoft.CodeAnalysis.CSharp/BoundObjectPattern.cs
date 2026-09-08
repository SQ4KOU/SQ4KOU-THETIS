using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundObjectPattern : BoundPattern
{
	public Symbol? Variable { get; }

	public BoundExpression? VariableAccess { get; }

	protected BoundObjectPattern(BoundKind kind, SyntaxNode syntax, Symbol? variable, BoundExpression? variableAccess, TypeSymbol inputType, TypeSymbol narrowedType, bool hasErrors = false)
		: base(kind, syntax, inputType, narrowedType, hasErrors)
	{
		Variable = variable;
		VariableAccess = variableAccess;
	}
}
