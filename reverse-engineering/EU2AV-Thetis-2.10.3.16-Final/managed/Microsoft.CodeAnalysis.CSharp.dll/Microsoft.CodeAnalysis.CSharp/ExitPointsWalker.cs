using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ExitPointsWalker : AbstractRegionControlFlowPass
{
	private readonly ArrayBuilder<LabelSymbol> _labelsInside;

	private readonly ArrayBuilder<StatementSyntax> _branchesOutOf;

	private ExitPointsWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
		_labelsInside = new ArrayBuilder<LabelSymbol>();
		_branchesOutOf = ArrayBuilder<StatementSyntax>.GetInstance();
	}

	protected override void Free()
	{
		if (_branchesOutOf != null)
		{
			_branchesOutOf.Free();
		}
		_labelsInside.Free();
		base.Free();
	}

	internal static ImmutableArray<StatementSyntax> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
	{
		ExitPointsWalker exitPointsWalker = new ExitPointsWalker(compilation, member, node, firstInRegion, lastInRegion);
		try
		{
			return exitPointsWalker.Analyze();
		}
		finally
		{
			exitPointsWalker.Free();
		}
	}

	private ImmutableArray<StatementSyntax> Analyze()
	{
		bool badRegion = false;
		Scan(ref badRegion);
		if (badRegion)
		{
			return ImmutableArray<StatementSyntax>.Empty;
		}
		_branchesOutOf.Sort((StatementSyntax x, StatementSyntax y) => x.SpanStart - y.SpanStart);
		return _branchesOutOf.ToImmutable();
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		if (base.IsInside)
		{
			_labelsInside.Add(node.Label);
		}
		return base.VisitLabelStatement(node);
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		if (base.IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
			_labelsInside.Add(node.ContinueLabel);
		}
		return base.VisitDoStatement(node);
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		if (base.IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
			_labelsInside.Add(node.ContinueLabel);
		}
		return base.VisitForEachStatement(node);
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		if (base.IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
			_labelsInside.Add(node.ContinueLabel);
		}
		return base.VisitForStatement(node);
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		if (base.IsInside)
		{
			_labelsInside.Add(node.BreakLabel);
		}
		return base.VisitWhileStatement(node);
	}

	protected override void EnterRegion()
	{
		base.EnterRegion();
	}

	protected override void LeaveRegion()
	{
		foreach (PendingBranch item in base.PendingBranches.AsEnumerable())
		{
			if (item.Branch == null || !RegionContains(item.Branch.Syntax.Span))
			{
				continue;
			}
			switch (item.Branch.Kind)
			{
			case BoundKind.GotoStatement:
				if (_labelsInside.Contains(((BoundGotoStatement)item.Branch).Label))
				{
					continue;
				}
				break;
			case BoundKind.BreakStatement:
				if (_labelsInside.Contains(((BoundBreakStatement)item.Branch).Label))
				{
					continue;
				}
				break;
			case BoundKind.ContinueStatement:
				if (_labelsInside.Contains(((BoundContinueStatement)item.Branch).Label))
				{
					continue;
				}
				break;
			case BoundKind.ForEachStatement:
			{
				ForEachEnumeratorInfo enumeratorInfoOpt = ((BoundForEachStatement)item.Branch).EnumeratorInfoOpt;
				if (enumeratorInfoOpt != null && enumeratorInfoOpt.MoveNextAwaitableInfo != null)
				{
					continue;
				}
				goto default;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(item.Branch.Kind);
			case BoundKind.ReturnStatement:
			case BoundKind.YieldBreakStatement:
				break;
			case BoundKind.AwaitExpression:
			case BoundKind.YieldReturnStatement:
			case BoundKind.UsingStatement:
				continue;
			}
			_branchesOutOf.Add((StatementSyntax)item.Branch.Syntax);
		}
		base.LeaveRegion();
	}
}
