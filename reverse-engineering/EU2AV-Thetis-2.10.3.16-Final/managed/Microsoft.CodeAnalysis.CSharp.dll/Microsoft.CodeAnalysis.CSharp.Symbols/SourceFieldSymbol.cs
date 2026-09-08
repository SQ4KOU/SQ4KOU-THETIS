using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceFieldSymbol : FieldSymbolWithAttributesAndModifiers
{
	protected readonly SourceMemberContainerTypeSymbol containingType;

	public abstract override string Name { get; }

	protected override IAttributeTargetSymbol AttributeOwner => this;

	internal sealed override bool RequiresCompletion => true;

	internal bool IsNew => (Modifiers & DeclarationModifiers.New) != 0;

	protected ImmutableArray<CustomModifier> RequiredCustomModifiers
	{
		get
		{
			if (!IsVolatile)
			{
				return ImmutableArray<CustomModifier>.Empty;
			}
			return ImmutableArray.Create(CSharpCustomModifier.CreateRequired(ContainingAssembly.GetSpecialType(SpecialType.System_Runtime_CompilerServices_IsVolatile)));
		}
	}

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public sealed override Symbol ContainingSymbol => containingType;

	public override NamedTypeSymbol ContainingType => containingType;

	internal sealed override bool HasRuntimeSpecialName => Name == "value__";

	internal override bool IsRequired => (Modifiers & DeclarationModifiers.Required) != 0;

	protected SourceFieldSymbol(SourceMemberContainerTypeSymbol containingType)
	{
		this.containingType = containingType;
	}

	protected void CheckAccessibility(BindingDiagnosticBag diagnostics)
	{
		ModifierUtils.CheckAccessibility(Modifiers, this, isExplicitInterfaceImplementation: false, diagnostics, ErrorLocation);
	}

	protected void ReportModifiersDiagnostics(BindingDiagnosticBag diagnostics)
	{
		if (ContainingType.IsSealed && DeclaredAccessibility.HasProtected())
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(containingType), ErrorLocation, this);
		}
		else if (IsVolatile && IsReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_VolatileAndReadonly, ErrorLocation, this);
		}
		else if (containingType.IsStatic && !IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_InstanceMemberInStaticClass, ErrorLocation, this);
		}
		else if (!IsStatic && !IsReadOnly && containingType.IsReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_FieldsInRoStruct, ErrorLocation);
		}
	}

	protected sealed override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (arguments.Attribute.IsTargetAttribute(AttributeDescription.FixedBufferAttribute))
		{
			((BindingDiagnosticBag)arguments.Diagnostics).Add(ErrorCode.ERR_DoNotUseFixedBufferAttr, arguments.AttributeSyntaxOpt.Name.Location);
		}
		else
		{
			base.DecodeWellKnownAttributeImpl(ref arguments);
		}
	}

	internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Location errorLocation = ErrorLocation;
		if (RefKind == RefKind.In)
		{
			declaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, errorLocation, modifyCompilation: true);
		}
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(base.Type))
		{
			declaringCompilation.EnsureNativeIntegerAttributeExists(diagnostics, errorLocation, modifyCompilation: true);
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this) && base.TypeWithAnnotations.NeedsNullableAttribute())
		{
			declaringCompilation.EnsureNullableAttributeExists(diagnostics, errorLocation, modifyCompilation: true);
		}
	}
}
