using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class FlowAnalysisPass
{
	public static BoundBlock Rewrite(MethodSymbol method, BoundBlock block, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics, bool hasTrailingExpression, bool originalBodyNested)
	{
		CSharpCompilation declaringCompilation = method.DeclaringCompilation;
		bool needsImplicitReturn2;
		ImmutableArray<FieldSymbol> implicitlyInitializedFieldsOpt2;
		if (method.ReturnsVoid || method.IsIterator || method.IsAsyncEffectivelyReturningTask(declaringCompilation))
		{
			ImmutableArray<FieldSymbol> implicitlyInitializedFieldsOpt = default(ImmutableArray<FieldSymbol>);
			bool needsImplicitReturn = true;
			if ((method.IsImplicitlyDeclared && !method.IsScriptInitializer) || Analyze(declaringCompilation, method, block, diagnostics.DiagnosticBag, out needsImplicitReturn, out implicitlyInitializedFieldsOpt))
			{
				if (!implicitlyInitializedFieldsOpt.IsDefault)
				{
					block = PrependImplicitInitializations(block, method, implicitlyInitializedFieldsOpt, compilationState, diagnostics);
				}
				if (needsImplicitReturn)
				{
					block = AppendImplicitReturn(block, method, originalBodyNested);
				}
			}
		}
		else if (Analyze(declaringCompilation, method, block, diagnostics.DiagnosticBag, out needsImplicitReturn2, out implicitlyInitializedFieldsOpt2))
		{
			TypeSymbol typeSymbol = (method as SynthesizedInteractiveInitializerMethod)?.ResultType;
			if (!hasTrailingExpression && (object)typeSymbol != null)
			{
				BoundDefaultExpression boundDefaultExpression = new BoundDefaultExpression(method.GetNonNullSyntaxNode(), typeSymbol);
				ImmutableArray<BoundStatement> statements = block.Statements.Add(new BoundReturnStatement(boundDefaultExpression.Syntax, RefKind.None, boundDefaultExpression, @checked: false));
				block = new BoundBlock(block.Syntax, ImmutableArray<LocalSymbol>.Empty, statements)
				{
					WasCompilerGenerated = true
				};
			}
			else if (method.Locations.Length == 1)
			{
				Location location = ((method is SynthesizedSimpleProgramEntryPointSymbol synthesizedSimpleProgramEntryPointSymbol) ? synthesizedSimpleProgramEntryPointSymbol.ReturnTypeSyntax.GetLocation() : method.GetFirstLocation());
				diagnostics.Add(ErrorCode.ERR_ReturnExpected, location, method);
			}
		}
		return block;
	}

	private static BoundBlock PrependImplicitInitializations(BoundBlock body, MethodSymbol method, ImmutableArray<FieldSymbol> implicitlyInitializedFields, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol containingType = method.ContainingType;
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(method, body.Syntax, compilationState, diagnostics);
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(implicitlyInitializedFields.Length);
		if (containingType.HasInlineArrayAttribute(out var length) && length > 1 && (object)containingType.TryGetPossiblyUnsupportedByLanguageInlineArrayElementField() != null)
		{
			instance.Add(syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.AssignmentExpression(syntheticBoundNodeFactory.This(), syntheticBoundNodeFactory.Default(containingType))));
		}
		else
		{
			foreach (FieldSymbol item2 in implicitlyInitializedFields)
			{
				if (item2.RefKind == RefKind.None)
				{
					instance.Add(syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.AssignmentExpression(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), item2), syntheticBoundNodeFactory.Default(item2.Type))));
				}
				else
				{
					instance.Add(syntheticBoundNodeFactory.ExpressionStatement(syntheticBoundNodeFactory.AssignmentExpression(syntheticBoundNodeFactory.Field(syntheticBoundNodeFactory.This(), item2), syntheticBoundNodeFactory.NullRef(item2.TypeWithAnnotations), isRef: true)));
				}
			}
		}
		BoundStatement item = syntheticBoundNodeFactory.HiddenSequencePoint(syntheticBoundNodeFactory.Block(instance.ToImmutableAndFree()));
		return body.Update(body.Locals, body.LocalFunctions, body.HasUnsafeModifier, body.Instrumentation, body.Statements.Insert(0, item));
	}

	private static BoundBlock AppendImplicitReturn(BoundBlock body, MethodSymbol method, bool originalBodyNested)
	{
		if (originalBodyNested)
		{
			ImmutableArray<BoundStatement> statements = body.Statements;
			int length = statements.Length;
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(length);
			instance.AddRange(statements, length - 1);
			instance.Add(AppendImplicitReturn((BoundBlock)statements[length - 1], method));
			return body.Update(body.Locals, ImmutableArray<MethodSymbol>.Empty, body.HasUnsafeModifier, body.Instrumentation, instance.ToImmutableAndFree());
		}
		return AppendImplicitReturn(body, method);
	}

	internal static BoundBlock AppendImplicitReturn(BoundBlock body, MethodSymbol method)
	{
		SyntaxNode syntax = body.Syntax;
		BoundStatement item = ((method.IsIterator && !method.IsAsync) ? ((BoundStatement)BoundYieldBreakStatement.Synthesized(syntax)) : ((BoundStatement)BoundReturnStatement.Synthesized(syntax, RefKind.None, null)));
		return body.Update(body.Locals, body.LocalFunctions, body.HasUnsafeModifier, body.Instrumentation, body.Statements.Add(item));
	}

	private static bool Analyze(CSharpCompilation compilation, MethodSymbol method, BoundBlock block, DiagnosticBag diagnostics, out bool needsImplicitReturn, out ImmutableArray<FieldSymbol> implicitlyInitializedFieldsOpt)
	{
		needsImplicitReturn = ControlFlowPass.Analyze(compilation, method, block, diagnostics);
		DefiniteAssignmentPass.Analyze(compilation, method, block, diagnostics, out implicitlyInitializedFieldsOpt, requireOutParamsAssigned: true);
		if (!needsImplicitReturn)
		{
			return !implicitlyInitializedFieldsOpt.IsDefault;
		}
		return true;
	}
}
