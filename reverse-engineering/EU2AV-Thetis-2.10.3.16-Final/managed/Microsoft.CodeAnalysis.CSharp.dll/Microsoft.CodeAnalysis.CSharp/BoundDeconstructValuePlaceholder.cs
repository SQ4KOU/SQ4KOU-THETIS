using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDeconstructValuePlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public new TypeSymbol Type => base.Type;

	public Symbol? VariableSymbol { get; }

	public bool IsDiscardExpression { get; }

	public BoundDeconstructValuePlaceholder(SyntaxNode syntax, Symbol? variableSymbol, bool isDiscardExpression, TypeSymbol type, bool hasErrors)
		: base(BoundKind.DeconstructValuePlaceholder, syntax, type, hasErrors)
	{
		VariableSymbol = variableSymbol;
		IsDiscardExpression = isDiscardExpression;
	}

	public BoundDeconstructValuePlaceholder(SyntaxNode syntax, Symbol? variableSymbol, bool isDiscardExpression, TypeSymbol type)
		: base(BoundKind.DeconstructValuePlaceholder, syntax, type)
	{
		VariableSymbol = variableSymbol;
		IsDiscardExpression = isDiscardExpression;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDeconstructValuePlaceholder(this);
	}

	public BoundDeconstructValuePlaceholder Update(Symbol? variableSymbol, bool isDiscardExpression, TypeSymbol type)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(variableSymbol, VariableSymbol) || isDiscardExpression != IsDiscardExpression || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder = new BoundDeconstructValuePlaceholder(Syntax, variableSymbol, isDiscardExpression, type, base.HasErrors);
			boundDeconstructValuePlaceholder.CopyAttributes(this);
			return boundDeconstructValuePlaceholder;
		}
		return this;
	}
}
