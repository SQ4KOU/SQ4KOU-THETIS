using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundRefArrayAccess : BoundExpression
{
	public new TypeSymbol? Type => base.Type;

	public BoundArrayAccess ArrayAccess { get; }

	public BoundRefArrayAccess(SyntaxNode syntax, BoundArrayAccess arrayAccess, bool hasErrors = false)
		: base(BoundKind.RefArrayAccess, syntax, null, hasErrors || arrayAccess.HasErrors())
	{
		ArrayAccess = arrayAccess;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitRefArrayAccess(this);
	}

	public BoundRefArrayAccess Update(BoundArrayAccess arrayAccess)
	{
		if (arrayAccess != ArrayAccess)
		{
			BoundRefArrayAccess boundRefArrayAccess = new BoundRefArrayAccess(Syntax, arrayAccess, base.HasErrors);
			boundRefArrayAccess.CopyAttributes(this);
			return boundRefArrayAccess;
		}
		return this;
	}
}
