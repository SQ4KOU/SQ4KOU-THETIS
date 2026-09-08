using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDisposableValuePlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public new TypeSymbol Type => base.Type;

	public BoundDisposableValuePlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.DisposableValuePlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundDisposableValuePlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.DisposableValuePlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDisposableValuePlaceholder(this);
	}

	public BoundDisposableValuePlaceholder Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundDisposableValuePlaceholder boundDisposableValuePlaceholder = new BoundDisposableValuePlaceholder(Syntax, type, base.HasErrors);
			boundDisposableValuePlaceholder.CopyAttributes(this);
			return boundDisposableValuePlaceholder;
		}
		return this;
	}
}
