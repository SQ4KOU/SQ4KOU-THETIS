using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedSourceOrdinaryMethodSymbol : SourceOrdinaryMethodSymbolBase
{
	public sealed override bool IsImplicitlyDeclared => true;

	protected sealed override Location ReturnTypeLocation => GetFirstLocation();

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	protected sealed override TypeSymbol? ExplicitInterfaceType => null;

	protected sealed override SourceMemberMethodSymbol? BoundAttributesSource => null;

	internal sealed override bool GenerateDebugInfo => false;

	internal sealed override bool SynthesizesLoweredBoundBody => true;

	protected SynthesizedSourceOrdinaryMethodSymbol(SourceMemberContainerTypeSymbol containingType, string name, Location location, CSharpSyntaxNode syntax, (DeclarationModifiers declarationModifiers, Flags flags) modifiersAndFlags)
		: base(containingType, name, location, syntax, isIterator: false, modifiersAndFlags)
	{
	}

	protected override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		var (returnType, parameters) = MakeParametersAndBindReturnType(diagnostics);
		MethodChecks(returnType, parameters, diagnostics);
	}

	protected abstract (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics);

	protected sealed override MethodSymbol? FindExplicitlyImplementedMethod(BindingDiagnosticBag diagnostics)
	{
		return null;
	}

	public sealed override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public sealed override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	protected sealed override void PartialMethodChecks(BindingDiagnosticBag diagnostics)
	{
	}

	protected sealed override void ExtensionMethodChecks(BindingDiagnosticBag diagnostics)
	{
	}

	protected sealed override void CompleteAsyncMethodChecksBetweenStartAndFinish()
	{
	}

	protected sealed override void CheckConstraintsForExplicitInterfaceType(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
	}

	internal sealed override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
	}

	public sealed override string? GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal sealed override ExecutableCodeBinder? TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SynthesizedSourceOrdinaryMethodSymbol.cs", 75);
	}

	internal abstract override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics);
}
