using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundValuePlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundExpression.cs", 157);
		}
	}

	public BoundValuePlaceholder(SyntaxNode syntax, TypeSymbol? type, bool hasErrors)
		: base(BoundKind.ValuePlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundValuePlaceholder(SyntaxNode syntax, TypeSymbol? type)
		: base(BoundKind.ValuePlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitValuePlaceholder(this);
	}

	public BoundValuePlaceholder Update(TypeSymbol? type)
	{
		if (!TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundValuePlaceholder boundValuePlaceholder = new BoundValuePlaceholder(Syntax, type, base.HasErrors);
			boundValuePlaceholder.CopyAttributes(this);
			return boundValuePlaceholder;
		}
		return this;
	}
}
