using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SwitchExpressionBinder : Binder
{
	private readonly SwitchExpressionSyntax SwitchExpressionSyntax;

	internal SwitchExpressionBinder(SwitchExpressionSyntax switchExpressionSyntax, Binder next)
		: base(next)
	{
		SwitchExpressionSyntax = switchExpressionSyntax;
	}

	internal override BoundExpression BindSwitchExpressionCore(SwitchExpressionSyntax node, Binder originalBinder, BindingDiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindSwitchGoverningExpression(diagnostics);
		ImmutableArray<BoundSwitchExpressionArm> immutableArray = BindSwitchExpressionArms(node, originalBinder, boundExpression, diagnostics);
		TypeSymbol type = InferResultType(immutableArray, diagnostics);
		bool reportedNotExhaustive = CheckSwitchExpressionExhaustive(node, boundExpression, immutableArray, out BoundDecisionDag decisionDag, out LabelSymbol defaultLabel, out bool wasReported, diagnostics);
		decisionDag = decisionDag.SimplifyDecisionDagIfConstantInput(boundExpression);
		if (!wasReported && diagnostics.AccumulatesDiagnostics && DecisionDagBuilder.EnableRedundantPatternsCheck(base.Compilation))
		{
			DecisionDagBuilder.CheckRedundantPatternsForSwitchExpression(base.Compilation, node, boundExpression, immutableArray, diagnostics);
		}
		return new BoundUnconvertedSwitchExpression(node, boundExpression, immutableArray, decisionDag, defaultLabel, reportedNotExhaustive, type);
	}

	private bool CheckSwitchExpressionExhaustive(SwitchExpressionSyntax node, BoundExpression boundInputExpression, ImmutableArray<BoundSwitchExpressionArm> switchArms, out BoundDecisionDag decisionDag, [NotNullWhen(true)] out LabelSymbol? defaultLabel, out bool wasReported, BindingDiagnosticBag diagnostics)
	{
		wasReported = false;
		defaultLabel = new GeneratedLabelSymbol("default");
		decisionDag = DecisionDagBuilder.CreateDecisionDagForSwitchExpression(base.Compilation, node, boundInputExpression, switchArms, defaultLabel, diagnostics);
		ImmutableHashSet<LabelSymbol> reachableLabels = decisionDag.ReachableLabels;
		bool flag = false;
		foreach (BoundSwitchExpressionArm item in switchArms)
		{
			flag |= item.HasErrors;
			if (!flag && !reachableLabels.Contains(item.Label))
			{
				diagnostics.Add(ErrorCode.ERR_SwitchArmSubsumed, item.Pattern.Syntax.Location);
				wasReported = true;
			}
		}
		if (!reachableLabels.Contains(defaultLabel))
		{
			defaultLabel = null;
			return false;
		}
		if (flag)
		{
			return true;
		}
		TopologicalSort.TryIterativeSort(decisionDag.RootNode, addNonNullSuccessors, out var result);
		foreach (BoundDecisionDagNode item2 in result)
		{
			if (item2 is BoundLeafDecisionDagNode boundLeafDecisionDagNode && boundLeafDecisionDagNode.Label == defaultLabel)
			{
				string text = PatternExplainer.SamplePatternForPathToDagNode(BoundDagTemp.ForOriginalInput(boundInputExpression), result, item2, nullPaths: false, out var requiresFalseWhenClause, out var unnamedEnumValue);
				ErrorCode code = (requiresFalseWhenClause ? ErrorCode.WRN_SwitchExpressionNotExhaustiveWithWhen : (unnamedEnumValue ? ErrorCode.WRN_SwitchExpressionNotExhaustiveWithUnnamedEnumValue : ErrorCode.WRN_SwitchExpressionNotExhaustive));
				diagnostics.Add(code, node.SwitchKeyword.GetLocation(), text);
				wasReported = true;
				return true;
			}
		}
		return false;
		static void addNonNullSuccessors(ref TemporaryArray<BoundDecisionDagNode> builder, BoundDecisionDagNode n)
		{
			if (n is BoundTestDecisionDagNode boundTestDecisionDagNode)
			{
				BoundDagTest test = boundTestDecisionDagNode.Test;
				if (!(test is BoundDagNonNullTest))
				{
					if (test is BoundDagExplicitNullTest)
					{
						builder.Add(boundTestDecisionDagNode.WhenFalse);
					}
					else
					{
						BoundDecisionDag.AddSuccessors(ref builder, n);
					}
				}
				else
				{
					builder.Add(boundTestDecisionDagNode.WhenTrue);
				}
			}
			else
			{
				BoundDecisionDag.AddSuccessors(ref builder, n);
			}
		}
	}

	private TypeSymbol? InferResultType(ImmutableArray<BoundSwitchExpressionArm> switchCases, BindingDiagnosticBag diagnostics)
	{
		PooledHashSet<TypeSymbol> pooledSymbolHashSetInstance = SpecializedSymbolCollections.GetPooledSymbolHashSetInstance<TypeSymbol>();
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		foreach (BoundSwitchExpressionArm item in switchCases)
		{
			TypeSymbol type = item.Value.Type;
			if ((object)type != null && pooledSymbolHashSetInstance.Add(type))
			{
				instance.Add(type);
			}
		}
		pooledSymbolHashSetInstance.Free();
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = GetNewCompoundUseSiteInfo(diagnostics);
		TypeSymbol typeSymbol = BestTypeInferrer.GetBestType(instance, base.Conversions, ref useSiteInfo);
		instance.Free();
		if ((object)typeSymbol != null)
		{
			foreach (BoundSwitchExpressionArm item2 in switchCases)
			{
				if (!base.Conversions.ClassifyImplicitConversionFromExpression(item2.Value, typeSymbol, ref useSiteInfo).Exists)
				{
					typeSymbol = null;
					break;
				}
			}
		}
		diagnostics.Add(SwitchExpressionSyntax, useSiteInfo);
		return typeSymbol;
	}

	private ImmutableArray<BoundSwitchExpressionArm> BindSwitchExpressionArms(SwitchExpressionSyntax node, Binder originalBinder, BoundExpression inputExpression, BindingDiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundSwitchExpressionArm> instance = ArrayBuilder<BoundSwitchExpressionArm>.GetInstance();
		TypeSymbol inputType = GetInputType(inputExpression);
		foreach (SwitchExpressionArmSyntax arm in node.Arms)
		{
			BoundSwitchExpressionArm item = originalBinder.GetRequiredBinder(arm).BindSwitchExpressionArm(arm, inputType, diagnostics);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	internal TypeSymbol GetInputType(BoundExpression? inputExpression = null)
	{
		if (inputExpression == null)
		{
			inputExpression = BindSwitchGoverningExpression(BindingDiagnosticBag.Discarded);
		}
		return inputExpression.Type;
	}

	private BoundExpression BindSwitchGoverningExpression(BindingDiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindRValueWithoutTargetType(SwitchExpressionSyntax.GoverningExpression, diagnostics);
		if ((object)boundExpression.Type == null || boundExpression.Type.IsVoidType())
		{
			diagnostics.Add(ErrorCode.ERR_BadPatternExpression, SwitchExpressionSyntax.GoverningExpression.Location, boundExpression.Display);
			boundExpression = GenerateConversionForAssignment(CreateErrorType(), boundExpression, diagnostics);
		}
		return boundExpression;
	}
}
