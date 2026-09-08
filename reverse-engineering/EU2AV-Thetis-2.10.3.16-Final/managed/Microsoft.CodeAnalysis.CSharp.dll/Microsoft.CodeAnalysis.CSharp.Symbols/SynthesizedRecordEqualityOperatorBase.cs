using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedRecordEqualityOperatorBase : SourceUserDefinedOperatorSymbolBase
{
	private readonly int _memberOffset;

	public sealed override bool IsImplicitlyDeclared => true;

	protected sealed override Location ReturnTypeLocation => GetFirstLocation();

	protected sealed override SourceMemberMethodSymbol? BoundAttributesSource => null;

	internal sealed override bool GenerateDebugInfo => false;

	internal sealed override bool SynthesizesLoweredBoundBody => true;

	protected SynthesizedRecordEqualityOperatorBase(SourceMemberContainerTypeSymbol containingType, string name, int memberOffset, BindingDiagnosticBag diagnostics)
		: base(MethodKind.UserDefinedOperator, null, name, isCompoundAssignmentOrIncrementAssignment: false, containingType, containingType.GetFirstLocation(), (CSharpSyntaxNode)containingType.SyntaxReferences[0].GetSyntax(), DeclarationModifiers.Static | DeclarationModifiers.Public, hasAnyBody: true, isExpressionBodied: false, isIterator: false, isNullableAnalysisEnabled: false, diagnostics)
	{
		_memberOffset = memberOffset;
	}

	internal sealed override LexicalSortKey GetLexicalSortKey()
	{
		return LexicalSortKey.GetSynthesizedMemberKey(_memberOffset);
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
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/Records/SynthesizedRecordEqualityOperatorBase.cs", 63);
	}

	internal abstract override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics);

	protected sealed override (TypeWithAnnotations ReturnType, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindReturnType(BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Location returnTypeLocation = ReturnTypeLocation;
		NullableAnnotation nullableAnnotation = (ContainingType.IsRecordStruct ? NullableAnnotation.Oblivious : NullableAnnotation.Annotated);
		return (ReturnType: TypeWithAnnotations.Create(Binder.GetSpecialType(declaringCompilation, SpecialType.System_Boolean, returnTypeLocation, diagnostics)), Parameters: ImmutableArray.Create((ParameterSymbol)new SourceSimpleParameterSymbol(this, TypeWithAnnotations.Create(ContainingType, nullableAnnotation), 0, RefKind.None, "left", Locations), (ParameterSymbol)new SourceSimpleParameterSymbol(this, TypeWithAnnotations.Create(ContainingType, nullableAnnotation), 1, RefKind.None, "right", Locations)));
	}

	protected override int GetParameterCountFromSyntax()
	{
		return 2;
	}
}
