using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedRecordEqualityContractProperty : SourcePropertySymbolBase
{
	internal sealed class GetAccessorSymbol : SourcePropertyAccessorSymbol
	{
		public override bool IsImplicitlyDeclared => true;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		internal override bool SynthesizesLoweredBoundBody => true;

		internal GetAccessorSymbol(NamedTypeSymbol containingType, SourcePropertySymbolBase property, DeclarationModifiers propertyModifiers, Location location, CSharpSyntaxNode syntax, BindingDiagnosticBag diagnostics)
			: base(containingType, property, propertyModifiers, location, syntax, hasBlockBody: true, hasExpressionBody: false, isIterator: false, default(SyntaxTokenList), MethodKind.PropertyGet, usesInit: false, isAutoPropertyAccessor: false, isNullableAnalysisEnabled: false, diagnostics)
		{
		}

		internal override ExecutableCodeBinder? TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, bool ignoreAccessibility = false)
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/Records/SynthesizedRecordEqualityContractProperty.cs", 171);
		}

		internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
		{
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
			try
			{
				syntheticBoundNodeFactory.CurrentFunction = this;
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Typeof(ContainingType, base.ReturnType))));
			}
			catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
			{
				diagnostics.Add(missingPredefinedMember.Diagnostic);
				syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
			}
		}
	}

	internal const string PropertyName = "EqualityContract";

	public override bool IsImplicitlyDeclared => true;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	protected override SourcePropertySymbolBase? BoundAttributesSource => null;

	public override IAttributeTargetSymbol AttributesOwner => this;

	protected override Location TypeLocation => ContainingType.GetFirstLocation();

	public SynthesizedRecordEqualityContractProperty(SourceMemberContainerTypeSymbol containingType, BindingDiagnosticBag diagnostics)
	{
		CSharpSyntaxNode syntax = (CSharpSyntaxNode)containingType.SyntaxReferences[0].GetSyntax();
		bool isSealed = containingType.IsSealed;
		bool flag = containingType.BaseTypeNoUseSiteDiagnostics.IsObjectType();
		DeclarationModifiers modifiers;
		if (isSealed)
		{
			if (!flag)
			{
				goto IL_0055;
			}
			modifiers = DeclarationModifiers.Private;
		}
		else
		{
			if (!flag)
			{
				goto IL_0055;
			}
			modifiers = DeclarationModifiers.Protected | DeclarationModifiers.Virtual;
		}
		goto IL_005b;
		IL_0055:
		modifiers = DeclarationModifiers.Protected | DeclarationModifiers.Override;
		goto IL_005b;
		IL_005b:
		base._002Ector(containingType, syntax, hasGetAccessor: true, hasSetAccessor: false, isExplicitInterfaceImplementation: false, null, null, modifiers, hasInitializer: false, hasExplicitAccessMod: false, hasAutoPropertyGet: false, hasAutoPropertySet: false, isExpressionBodied: false, accessorsHaveImplementation: true, getterUsesFieldKeyword: false, setterUsesFieldKeyword: false, RefKind.None, "EqualityContract", default(SyntaxList<AttributeListSyntax>), containingType.GetFirstLocation(), diagnostics);
	}

	public override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany<SyntaxList<AttributeListSyntax>>.Empty;
	}

	protected override SourcePropertyAccessorSymbol CreateGetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		return SourcePropertyAccessorSymbol.CreateAccessorSymbol(ContainingType, this, _modifiers, ContainingType.GetFirstLocation(), (CSharpSyntaxNode)((SourceMemberContainerTypeSymbol)ContainingType).SyntaxReferences[0].GetSyntax(), diagnostics);
	}

	protected override SourcePropertyAccessorSymbol CreateSetAccessorSymbol(bool isAutoPropertyAccessor, BindingDiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/Records/SynthesizedRecordEqualityContractProperty.cs", 77);
	}

	protected override (TypeWithAnnotations Type, ImmutableArray<ParameterSymbol> Parameters) MakeParametersAndBindType(BindingDiagnosticBag diagnostics)
	{
		return (Type: TypeWithAnnotations.Create(Binder.GetWellKnownType(DeclaringCompilation, WellKnownType.System_Type, diagnostics, base.Location), NullableAnnotation.NotAnnotated), Parameters: ImmutableArray<ParameterSymbol>.Empty);
	}

	protected override void ValidatePropertyType(BindingDiagnosticBag diagnostics)
	{
		base.ValidatePropertyType(diagnostics);
		VerifyOverridesEqualityContractFromBase(this, diagnostics);
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
	}

	internal static void VerifyOverridesEqualityContractFromBase(PropertySymbol overriding, BindingDiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = overriding.ContainingType.BaseTypeNoUseSiteDiagnostics;
		if (baseTypeNoUseSiteDiagnostics.IsObjectType() || !baseTypeNoUseSiteDiagnostics.IsRecord)
		{
			return;
		}
		bool flag = false;
		if (!overriding.IsOverride)
		{
			flag = true;
		}
		else
		{
			PropertySymbol overriddenProperty = overriding.OverriddenProperty;
			if ((object)overriddenProperty != null && !overriddenProperty.ContainingType.Equals(baseTypeNoUseSiteDiagnostics, TypeCompareKind.AllIgnoreOptions))
			{
				flag = true;
			}
		}
		if (flag)
		{
			diagnostics.Add(ErrorCode.ERR_DoesNotOverrideBaseEqualityContract, overriding.GetFirstLocation(), overriding, baseTypeNoUseSiteDiagnostics);
		}
	}
}
