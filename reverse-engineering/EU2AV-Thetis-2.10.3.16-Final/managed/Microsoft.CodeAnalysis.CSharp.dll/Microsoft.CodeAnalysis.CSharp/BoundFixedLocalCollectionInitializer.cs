using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundFixedLocalCollectionInitializer : BoundExpression
{
	protected override ImmutableArray<BoundNode?> Children => ImmutableArray.Create((BoundNode)Expression);

	public new TypeSymbol Type => base.Type;

	public TypeSymbol ElementPointerType { get; }

	public BoundValuePlaceholder? ElementPointerPlaceholder { get; }

	public BoundExpression? ElementPointerConversion { get; }

	public BoundExpression Expression { get; }

	public MethodSymbol? GetPinnableOpt { get; }

	public BoundFixedLocalCollectionInitializer(SyntaxNode syntax, TypeSymbol elementPointerType, BoundValuePlaceholder? elementPointerPlaceholder, BoundExpression? elementPointerConversion, BoundExpression expression, MethodSymbol? getPinnableOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.FixedLocalCollectionInitializer, syntax, type, hasErrors || elementPointerPlaceholder.HasErrors() || elementPointerConversion.HasErrors() || expression.HasErrors())
	{
		ElementPointerType = elementPointerType;
		ElementPointerPlaceholder = elementPointerPlaceholder;
		ElementPointerConversion = elementPointerConversion;
		Expression = expression;
		GetPinnableOpt = getPinnableOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFixedLocalCollectionInitializer(this);
	}

	public BoundFixedLocalCollectionInitializer Update(TypeSymbol elementPointerType, BoundValuePlaceholder? elementPointerPlaceholder, BoundExpression? elementPointerConversion, BoundExpression expression, MethodSymbol? getPinnableOpt, TypeSymbol type)
	{
		if (!TypeSymbol.Equals(elementPointerType, ElementPointerType, TypeCompareKind.ConsiderEverything) || elementPointerPlaceholder != ElementPointerPlaceholder || elementPointerConversion != ElementPointerConversion || expression != Expression || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(getPinnableOpt, GetPinnableOpt) || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundFixedLocalCollectionInitializer boundFixedLocalCollectionInitializer = new BoundFixedLocalCollectionInitializer(Syntax, elementPointerType, elementPointerPlaceholder, elementPointerConversion, expression, getPinnableOpt, type, base.HasErrors);
			boundFixedLocalCollectionInitializer.CopyAttributes(this);
			return boundFixedLocalCollectionInitializer;
		}
		return this;
	}
}
