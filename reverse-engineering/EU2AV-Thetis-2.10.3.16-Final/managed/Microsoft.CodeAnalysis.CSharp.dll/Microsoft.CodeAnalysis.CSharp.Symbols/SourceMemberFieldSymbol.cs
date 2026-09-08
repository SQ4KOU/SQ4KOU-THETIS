using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMemberFieldSymbol : SourceFieldSymbolWithSyntaxReference
{
	private readonly DeclarationModifiers _modifiers;

	protected sealed override DeclarationModifiers Modifiers => _modifiers;

	protected abstract TypeSyntax TypeSyntax { get; }

	protected abstract SyntaxTokenList ModifiersTokenList { get; }

	public abstract bool HasInitializer { get; }

	public override Symbol AssociatedSymbol => null;

	public override int FixedSize
	{
		get
		{
			state.NotePartComplete(CompletionPart.Members);
			return 0;
		}
	}

	internal SourceMemberFieldSymbol(SourceMemberContainerTypeSymbol containingType, DeclarationModifiers modifiers, string name, SyntaxReference syntax, TextSpan locationSpan)
		: base(containingType, name, syntax, locationSpan)
	{
		_modifiers = modifiers;
	}

	protected void TypeChecks(TypeSymbol type, BindingDiagnosticBag diagnostics)
	{
		if (type.HasFileLocalTypes() && !ContainingType.HasFileLocalTypes())
		{
			diagnostics.Add(ErrorCode.ERR_FileTypeDisallowedInSignature, ErrorLocation, type, ContainingType);
		}
		else if (type.IsStatic)
		{
			diagnostics.Add(ErrorCode.ERR_VarDeclIsStaticClass, ErrorLocation, type);
		}
		else if (type.IsVoidType())
		{
			diagnostics.Add(ErrorCode.ERR_FieldCantHaveVoidType, getTypeErrorLocation());
		}
		else if (type.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_FieldCantBeRefAny, getTypeErrorLocation(), type);
		}
		else if (type.IsRefLikeOrAllowsRefLikeType() && (IsStatic || !containingType.IsRefLikeType))
		{
			diagnostics.Add(ErrorCode.ERR_FieldAutoPropCantBeByRefLike, getTypeErrorLocation(), type);
		}
		else if (!IsStatic && (ContainingType.IsRecord || ContainingType.IsRecordStruct) && type.IsPointerOrFunctionPointer())
		{
			diagnostics.Add(ErrorCode.ERR_BadFieldTypeInRecord, getTypeErrorLocation(), type);
		}
		else if (IsConst && !type.CanBeConst())
		{
			SyntaxToken syntaxToken = default(SyntaxToken);
			foreach (SyntaxToken modifiersToken in ModifiersTokenList)
			{
				if (modifiersToken.Kind() == SyntaxKind.ConstKeyword)
				{
					syntaxToken = modifiersToken;
					break;
				}
			}
			diagnostics.Add(ErrorCode.ERR_BadConstType, syntaxToken.GetLocation(), type);
		}
		else if (IsVolatile && !type.IsValidVolatileFieldType())
		{
			diagnostics.Add(ErrorCode.ERR_VolatileStruct, ErrorLocation, this, type);
		}
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (!this.IsNoMoreVisibleThan(type, ref useSiteInfo))
		{
			diagnostics.Add(ErrorCode.ERR_BadVisFieldType, ErrorLocation, this, type);
		}
		diagnostics.Add(ErrorLocation, useSiteInfo);
		Location getTypeErrorLocation()
		{
			return TypeSyntax?.Location ?? GetFirstLocation();
		}
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ConstantValue constantValue = GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
		if (IsConst && constantValue != null && base.Type.SpecialType == SpecialType.System_Decimal)
		{
			FieldWellKnownAttributeData decodedWellKnownAttributeData = GetDecodedWellKnownAttributeData();
			if (decodedWellKnownAttributeData == null || decodedWellKnownAttributeData.ConstValue == Microsoft.CodeAnalysis.ConstantValue.Unset)
			{
				Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDecimalConstantAttribute(constantValue.DecimalValue));
			}
		}
		if (IsRequired)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, DeclaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_RequiredMemberAttribute__ctor));
		}
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
		if (IsConst && base.Type.SpecialType == SpecialType.System_Decimal && (object)GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false) != null && (!(decodedData is FieldWellKnownAttributeData fieldWellKnownAttributeData) || !(fieldWellKnownAttributeData.ConstValue != Microsoft.CodeAnalysis.ConstantValue.Unset)))
		{
			Binder.ReportUseSiteDiagnosticForSynthesizedAttribute(DeclaringCompilation, WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctor, diagnostics, null, base.SyntaxNode);
		}
	}

	internal static DeclarationModifiers MakeModifiers(NamedTypeSymbol containingType, SyntaxToken firstIdentifier, SyntaxTokenList modifiers, bool isRefField, BindingDiagnosticBag diagnostics, out bool modifierErrors)
	{
		bool isInterface = containingType.IsInterface;
		DeclarationModifiers defaultAccess = (isInterface ? DeclarationModifiers.Public : DeclarationModifiers.Private);
		DeclarationModifiers allowedModifiers = DeclarationModifiers.AccessibilityMask | DeclarationModifiers.Abstract | DeclarationModifiers.Static | DeclarationModifiers.New | DeclarationModifiers.ReadOnly | DeclarationModifiers.Const | DeclarationModifiers.Volatile | DeclarationModifiers.Unsafe | DeclarationModifiers.Fixed | DeclarationModifiers.Required;
		SourceLocation sourceLocation = new SourceLocation(in firstIdentifier);
		DeclarationModifiers declarationModifiers = ModifierUtils.MakeAndCheckNonTypeMemberModifiers(isOrdinaryMethod: false, isInterface, modifiers, defaultAccess, allowedModifiers, sourceLocation, diagnostics, out modifierErrors, out var _);
		if ((declarationModifiers & DeclarationModifiers.Abstract) != DeclarationModifiers.None)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractField, sourceLocation);
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFFFFFFEu);
		}
		if ((declarationModifiers & DeclarationModifiers.Fixed) != DeclarationModifiers.None)
		{
			foreach (SyntaxToken item in modifiers)
			{
				if (item.IsKind(SyntaxKind.FixedKeyword))
				{
					MessageID.IDS_FeatureFixedBuffer.CheckFeatureAvailability(diagnostics, item);
				}
			}
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Static, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.ReadOnly, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Const, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Volatile, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Required, diagnostics, sourceLocation);
			declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFBFE3FBu);
		}
		if ((declarationModifiers & DeclarationModifiers.Const) != DeclarationModifiers.None)
		{
			if ((declarationModifiers & DeclarationModifiers.Static) != DeclarationModifiers.None)
			{
				diagnostics.Add(ErrorCode.ERR_StaticConstant, sourceLocation, firstIdentifier.ValueText);
			}
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.ReadOnly, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Volatile, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Unsafe, diagnostics, sourceLocation);
			if (reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Required, diagnostics, sourceLocation))
			{
				declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFBFFFFFu);
			}
			declarationModifiers |= DeclarationModifiers.Static;
		}
		else
		{
			if ((declarationModifiers & DeclarationModifiers.Static) != DeclarationModifiers.None && (declarationModifiers & DeclarationModifiers.Required) != DeclarationModifiers.None)
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, sourceLocation, SyntaxFacts.GetText(SyntaxKind.RequiredKeyword));
				declarationModifiers = (DeclarationModifiers)((uint)declarationModifiers & 0xFFBFFFFFu);
			}
			containingType.CheckUnsafeModifier(declarationModifiers, sourceLocation, diagnostics);
		}
		if (isRefField)
		{
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Static, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Const, diagnostics, sourceLocation);
			reportBadMemberFlagIfAny(declarationModifiers, DeclarationModifiers.Volatile, diagnostics, sourceLocation);
		}
		return declarationModifiers;
		static bool reportBadMemberFlagIfAny(DeclarationModifiers result, DeclarationModifiers modifier, BindingDiagnosticBag bindingDiagnosticBag, SourceLocation errorLocation)
		{
			if ((result & modifier) != DeclarationModifiers.None)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_BadMemberFlag, errorLocation, ModifierUtils.ConvertSingleModifierToSyntaxText(modifier));
				return true;
			}
			return false;
		}
	}

	internal sealed override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		if (filter != null && !filter(this))
		{
			return;
		}
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.Type:
				GetFieldType(ConsList<FieldSymbol>.Empty);
				break;
			case CompletionPart.Members:
				_ = FixedSize;
				break;
			case CompletionPart.TypeMembers:
				GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete(CompletionPart.ImportsAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.TypeParameters | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompletedChecksStarted | CompletionPart.MembersCompleted);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override NamedTypeSymbol FixedImplementationType(PEModuleBuilder emitModule)
	{
		return null;
	}
}
