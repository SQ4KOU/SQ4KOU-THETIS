using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class StackOverflowProbingInstrumenter(MethodSymbol ensureStackMethod, SyntheticBoundNodeFactory factory, Instrumenter previous) : CompoundInstrumenter(previous)
{
	private readonly MethodSymbol _ensureStackMethod = ensureStackMethod;

	private readonly SyntheticBoundNodeFactory _factory = factory;

	protected override CompoundInstrumenter WithPreviousImpl(Instrumenter previous)
	{
		return new StackOverflowProbingInstrumenter(_ensureStackMethod, _factory, previous);
	}

	public static bool TryCreate(MethodSymbol method, SyntheticBoundNodeFactory factory, Instrumenter previous, [NotNullWhen(true)] out StackOverflowProbingInstrumenter? instrumenter)
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
		MethodSymbol methodSymbol = factory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_RuntimeHelpers__EnsureSufficientExecutionStack, isOptional: true);
		if ((object)methodSymbol == null)
		{
			return false;
		}
		instrumenter = new StackOverflowProbingInstrumenter(methodSymbol, factory, previous);
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
			instrumentation = _factory.CombineInstrumentation(instrumentation, null, _factory.ExpressionStatement(_factory.Call(null, _ensureStackMethod)));
		}
	}
}
