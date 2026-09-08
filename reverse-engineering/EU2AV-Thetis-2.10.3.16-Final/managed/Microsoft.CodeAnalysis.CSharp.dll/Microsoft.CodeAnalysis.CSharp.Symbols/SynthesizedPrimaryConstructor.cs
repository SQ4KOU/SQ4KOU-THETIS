using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedPrimaryConstructor : SourceConstructorSymbolBase
{
	private IReadOnlyDictionary<ParameterSymbol, FieldSymbol>? _capturedParameters;

	private IReadOnlySet<ParameterSymbol>? _parametersPassedToTheBase;

	protected override IAttributeTargetSymbol AttributeOwner => (IAttributeTargetSymbol)ContainingType;

	protected override AttributeLocation AttributeLocationForLoadAndValidateAttributes => AttributeLocation.Method;

	public new SourceMemberContainerTypeSymbol ContainingType => (SourceMemberContainerTypeSymbol)base.ContainingType;

	protected override bool AllowRefOrOut
	{
		get
		{
			SourceMemberContainerTypeSymbol containingType = ContainingType;
			bool flag = (((object)containingType != null && (containingType.IsRecord || containingType.IsRecordStruct)) ? true : false);
			return !flag;
		}
	}

	public SynthesizedPrimaryConstructor(SourceMemberContainerTypeSymbol containingType, TypeDeclarationSyntax syntax)
		: base(containingType, syntax.Identifier.GetLocation(), syntax, isIterator: false, MakeModifiersAndFlags(containingType, syntax))
	{
		PrimaryConstructorBaseTypeSyntax primaryConstructorBaseTypeIfClass = syntax.PrimaryConstructorBaseTypeIfClass;
		if (primaryConstructorBaseTypeIfClass != null)
		{
			ArgumentListSyntax argumentList = primaryConstructorBaseTypeIfClass.ArgumentList;
			if (argumentList != null && argumentList.Arguments.Count != 0)
			{
				return;
			}
		}
		_parametersPassedToTheBase = SpecializedCollections.EmptyReadOnlySet<ParameterSymbol>();
	}

	private static (DeclarationModifiers, Flags) MakeModifiersAndFlags(SourceMemberContainerTypeSymbol containingType, TypeDeclarationSyntax syntax)
	{
		DeclarationModifiers declarationModifiers = (containingType.IsAbstract ? DeclarationModifiers.Protected : DeclarationModifiers.Public);
		Flags item = SourceMemberMethodSymbol.MakeFlags(MethodKind.Constructor, RefKind.None, declarationModifiers, returnsVoid: true, returnsVoidIsSet: true, isExpressionBodied: false, isExtensionMethod: false, isNullableAnalysisEnabled: false, syntax.ParameterList.IsVarArg(), isExplicitInterfaceImplementation: false, hasThisInitializer: false);
		return (declarationModifiers, item);
	}

	internal TypeDeclarationSyntax GetSyntax()
	{
		return (TypeDeclarationSyntax)syntaxReferenceOpt.GetSyntax();
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return new OneOrMany<SyntaxList<AttributeListSyntax>>(((SourceNamedTypeSymbol)ContainingType).GetAttributeDeclarations());
	}

	protected override ParameterListSyntax GetParameterList()
	{
		return GetSyntax().ParameterList;
	}

	protected override CSharpSyntaxNode? GetInitializer()
	{
		return GetSyntax().PrimaryConstructorBaseTypeIfClass;
	}

	internal override bool IsNullableAnalysisEnabled()
	{
		return ContainingType.IsNullableEnabledForConstructorsAndInitializers(IsStatic);
	}

	protected override bool IsWithinExpressionOrBlockBody(int position, out int offset)
	{
		offset = -1;
		return false;
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		TypeDeclarationSyntax syntax = GetSyntax();
		InMethodBinder primaryConstructorInMethodBinder = (binderFactoryOpt ?? DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree)).GetPrimaryConstructorInMethodBinder(this);
		return new ExecutableCodeBinder(base.SyntaxNode, this, primaryConstructorInMethodBinder.WithAdditionalFlags(ignoreAccessibility ? BinderFlags.IgnoreAccessibility : BinderFlags.None));
	}

	public IEnumerable<FieldSymbol> GetBackingFields()
	{
		IReadOnlyDictionary<ParameterSymbol, FieldSymbol> capturedParameters = GetCapturedParameters();
		if (capturedParameters.Count == 0)
		{
			return SpecializedCollections.EmptyEnumerable<FieldSymbol>();
		}
		return from pair in capturedParameters
			orderby pair.Key.Ordinal
			select pair.Value;
	}

	public IReadOnlyDictionary<ParameterSymbol, FieldSymbol> GetCapturedParameters()
	{
		if (_capturedParameters != null)
		{
			return _capturedParameters;
		}
		SourceMemberContainerTypeSymbol containingType = ContainingType;
		bool flag = (((object)containingType != null && (containingType.IsRecord || containingType.IsRecordStruct)) ? true : false);
		if (flag || ParameterCount == 0)
		{
			_capturedParameters = SpecializedCollections.EmptyReadOnlyDictionary<ParameterSymbol, FieldSymbol>();
			return _capturedParameters;
		}
		Interlocked.CompareExchange(ref _capturedParameters, Binder.CapturedParametersFinder.GetCapturedParameters(this), null);
		return _capturedParameters;
	}

	internal override (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		arguments.SymbolPart = AttributeLocation.None;
		(CSharpAttributeData?, BoundAttribute?) result = base.EarlyDecodeWellKnownAttribute(ref arguments);
		arguments.SymbolPart = AttributeLocation.Method;
		return result;
	}

	protected override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		arguments.SymbolPart = AttributeLocation.None;
		base.DecodeWellKnownAttributeImpl(ref arguments);
		arguments.SymbolPart = AttributeLocation.Method;
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, (symbolPart != AttributeLocation.Method) ? symbolPart : AttributeLocation.None, decodedData);
	}

	protected override bool ShouldBindAttributes(AttributeListSyntax attributeDeclarationSyntax, BindingDiagnosticBag diagnostics)
	{
		if (!base.ShouldBindAttributes(attributeDeclarationSyntax, diagnostics))
		{
			return false;
		}
		if (attributeDeclarationSyntax.SyntaxTree == base.SyntaxRef.SyntaxTree && GetSyntax().AttributeLists.Contains(attributeDeclarationSyntax))
		{
			SourceMemberContainerTypeSymbol containingType = ContainingType;
			if (((object)containingType != null && (containingType.IsRecord || containingType.IsRecordStruct)) ? true : false)
			{
				MessageID.IDS_FeaturePrimaryConstructors.CheckFeatureAvailability(diagnostics, attributeDeclarationSyntax, attributeDeclarationSyntax.Target.Identifier.GetLocation());
			}
			return true;
		}
		SyntaxToken identifier = attributeDeclarationSyntax.Target.Identifier;
		diagnostics.Add(ErrorCode.WRN_AttributeLocationOnBadDeclaration, identifier.GetLocation(), identifier.ToString(), (AttributeOwner.AllowedAttributeLocations & ~AttributeLocation.Method).ToDisplayString());
		return false;
	}

	public IReadOnlySet<ParameterSymbol> GetParametersPassedToTheBase()
	{
		if (_parametersPassedToTheBase != null)
		{
			return _parametersPassedToTheBase;
		}
		TryGetBodyBinder().BindConstructorInitializer(GetSyntax().PrimaryConstructorBaseTypeIfClass, BindingDiagnosticBag.Discarded);
		if (_parametersPassedToTheBase == null)
		{
			_parametersPassedToTheBase = SpecializedCollections.EmptyReadOnlySet<ParameterSymbol>();
		}
		return _parametersPassedToTheBase;
	}

	internal void SetParametersPassedToTheBase(IReadOnlySet<ParameterSymbol> value)
	{
		_parametersPassedToTheBase = value;
	}
}
