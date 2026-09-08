using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedSimpleProgramEntryPointSymbol : SourceMemberMethodSymbol
{
	private readonly SingleTypeDeclaration _declaration;

	private readonly TypeSymbol _returnType;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private WeakReference<ExecutableCodeBinder>? _weakBodyBinder;

	private WeakReference<ExecutableCodeBinder>? _weakIgnoreAccessibilityBodyBinder;

	public override string Name => "<Main>$";

	internal override MethodImplAttributes ImplementationAttributes
	{
		get
		{
			MethodImplAttributes result = MethodImplAttributes.IL;
			AddAsyncImplAttributeIfNeeded(ref result);
			return result;
		}
	}

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	internal override int ParameterCount => 1;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override TypeWithAnnotations ReturnTypeWithAnnotations => TypeWithAnnotations.Create(_returnType);

	public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public sealed override bool IsImplicitlyDeclared => false;

	internal sealed override bool GenerateDebugInfo => true;

	protected override object MethodChecksLockObject => _declaration;

	internal CompilationUnitSyntax CompilationUnit => (CompilationUnitSyntax)base.SyntaxNode;

	public SyntaxNode ReturnTypeSyntax => CompilationUnit.Members.First((MemberDeclarationSyntax m) => m.Kind() == SyntaxKind.GlobalStatement);

	internal SynthesizedSimpleProgramEntryPointSymbol(SourceMemberContainerTypeSymbol containingType, SingleTypeDeclaration declaration, BindingDiagnosticBag diagnostics)
		: base(containingType, declaration.SyntaxReference, declaration.SyntaxReference.GetLocation(), declaration.IsIterator, MakeModifiersAndFlags(containingType, declaration))
	{
		_declaration = declaration;
		bool hasAwaitExpressions = declaration.HasAwaitExpressions;
		bool hasReturnWithExpression = declaration.HasReturnWithExpression;
		CSharpCompilation declaringCompilation = containingType.DeclaringCompilation;
		if (hasAwaitExpressions)
		{
			if (!hasReturnWithExpression)
			{
				_returnType = Binder.GetWellKnownType(declaringCompilation, WellKnownType.System_Threading_Tasks_Task, diagnostics, NoLocation.Singleton);
			}
			else
			{
				_returnType = Binder.GetWellKnownType(declaringCompilation, WellKnownType.System_Threading_Tasks_Task_T, diagnostics, NoLocation.Singleton).Construct(Binder.GetSpecialType(declaringCompilation, SpecialType.System_Int32, NoLocation.Singleton, diagnostics));
			}
		}
		else if (!hasReturnWithExpression)
		{
			_returnType = Binder.GetSpecialType(declaringCompilation, SpecialType.System_Void, NoLocation.Singleton, diagnostics);
		}
		else
		{
			_returnType = Binder.GetSpecialType(declaringCompilation, SpecialType.System_Int32, NoLocation.Singleton, diagnostics);
		}
		_parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, TypeWithAnnotations.Create(ArrayTypeSymbol.CreateCSharpArray(declaringCompilation.Assembly, TypeWithAnnotations.Create(Binder.GetSpecialType(declaringCompilation, SpecialType.System_String, NoLocation.Singleton, diagnostics)))), 0, RefKind.None, "args"));
	}

	private static (DeclarationModifiers, Flags) MakeModifiersAndFlags(SourceMemberContainerTypeSymbol containingType, SingleTypeDeclaration declaration)
	{
		bool hasAwaitExpressions = declaration.HasAwaitExpressions;
		bool hasReturnWithExpression = declaration.HasReturnWithExpression;
		DeclarationModifiers declarationModifiers = (DeclarationModifiers)(0x104 | (hasAwaitExpressions ? 1048576 : 0));
		CSharpCompilation declaringCompilation = containingType.DeclaringCompilation;
		CompilationUnitSyntax syntax = (CompilationUnitSyntax)declaration.SyntaxReference.GetSyntax();
		bool isNullableAnalysisEnabled = IsNullableAnalysisEnabled(declaringCompilation, syntax);
		Flags item = SourceMemberMethodSymbol.MakeFlags(MethodKind.Ordinary, RefKind.None, declarationModifiers, !hasAwaitExpressions && !hasReturnWithExpression, returnsVoidIsSet: true, isExpressionBodied: false, isExtensionMethod: false, isNullableAnalysisEnabled, isVarArg: false, isExplicitInterfaceImplementation: false, hasThisInitializer: false);
		return (declarationModifiers, item);
	}

	internal static SynthesizedSimpleProgramEntryPointSymbol? GetSimpleProgramEntryPoint(CSharpCompilation compilation, CompilationUnitSyntax compilationUnit, bool fallbackToMainEntryPoint)
	{
		SourceNamedTypeSymbol simpleProgramNamedTypeSymbol = GetSimpleProgramNamedTypeSymbol(compilation);
		if ((object)simpleProgramNamedTypeSymbol == null)
		{
			return null;
		}
		ImmutableArray<SynthesizedSimpleProgramEntryPointSymbol> simpleProgramEntryPoints = simpleProgramNamedTypeSymbol.GetSimpleProgramEntryPoints();
		foreach (SynthesizedSimpleProgramEntryPointSymbol item in simpleProgramEntryPoints)
		{
			if (item.SyntaxTree == compilationUnit.SyntaxTree && item.SyntaxNode == compilationUnit)
			{
				return item;
			}
		}
		if (!fallbackToMainEntryPoint)
		{
			return null;
		}
		return simpleProgramEntryPoints[0];
	}

	internal static SynthesizedSimpleProgramEntryPointSymbol? GetSimpleProgramEntryPoint(CSharpCompilation compilation)
	{
		return GetSimpleProgramNamedTypeSymbol(compilation)?.GetSimpleProgramEntryPoints().First();
	}

	private static SourceNamedTypeSymbol? GetSimpleProgramNamedTypeSymbol(CSharpCompilation compilation)
	{
		return compilation.SourceModule.GlobalNamespace.GetTypeMembers("Program").OfType<SourceNamedTypeSymbol>().SingleOrDefault((SourceNamedTypeSymbol s) => s.IsSimpleProgram);
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return localPosition;
	}

	protected override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
	}

	public override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return GetBodyBinder(ignoreAccessibility);
	}

	private ExecutableCodeBinder CreateBodyBinder(bool ignoreAccessibility)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		CSharpSyntaxNode syntaxNode = base.SyntaxNode;
		Binder next = new BuckStopsHereBinder(declaringCompilation, FileIdentifier.Create(syntaxNode.SyntaxTree, declaringCompilation.Options.SourceReferenceResolver));
		NamespaceSymbol globalNamespace = declaringCompilation.GlobalNamespace;
		SourceNamespaceSymbol declaringSymbol = (SourceNamespaceSymbol)declaringCompilation.SourceModule.GlobalNamespace;
		next = WithExternAndUsingAliasesBinder.Create(declaringSymbol, syntaxNode, WithUsingNamespacesAndTypesBinder.Create(declaringSymbol, syntaxNode, next));
		next = new InContainerBinder(globalNamespace, next);
		next = new InContainerBinder(ContainingType, next);
		next = new InMethodBinder(this, next);
		next = next.WithAdditionalFlags(ignoreAccessibility ? BinderFlags.IgnoreAccessibility : BinderFlags.None);
		return new ExecutableCodeBinder(syntaxNode, this, next);
	}

	internal ExecutableCodeBinder GetBodyBinder(bool ignoreAccessibility)
	{
		ref WeakReference<ExecutableCodeBinder> reference = ref ignoreAccessibility ? ref _weakIgnoreAccessibilityBodyBinder : ref _weakBodyBinder;
		WeakReference<ExecutableCodeBinder> weakReference;
		ExecutableCodeBinder executableCodeBinder;
		do
		{
			weakReference = reference;
			if (weakReference != null && weakReference.TryGetTarget(out var target))
			{
				return target;
			}
			executableCodeBinder = CreateBodyBinder(ignoreAccessibility);
		}
		while (Interlocked.CompareExchange(ref reference, new WeakReference<ExecutableCodeBinder>(executableCodeBinder), weakReference) != weakReference);
		return executableCodeBinder;
	}

	public override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken)
	{
		if (_declaration.SyntaxReference.SyntaxTree == tree)
		{
			if (!definedWithinSpan.HasValue)
			{
				return true;
			}
			TextSpan valueOrDefault = definedWithinSpan.GetValueOrDefault();
			foreach (GlobalStatementSyntax item in ((CompilationUnitSyntax)tree.GetRoot(cancellationToken)).Members.OfType<GlobalStatementSyntax>())
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (item.Span.IntersectsWith(valueOrDefault))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsNullableAnalysisEnabled(CSharpCompilation compilation, CompilationUnitSyntax syntax)
	{
		foreach (MemberDeclarationSyntax member in syntax.Members)
		{
			if (member.Kind() == SyntaxKind.GlobalStatement && compilation.IsNullableAnalysisEnabledIn(member))
			{
				return true;
			}
		}
		return false;
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
