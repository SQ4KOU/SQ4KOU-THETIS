using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceConstructorSymbolBase : SourceMemberMethodSymbol
{
	protected ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeWithAnnotations _lazyReturnType;

	protected abstract bool AllowRefOrOut { get; }

	public sealed override bool IsImplicitlyDeclared => base.IsImplicitlyDeclared;

	internal sealed override int ParameterCount
	{
		get
		{
			if (!_lazyParameters.IsDefault)
			{
				return _lazyParameters.Length;
			}
			return GetParameterList().ParameterCount;
		}
	}

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
			return _lazyParameters;
		}
	}

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public sealed override TypeWithAnnotations ReturnTypeWithAnnotations
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public sealed override string Name
	{
		get
		{
			if (!IsStatic)
			{
				return ".ctor";
			}
			return ".cctor";
		}
	}

	internal sealed override bool GenerateDebugInfo => true;

	protected sealed override bool HasSetsRequiredMembersImpl => GetEarlyDecodedWellKnownAttributeData()?.HasSetsRequiredMembersAttribute ?? false;

	protected SourceConstructorSymbolBase(SourceMemberContainerTypeSymbol containingType, Location location, CSharpSyntaxNode syntax, bool isIterator, (DeclarationModifiers declarationModifiers, Flags flags) modifiersAndFlags)
		: base(containingType, syntax.GetReference(), location, isIterator, modifiersAndFlags)
	{
	}

	protected sealed override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)syntaxReferenceOpt.GetSyntax();
		BinderFactory binderFactory = DeclaringCompilation.GetBinderFactory(cSharpSyntaxNode.SyntaxTree);
		ParameterListSyntax parameterList = GetParameterList();
		Binder binder = binderFactory.GetBinder(parameterList, cSharpSyntaxNode, this).WithContainingMemberOrLambda(this);
		Binder withTypeParametersBinder = binder.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.SuppressConstraintChecks, this);
		bool allowRefOrOut = AllowRefOrOut;
		_lazyParameters = ParameterHelpers.MakeParameters(withTypeParametersBinder, this, parameterList, out var _, diagnostics, allowRefOrOut, allowThis: false, addRefReadOnlyModifier: false).Cast<SourceParameterSymbol, ParameterSymbol>();
		_lazyReturnType = TypeWithAnnotations.Create(binder.GetSpecialType(SpecialType.System_Void, diagnostics, cSharpSyntaxNode));
		Location firstLocation = GetFirstLocation();
		if (MethodKind == MethodKind.StaticConstructor && _lazyParameters.Length != 0 && ContainingType.Name == ((ConstructorDeclarationSyntax)base.SyntaxNode).Identifier.ValueText)
		{
			diagnostics.Add(ErrorCode.ERR_StaticConstParam, firstLocation, this);
		}
		CheckEffectiveAccessibility(_lazyReturnType, _lazyParameters, diagnostics);
		CheckFileTypeUsage(_lazyReturnType, _lazyParameters, diagnostics);
		if (IsVararg && (IsGenericMethod || ContainingType.IsGenericType || (_lazyParameters.Length > 0 && _lazyParameters[_lazyParameters.Length - 1].IsParams)))
		{
			diagnostics.Add(ErrorCode.ERR_BadVarargs, firstLocation);
		}
	}

	protected abstract ParameterListSyntax GetParameterList();

	internal sealed override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		base.AfterAddingTypeMembersChecks(conversions, diagnostics);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ParameterHelpers.EnsureRefKindAttributesExist(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureParamCollectionAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureNativeIntegerAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureScopedRefAttributeExists(declaringCompilation, Parameters, diagnostics, modifyCompilation: true);
		ParameterHelpers.EnsureNullableAttributeExists(declaringCompilation, this, Parameters, diagnostics, modifyCompilation: true);
		foreach (ParameterSymbol parameter in Parameters)
		{
			parameter.Type.CheckAllConstraints(declaringCompilation, conversions, parameter.GetFirstLocation(), diagnostics);
		}
		PartialConstructorChecks(diagnostics);
	}

	protected virtual void PartialConstructorChecks(BindingDiagnosticBag diagnostics)
	{
	}

	public sealed override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public sealed override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	internal sealed override OneOrMany<SyntaxList<AttributeListSyntax>> GetReturnTypeAttributeDeclarations()
	{
		return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
	}

	internal sealed override int CalculateLocalSyntaxOffset(int position, SyntaxTree tree)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)syntaxReferenceOpt.GetSyntax();
		if (tree == cSharpSyntaxNode.SyntaxTree)
		{
			if (IsWithinExpressionOrBlockBody(position, out var offset))
			{
				return offset;
			}
			if (position == cSharpSyntaxNode.SpanStart)
			{
				return -1;
			}
		}
		CSharpSyntaxNode initializer = GetInitializer();
		int num;
		if (tree == initializer?.SyntaxTree)
		{
			TextSpan span = initializer.Span;
			num = span.Length;
			if (span.Contains(position))
			{
				return -num + (position - span.Start);
			}
		}
		else
		{
			num = 0;
		}
		if (((SourceNamedTypeSymbol)ContainingType).TryCalculateSyntaxOffsetOfPositionInInitializer(position, tree, IsStatic, num, out var syntaxOffset))
		{
			return syntaxOffset;
		}
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceConstructorSymbolBase.cs", 229);
	}

	internal abstract override bool IsNullableAnalysisEnabled();

	protected abstract CSharpSyntaxNode GetInitializer();

	protected abstract bool IsWithinExpressionOrBlockBody(int position, out int offset);

	internal override (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (arguments.SymbolPart == AttributeLocation.None && CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.SetsRequiredMembersAttribute))
		{
			arguments.GetOrCreateData<MethodEarlyWellKnownAttributeData>().HasSetsRequiredMembersAttribute = true;
			if (ContainingType.IsWellKnownSetsRequiredMembersAttribute())
			{
				return (null, null);
			}
			var (item, item2) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out var generatedDiagnostics);
			if (!generatedDiagnostics)
			{
				return (item, item2);
			}
			return (null, null);
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}
}
