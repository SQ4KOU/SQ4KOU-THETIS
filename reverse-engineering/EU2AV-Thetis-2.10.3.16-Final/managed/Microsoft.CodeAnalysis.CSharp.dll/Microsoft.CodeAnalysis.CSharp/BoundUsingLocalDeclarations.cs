using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUsingLocalDeclarations : BoundMultipleLocalDeclarationsBase
{
	public MethodArgumentInfo? PatternDisposeInfoOpt { get; }

	public BoundAwaitableInfo? AwaitOpt { get; }

	public BoundUsingLocalDeclarations(SyntaxNode syntax, MethodArgumentInfo? patternDisposeInfoOpt, BoundAwaitableInfo? awaitOpt, ImmutableArray<BoundLocalDeclaration> localDeclarations, bool hasErrors = false)
		: base(BoundKind.UsingLocalDeclarations, syntax, localDeclarations, hasErrors || awaitOpt.HasErrors() || localDeclarations.HasErrors())
	{
		PatternDisposeInfoOpt = patternDisposeInfoOpt;
		AwaitOpt = awaitOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUsingLocalDeclarations(this);
	}

	public BoundUsingLocalDeclarations Update(MethodArgumentInfo? patternDisposeInfoOpt, BoundAwaitableInfo? awaitOpt, ImmutableArray<BoundLocalDeclaration> localDeclarations)
	{
		if (patternDisposeInfoOpt != PatternDisposeInfoOpt || awaitOpt != AwaitOpt || localDeclarations != base.LocalDeclarations)
		{
			BoundUsingLocalDeclarations boundUsingLocalDeclarations = new BoundUsingLocalDeclarations(Syntax, patternDisposeInfoOpt, awaitOpt, localDeclarations, base.HasErrors);
			boundUsingLocalDeclarations.CopyAttributes(this);
			return boundUsingLocalDeclarations;
		}
		return this;
	}
}
