using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundConditionalLoopStatement : BoundLoopStatement
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundExpression Condition { get; }

	public BoundStatement Body { get; }

	protected BoundConditionalLoopStatement(BoundKind kind, SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundExpression condition, BoundStatement body, LabelSymbol breakLabel, LabelSymbol continueLabel, bool hasErrors = false)
		: base(kind, syntax, breakLabel, continueLabel, hasErrors)
	{
		Locals = locals;
		Condition = condition;
		Body = body;
	}
}
