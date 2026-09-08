using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundGlobalStatementInitializer : BoundInitializer
{
	public BoundStatement Statement { get; }

	public BoundGlobalStatementInitializer(SyntaxNode syntax, BoundStatement statement, bool hasErrors = false)
		: base(BoundKind.GlobalStatementInitializer, syntax, hasErrors || statement.HasErrors())
	{
		Statement = statement;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitGlobalStatementInitializer(this);
	}

	public BoundGlobalStatementInitializer Update(BoundStatement statement)
	{
		if (statement != Statement)
		{
			BoundGlobalStatementInitializer boundGlobalStatementInitializer = new BoundGlobalStatementInitializer(Syntax, statement, base.HasErrors);
			boundGlobalStatementInitializer.CopyAttributes(this);
			return boundGlobalStatementInitializer;
		}
		return this;
	}
}
