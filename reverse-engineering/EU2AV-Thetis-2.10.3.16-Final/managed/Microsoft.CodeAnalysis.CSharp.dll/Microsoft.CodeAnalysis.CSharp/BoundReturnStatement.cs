using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundReturnStatement : BoundStatement
{
	public RefKind RefKind { get; }

	public BoundExpression? ExpressionOpt { get; }

	public bool Checked { get; }

	public static BoundReturnStatement Synthesized(SyntaxNode syntax, RefKind refKind, BoundExpression expression, bool hasErrors = false)
	{
		return new BoundReturnStatement(syntax, refKind, expression, hasErrors)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundReturnStatement(SyntaxNode syntax, RefKind refKind, BoundExpression? expressionOpt, bool @checked, bool hasErrors = false)
		: base(BoundKind.ReturnStatement, syntax, hasErrors || expressionOpt.HasErrors())
	{
		RefKind = refKind;
		ExpressionOpt = expressionOpt;
		Checked = @checked;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitReturnStatement(this);
	}

	public BoundReturnStatement Update(RefKind refKind, BoundExpression? expressionOpt, bool @checked)
	{
		if (refKind != RefKind || expressionOpt != ExpressionOpt || @checked != Checked)
		{
			BoundReturnStatement boundReturnStatement = new BoundReturnStatement(Syntax, refKind, expressionOpt, @checked, base.HasErrors);
			boundReturnStatement.CopyAttributes(this);
			return boundReturnStatement;
		}
		return this;
	}
}
