using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAwaitableValuePlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public new TypeSymbol? Type => base.Type;

	public BoundAwaitableValuePlaceholder(SyntaxNode syntax, TypeSymbol? type, bool hasErrors)
		: base(BoundKind.AwaitableValuePlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundAwaitableValuePlaceholder(SyntaxNode syntax, TypeSymbol? type)
		: base(BoundKind.AwaitableValuePlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAwaitableValuePlaceholder(this);
	}

	public BoundAwaitableValuePlaceholder Update(TypeSymbol? type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundAwaitableValuePlaceholder boundAwaitableValuePlaceholder = new BoundAwaitableValuePlaceholder(Syntax, type, base.HasErrors);
			boundAwaitableValuePlaceholder.CopyAttributes(this);
			return boundAwaitableValuePlaceholder;
		}
		return this;
	}
}
