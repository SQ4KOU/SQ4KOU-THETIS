using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundParameterId : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public ParameterSymbol Parameter { get; }

	public FieldSymbol? HoistedField { get; }

	public BoundParameterId(SyntaxNode syntax, ParameterSymbol parameter, FieldSymbol? hoistedField, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ParameterId, syntax, type, hasErrors)
	{
		Parameter = parameter;
		HoistedField = hoistedField;
	}

	public BoundParameterId(SyntaxNode syntax, ParameterSymbol parameter, FieldSymbol? hoistedField, TypeSymbol type)
		: base(BoundKind.ParameterId, syntax, type)
	{
		Parameter = parameter;
		HoistedField = hoistedField;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitParameterId(this);
	}

	public BoundParameterId Update(ParameterSymbol parameter, FieldSymbol? hoistedField, TypeSymbol type)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(parameter, Parameter) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(hoistedField, HoistedField) || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundParameterId boundParameterId = new BoundParameterId(Syntax, parameter, hoistedField, type, base.HasErrors);
			boundParameterId.CopyAttributes(this);
			return boundParameterId;
		}
		return this;
	}
}
