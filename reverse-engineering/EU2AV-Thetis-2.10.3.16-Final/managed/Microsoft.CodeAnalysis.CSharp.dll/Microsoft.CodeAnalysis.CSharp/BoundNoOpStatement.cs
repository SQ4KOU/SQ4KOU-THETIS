using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNoOpStatement : BoundStatement
{
	public NoOpStatementFlavor Flavor { get; }

	public BoundNoOpStatement(SyntaxNode syntax, NoOpStatementFlavor flavor, bool hasErrors)
		: base(BoundKind.NoOpStatement, syntax, hasErrors)
	{
		Flavor = flavor;
	}

	public BoundNoOpStatement(SyntaxNode syntax, NoOpStatementFlavor flavor)
		: base(BoundKind.NoOpStatement, syntax)
	{
		Flavor = flavor;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNoOpStatement(this);
	}

	public BoundNoOpStatement Update(NoOpStatementFlavor flavor)
	{
		if (flavor != Flavor)
		{
			BoundNoOpStatement boundNoOpStatement = new BoundNoOpStatement(Syntax, flavor, base.HasErrors);
			boundNoOpStatement.CopyAttributes(this);
			return boundNoOpStatement;
		}
		return this;
	}
}
