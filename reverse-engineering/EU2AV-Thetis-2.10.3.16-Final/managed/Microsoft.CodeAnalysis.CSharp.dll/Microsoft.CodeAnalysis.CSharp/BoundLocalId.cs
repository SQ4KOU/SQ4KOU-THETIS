using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLocalId : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public LocalSymbol Local { get; }

	public FieldSymbol? HoistedField { get; }

	public BoundLocalId(SyntaxNode syntax, LocalSymbol local, FieldSymbol? hoistedField, TypeSymbol type, bool hasErrors)
		: base(BoundKind.LocalId, syntax, type, hasErrors)
	{
		Local = local;
		HoistedField = hoistedField;
	}

	public BoundLocalId(SyntaxNode syntax, LocalSymbol local, FieldSymbol? hoistedField, TypeSymbol type)
		: base(BoundKind.LocalId, syntax, type)
	{
		Local = local;
		HoistedField = hoistedField;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLocalId(this);
	}

	public BoundLocalId Update(LocalSymbol local, FieldSymbol? hoistedField, TypeSymbol type)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(local, Local) || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(hoistedField, HoistedField) || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundLocalId boundLocalId = new BoundLocalId(Syntax, local, hoistedField, type, base.HasErrors);
			boundLocalId.CopyAttributes(this);
			return boundLocalId;
		}
		return this;
	}
}
