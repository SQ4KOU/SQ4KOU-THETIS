using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ModuleCancellationInstrumenter(MethodSymbol throwMethod, SyntheticBoundNodeFactory factory, Instrumenter previous) : CompoundInstrumenter(previous)
{
	private readonly MethodSymbol _throwMethod = throwMethod;

	private readonly SyntheticBoundNodeFactory _factory = factory;

	protected override CompoundInstrumenter WithPreviousImpl(Instrumenter previous)
	{
		return new ModuleCancellationInstrumenter(_throwMethod, _factory, previous);
	}

	public static bool TryCreate(MethodSymbol method, SyntheticBoundNodeFactory factory, Instrumenter previous, [NotNullWhen(true)] out ModuleCancellationInstrumenter? instrumenter)
	{
		instrumenter = null;
		MethodKind methodKind = method.MethodKind;
		if ((methodKind != MethodKind.Constructor && methodKind != MethodKind.StaticConstructor) || 1 == 0)
		{
			if ((object)method != null && method.IsImplicitlyDeclared)
			{
				goto IL_0053;
			}
			if (method is SourceMemberMethodSymbol sourceMemberMethodSymbol)
			{
				(BlockSyntax, ArrowExpressionClauseSyntax) bodies = sourceMemberMethodSymbol.Bodies;
				if (bodies.Item2 == null && bodies.Item1 == null && !(sourceMemberMethodSymbol is SynthesizedSimpleProgramEntryPointSymbol))
				{
					goto IL_0053;
				}
			}
		}
		MethodSymbol methodSymbol = factory.WellKnownMethod(WellKnownMember.System_Threading_CancellationToken__ThrowIfCancellationRequested, isOptional: true);
		if ((object)methodSymbol == null)
		{
			return false;
		}
		instrumenter = new ModuleCancellationInstrumenter(methodSymbol, factory, previous);
		return true;
		IL_0053:
		return false;
	}

	public override void InstrumentBlock(BoundBlock original, LocalRewriter rewriter, ref TemporaryArray<LocalSymbol> additionalLocals, out BoundStatement? prologue, out BoundStatement? epilogue, out BoundBlockInstrumentation? instrumentation)
	{
		base.InstrumentBlock(original, rewriter, ref additionalLocals, out prologue, out epilogue, out instrumentation);
		bool flag = rewriter.CurrentMethodBody == original;
		bool flag2 = rewriter.CurrentLambdaBody == original;
		if ((flag || flag2) && (!flag || _factory.TopLevelMethod.MethodKind != MethodKind.StaticConstructor))
		{
			instrumentation = _factory.CombineInstrumentation(instrumentation, null, _factory.ExpressionStatement(_factory.ThrowIfModuleCancellationRequested()));
		}
	}

	private BoundExpression InstrumentExpression(BoundExpression expression)
	{
		return _factory.Sequence(ImmutableArray<LocalSymbol>.Empty, ImmutableCollectionsMarshal.AsImmutableArray(new BoundExpression[1] { _factory.ThrowIfModuleCancellationRequested() }), expression);
	}

	private BoundStatement InstrumentStatement(BoundStatement statement)
	{
		return _factory.StatementList(_factory.ExpressionStatement(_factory.ThrowIfModuleCancellationRequested()), statement);
	}

	public override BoundExpression InstrumentWhileStatementCondition(BoundWhileStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return InstrumentExpression(base.InstrumentWhileStatementCondition(original, rewrittenCondition, factory));
	}

	public override BoundExpression InstrumentDoStatementCondition(BoundDoStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return InstrumentExpression(base.InstrumentDoStatementCondition(original, rewrittenCondition, factory));
	}

	public override BoundExpression InstrumentForStatementCondition(BoundForStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return InstrumentExpression(base.InstrumentForStatementCondition(original, rewrittenCondition, factory));
	}

	public override BoundStatement InstrumentForStatementConditionalGotoStartOrBreak(BoundForStatement original, BoundStatement branchBack)
	{
		return InstrumentStatement(base.InstrumentForStatementConditionalGotoStartOrBreak(original, branchBack));
	}

	public override BoundStatement InstrumentForEachStatementConditionalGotoStart(BoundForEachStatement original, BoundStatement branchBack)
	{
		return InstrumentStatement(base.InstrumentForEachStatementConditionalGotoStart(original, branchBack));
	}

	public override BoundStatement InstrumentGotoStatement(BoundGotoStatement original, BoundStatement rewritten)
	{
		return InstrumentStatement(base.InstrumentGotoStatement(original, rewritten));
	}

	public override void InterceptCallAndAdjustArguments(ref MethodSymbol method, ref BoundExpression? receiver, ref ImmutableArray<BoundExpression> arguments, ref ImmutableArray<RefKind> argumentRefKindsOpt)
	{
		base.Previous.InterceptCallAndAdjustArguments(ref method, ref receiver, ref arguments, ref argumentRefKindsOpt);
		ImmutableArray<BoundExpression> immutableArray = arguments;
		int length = immutableArray.Length;
		ImmutableArray<RefKind> immutableArray2;
		if (length >= 1)
		{
			BoundExpression boundExpression = immutableArray[length - 1];
			if (boundExpression != null)
			{
				TypeSymbol type = boundExpression.Type;
				if ((object)type != null)
				{
					if (!argumentRefKindsOpt.IsDefault)
					{
						immutableArray2 = argumentRefKindsOpt;
						length = immutableArray2.Length;
						if (length < 1 || immutableArray2[length - 1] != RefKind.None)
						{
							goto IL_0118;
						}
					}
					if (type.Equals(_throwMethod.ContainingType, TypeCompareKind.ConsiderEverything))
					{
						immutableArray = arguments.Slice(0, arguments.Length - 1);
						length = 0;
						BoundExpression[] array = new BoundExpression[1 + immutableArray.Length];
						ReadOnlySpan<BoundExpression> readOnlySpan = immutableArray.AsSpan();
						readOnlySpan.CopyTo(new Span<BoundExpression>(array).Slice(length, readOnlySpan.Length));
						length += readOnlySpan.Length;
						array[length] = _factory.Sequence(new BoundExpression[1] { boundExpression }, _factory.ModuleCancellationToken());
						arguments = ImmutableCollectionsMarshal.AsImmutableArray(array);
						return;
					}
				}
			}
		}
		goto IL_0118;
		IL_0118:
		MethodSymbol methodSymbol = FindOverloadWithCancellationToken(method);
		if ((object)methodSymbol != null)
		{
			method = methodSymbol;
			immutableArray = arguments;
			length = 0;
			BoundExpression[] array = new BoundExpression[1 + immutableArray.Length];
			ReadOnlySpan<BoundExpression> readOnlySpan = immutableArray.AsSpan();
			readOnlySpan.CopyTo(new Span<BoundExpression>(array).Slice(length, readOnlySpan.Length));
			length += readOnlySpan.Length;
			array[length] = _factory.ModuleCancellationToken();
			arguments = ImmutableCollectionsMarshal.AsImmutableArray(array);
			ImmutableArray<RefKind> immutableArray3;
			if (!argumentRefKindsOpt.IsDefault)
			{
				immutableArray2 = argumentRefKindsOpt;
				length = 0;
				RefKind[] array2 = new RefKind[1 + immutableArray2.Length];
				ReadOnlySpan<RefKind> readOnlySpan2 = immutableArray2.AsSpan();
				readOnlySpan2.CopyTo(new Span<RefKind>(array2).Slice(length, readOnlySpan2.Length));
				length += readOnlySpan2.Length;
				array2[length] = RefKind.None;
				immutableArray3 = ImmutableCollectionsMarshal.AsImmutableArray(array2);
			}
			else
			{
				immutableArray2 = default(ImmutableArray<RefKind>);
				immutableArray3 = immutableArray2;
			}
			argumentRefKindsOpt = immutableArray3;
		}
	}

	private MethodSymbol? FindOverloadWithCancellationToken(MethodSymbol method)
	{
		MethodKind methodKind = method.MethodKind;
		if ((methodKind != MethodKind.Constructor && methodKind != MethodKind.Ordinary) || 1 == 0)
		{
			return null;
		}
		MethodSymbol originalDefinition = method.OriginalDefinition;
		bool hasSetsRequiredMembers = originalDefinition.HasSetsRequiredMembers;
		foreach (Symbol member in originalDefinition.ContainingType.GetMembers(method.Name))
		{
			if (member == _factory.TopLevelMethod?.OriginalDefinition || member.IsStatic != originalDefinition.IsStatic || member.MetadataVisibility != originalDefinition.MetadataVisibility || !(member is MethodSymbol { Parameters: { Length: var length } parameters } methodSymbol) || length < 1)
			{
				continue;
			}
			ParameterSymbol parameterSymbol = parameters[length - 1];
			if ((object)parameterSymbol == null || parameterSymbol.RefKind != RefKind.None)
			{
				continue;
			}
			TypeSymbol type = parameterSymbol.Type;
			if ((object)type == null || methodSymbol.Arity != originalDefinition.Arity || originalDefinition.MethodKind != methodSymbol.MethodKind)
			{
				continue;
			}
			ImmutableArray<ParameterSymbol> parameters2 = originalDefinition.Parameters;
			if (parameters2.Length != parameters.Length - 1 || !type.Equals(_throwMethod.ContainingType, TypeCompareKind.ConsiderEverything) || (hasSetsRequiredMembers && !methodSymbol.HasSetsRequiredMembers))
			{
				continue;
			}
			TypeMap typeMap = ((originalDefinition.Arity > 0) ? new TypeMap(methodSymbol.TypeParameters, originalDefinition.TypeParameters) : null);
			parameters2 = originalDefinition.Parameters;
			ReadOnlySpan<ParameterSymbol> @params = parameters2.AsSpan();
			parameters2 = originalDefinition.Parameters;
			if (MemberSignatureComparer.HaveSameParameterTypes(@params, null, parameters.AsSpan(0, parameters2.Length), typeMap, MemberSignatureComparer.RefKindCompareMode.ConsiderDifferences, considerDefaultValues: false, TypeCompareKind.AllNullableIgnoreOptions | TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNativeIntegers) && MemberSignatureComparer.HaveSameReturnTypes(originalDefinition, null, methodSymbol, typeMap, TypeCompareKind.AllNullableIgnoreOptions | TypeCompareKind.IgnoreTupleNames | TypeCompareKind.IgnoreNativeIntegers) && MemberSignatureComparer.HaveSameConstraints(originalDefinition.TypeParameters, null, methodSymbol.TypeParameters, typeMap, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes))
			{
				MethodSymbol methodSymbol2 = methodSymbol.AsMember(method.ContainingType);
				if (methodSymbol2.Arity <= 0)
				{
					return methodSymbol2;
				}
				return methodSymbol2.Construct(method.TypeArgumentsWithAnnotations);
			}
		}
		return null;
	}
}
