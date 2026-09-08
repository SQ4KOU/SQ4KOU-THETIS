using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class VariablesDeclaredWalker : AbstractRegionControlFlowPass
{
	private HashSet<Symbol> _variablesDeclared = new HashSet<Symbol>();

	internal static IEnumerable<Symbol> Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
	{
		VariablesDeclaredWalker variablesDeclaredWalker = new VariablesDeclaredWalker(compilation, member, node, firstInRegion, lastInRegion);
		try
		{
			bool badRegion = false;
			variablesDeclaredWalker.Analyze(ref badRegion);
			IEnumerable<Symbol> result;
			if (!badRegion)
			{
				IEnumerable<Symbol> variablesDeclared = variablesDeclaredWalker._variablesDeclared;
				result = variablesDeclared;
			}
			else
			{
				result = SpecializedCollections.EmptyEnumerable<Symbol>();
			}
			return result;
		}
		finally
		{
			variablesDeclaredWalker.Free();
		}
	}

	internal VariablesDeclaredWalker(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion)
		: base(compilation, member, node, firstInRegion, lastInRegion)
	{
	}

	protected override void Free()
	{
		base.Free();
		_variablesDeclared = null;
	}

	public override void VisitPattern(BoundPattern pattern)
	{
		NoteDeclaredPatternVariables(pattern);
		base.VisitPattern(pattern);
	}

	protected override void VisitSwitchSection(BoundSwitchSection node, bool isLastSection)
	{
		foreach (BoundSwitchLabel switchLabel in node.SwitchLabels)
		{
			NoteDeclaredPatternVariables(switchLabel.Pattern);
		}
		base.VisitSwitchSection(node, isLastSection);
	}

	private void NoteDeclaredPatternVariables(BoundPattern pattern)
	{
		if (!(pattern is BoundDeclarationPattern boundDeclarationPattern))
		{
			if (!(pattern is BoundRecursivePattern boundRecursivePattern))
			{
				if (!(pattern is BoundITuplePattern boundITuplePattern))
				{
					if (!(pattern is BoundListPattern boundListPattern))
					{
						if (!(pattern is BoundConstantPattern boundConstantPattern))
						{
							if (!(pattern is BoundRelationalPattern boundRelationalPattern))
							{
								if (!(pattern is BoundNegatedPattern boundNegatedPattern))
								{
									if (!(pattern is BoundSlicePattern boundSlicePattern))
									{
										if (pattern is BoundDiscardPattern || pattern is BoundTypePattern)
										{
											return;
										}
										if (!(pattern is BoundBinaryPattern))
										{
											throw ExceptionUtilities.UnexpectedValue(pattern.Kind);
										}
										BoundBinaryPattern boundBinaryPattern = (BoundBinaryPattern)pattern;
										if (!(boundBinaryPattern.Left is BoundBinaryPattern))
										{
											NoteDeclaredPatternVariables(boundBinaryPattern.Left);
											NoteDeclaredPatternVariables(boundBinaryPattern.Right);
											return;
										}
										ArrayBuilder<BoundBinaryPattern> instance = ArrayBuilder<BoundBinaryPattern>.GetInstance();
										do
										{
											instance.Push(boundBinaryPattern);
											boundBinaryPattern = boundBinaryPattern.Left as BoundBinaryPattern;
										}
										while (boundBinaryPattern != null);
										boundBinaryPattern = instance.Pop();
										NoteDeclaredPatternVariables(boundBinaryPattern.Left);
										do
										{
											NoteDeclaredPatternVariables(boundBinaryPattern.Right);
										}
										while (instance.TryPop(out boundBinaryPattern));
										instance.Free();
									}
									else if (boundSlicePattern.Pattern != null)
									{
										NoteDeclaredPatternVariables(boundSlicePattern.Pattern);
									}
								}
								else
								{
									NoteDeclaredPatternVariables(boundNegatedPattern.Negated);
								}
							}
							else
							{
								VisitRvalue(boundRelationalPattern.Value);
							}
						}
						else
						{
							VisitRvalue(boundConstantPattern.Value);
						}
					}
					else
					{
						foreach (BoundPattern subpattern in boundListPattern.Subpatterns)
						{
							NoteDeclaredPatternVariables(subpattern);
						}
						noteOneVariable(boundListPattern.Variable);
					}
				}
				else
				{
					foreach (BoundPositionalSubpattern subpattern2 in boundITuplePattern.Subpatterns)
					{
						NoteDeclaredPatternVariables(subpattern2.Pattern);
					}
				}
			}
			else
			{
				foreach (BoundPositionalSubpattern item in boundRecursivePattern.Deconstruction.NullToEmpty())
				{
					NoteDeclaredPatternVariables(item.Pattern);
				}
				foreach (BoundPropertySubpattern item2 in boundRecursivePattern.Properties.NullToEmpty())
				{
					NoteDeclaredPatternVariables(item2.Pattern);
				}
				noteOneVariable(boundRecursivePattern.Variable);
			}
		}
		else
		{
			noteOneVariable(boundDeclarationPattern.Variable);
		}
		void noteOneVariable(Symbol? symbol)
		{
			if (base.IsInside && (object)symbol != null && symbol.Kind == SymbolKind.Local)
			{
				_variablesDeclared.Add(symbol);
			}
		}
	}

	public override BoundNode? VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		if (base.IsInside)
		{
			_variablesDeclared.Add(node.LocalSymbol);
		}
		return base.VisitLocalDeclaration(node);
	}

	public override BoundNode? VisitLambda(BoundLambda node)
	{
		if (base.IsInside && !node.WasCompilerGenerated)
		{
			foreach (ParameterSymbol parameter in node.Symbol.Parameters)
			{
				_variablesDeclared.Add(parameter);
			}
		}
		return base.VisitLambda(node);
	}

	public override BoundNode? VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		if (base.IsInside && !node.WasCompilerGenerated)
		{
			foreach (ParameterSymbol parameter in node.Symbol.Parameters)
			{
				_variablesDeclared.Add(parameter);
			}
		}
		return base.VisitLocalFunctionStatement(node);
	}

	public override void VisitForEachIterationVariables(BoundForEachStatement node)
	{
		if (!base.IsInside)
		{
			return;
		}
		BoundDeconstructionAssignmentOperator boundDeconstructionAssignmentOperator = node.DeconstructionOpt?.DeconstructionAssignment;
		if (boundDeconstructionAssignmentOperator == null)
		{
			_variablesDeclared.AddAll(node.IterationVariables);
			return;
		}
		boundDeconstructionAssignmentOperator.Left.VisitAllElements(delegate(BoundExpression x, VariablesDeclaredWalker self)
		{
			self.Visit(x);
		}, this);
	}

	public override BoundNode? VisitCatchBlock(BoundCatchBlock catchBlock)
	{
		if (base.IsInside)
		{
			LocalSymbol localSymbol = catchBlock.Locals.FirstOrDefault();
			if ((object)localSymbol != null && localSymbol.DeclarationKind == LocalDeclarationKind.CatchVariable)
			{
				_variablesDeclared.Add(localSymbol);
			}
		}
		base.VisitCatchBlock(catchBlock);
		return null;
	}

	public override BoundNode? VisitQueryClause(BoundQueryClause node)
	{
		if (base.IsInside && (object)node.DefinedSymbol != null)
		{
			_variablesDeclared.Add(node.DefinedSymbol);
		}
		return base.VisitQueryClause(node);
	}

	protected override void VisitLvalue(BoundLocal node)
	{
		VisitLocal(node);
	}

	public override BoundNode? VisitLocal(BoundLocal node)
	{
		if (base.IsInside && node.DeclarationKind != BoundLocalDeclarationKind.None)
		{
			_variablesDeclared.Add(node.LocalSymbol);
		}
		return null;
	}
}
