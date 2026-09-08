using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundImplicitIndexerValuePlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundExpression.cs", 197);
		}
	}

	public new TypeSymbol Type => base.Type;

	public BoundImplicitIndexerValuePlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ImplicitIndexerValuePlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundImplicitIndexerValuePlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ImplicitIndexerValuePlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitImplicitIndexerValuePlaceholder(this);
	}

	public BoundImplicitIndexerValuePlaceholder Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundImplicitIndexerValuePlaceholder boundImplicitIndexerValuePlaceholder = new BoundImplicitIndexerValuePlaceholder(Syntax, type, base.HasErrors);
			boundImplicitIndexerValuePlaceholder.CopyAttributes(this);
			return boundImplicitIndexerValuePlaceholder;
		}
		return this;
	}
}
