using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPropertyAccess : BoundExpression
{
	public override Symbol? ExpressionSymbol => PropertySymbol;

	public new TypeSymbol Type => base.Type;

	public BoundExpression? ReceiverOpt { get; }

	public ThreeState InitialBindingReceiverIsSubjectToCloning { get; }

	public PropertySymbol PropertySymbol { get; }

	public AccessorKind AutoPropertyAccessorKind { get; }

	public override LookupResultKind ResultKind { get; }

	public BoundPropertyAccess(SyntaxNode syntax, BoundExpression? receiverOpt, ThreeState initialBindingReceiverIsSubjectToCloning, PropertySymbol propertySymbol, AccessorKind autoPropertyAccessorKind, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PropertyAccess, syntax, type, hasErrors || receiverOpt.HasErrors())
	{
		ReceiverOpt = receiverOpt;
		InitialBindingReceiverIsSubjectToCloning = initialBindingReceiverIsSubjectToCloning;
		PropertySymbol = propertySymbol;
		AutoPropertyAccessorKind = autoPropertyAccessorKind;
		ResultKind = resultKind;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPropertyAccess(this);
	}

	public BoundPropertyAccess Update(BoundExpression? receiverOpt, ThreeState initialBindingReceiverIsSubjectToCloning, PropertySymbol propertySymbol, AccessorKind autoPropertyAccessorKind, LookupResultKind resultKind, TypeSymbol type)
	{
		if (receiverOpt != ReceiverOpt || initialBindingReceiverIsSubjectToCloning != InitialBindingReceiverIsSubjectToCloning || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(propertySymbol, PropertySymbol) || autoPropertyAccessorKind != AutoPropertyAccessorKind || resultKind != ResultKind || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundPropertyAccess boundPropertyAccess = new BoundPropertyAccess(Syntax, receiverOpt, initialBindingReceiverIsSubjectToCloning, propertySymbol, autoPropertyAccessorKind, resultKind, type, base.HasErrors);
			boundPropertyAccess.CopyAttributes(this);
			return boundPropertyAccess;
		}
		return this;
	}
}
