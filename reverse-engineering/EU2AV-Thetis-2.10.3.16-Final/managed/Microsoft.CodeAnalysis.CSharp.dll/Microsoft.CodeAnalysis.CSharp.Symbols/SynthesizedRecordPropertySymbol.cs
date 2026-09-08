using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordPropertySymbol : SourcePropertySymbolBase
{
	public SourceParameterSymbol BackingParameter { get; }

	protected override SourcePropertySymbolBase? BoundAttributesSource => null;

	public override IAttributeTargetSymbol AttributesOwner => (BackingParameter as IAttributeTargetSymbol) ?? this;

	protected override Location TypeLocation => ((ParameterSyntax)base.CSharpSyntaxNode).Type.Location;

	public SynthesizedRecordPropertySymbol(SourceMemberContainerTypeSymbol containingType, CSharpSyntaxNode syntax, ParameterSymbol backingParameter, bool isOverride, BindingDiagnosticBag diagnostics)
		: base(containingType, syntax, hasGetAccessor: true, hasSetAccessor: true, isExplicitInterfaceImplementation: false, null, null, (DeclarationModifiers)(0x10 | (isOverride ? 262144 : 0)), hasInitializer: true, hasExplicitAccessMod: false, hasAutoPropertyGet: true, hasAutoPropertySet: true, isExpressionBodied: false, accessorsHaveImplementation: true, getterUsesFieldKeyword: false, setterUsesFieldKeyword: false, RefKind.None, backingParameter.Name, default(SyntaxList<AttributeListSyntax>), backingParameter.GetFirstLocation(), diagnostics)
	{
		BackingParameter = (SourceParameterSymbol)backingParameter;
	}

	public override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(BackingParameter.AttributeDeclarationList);
	}

	protected override SourcePropertyAccessorSymbol CreateGetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		return CreateAccessorSymbol(isGet: true, base.CSharpSyntaxNode, diagnostics);
	}

	protected override SourcePropertyAccessorSymbol CreateSetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		return CreateAccessorSymbol(isGet: false, base.CSharpSyntaxNode, diagnostics);
	}

	private static bool ShouldUseInit(TypeSymbol container)
	{
		if (container.IsStructType())
		{
			return container.IsReadOnly;
		}
		return true;
	}

	private SourcePropertyAccessorSymbol CreateAccessorSymbol(bool isGet, CSharpSyntaxNode syntax, BindingDiagnosticBag diagnostics)
	{
		bool usesInit = !isGet && ShouldUseInit(ContainingType);
		return SourcePropertyAccessorSymbol.CreateAccessorSymbol(isGet, usesInit, ContainingType, this, _modifiers, ((ParameterSyntax)syntax).Identifier.GetLocation(), syntax, diagnostics);
	}

	protected override (TypeWithAnnotations Type, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindType(BindingDiagnosticBag diagnostics)
	{
		return (Type: BackingParameter.TypeWithAnnotations, Parameters: ImmutableArray<ParameterSymbol>.Empty);
	}

	public static bool HaveCorrespondingSynthesizedRecordPropertySymbol(SourceParameterSymbol parameter)
	{
		if (parameter.ContainingSymbol is SynthesizedPrimaryConstructor)
		{
			return parameter.ContainingType.GetMembersUnordered().Any((Symbol s, SourceParameterSymbol sourceParameterSymbol) => (object)(s as SynthesizedRecordPropertySymbol)?.BackingParameter == sourceParameterSymbol, parameter);
		}
		return false;
	}
}
