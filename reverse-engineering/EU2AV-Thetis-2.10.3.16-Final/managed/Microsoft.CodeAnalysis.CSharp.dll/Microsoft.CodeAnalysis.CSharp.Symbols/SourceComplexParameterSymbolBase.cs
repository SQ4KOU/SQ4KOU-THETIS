using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceComplexParameterSymbolBase : SourceParameterSymbol, IAttributeTargetSymbol
{
	[Flags]
	private enum ParameterFlags : byte
	{
		None = 0,
		HasParamsModifier = 1,
		ParamsParameter = 2,
		ExtensionThisParameter = 4,
		DefaultParameter = 8
	}

	private readonly SyntaxReference _syntaxRef;

	private readonly ParameterFlags _parameterSyntaxKind;

	private ThreeState _lazyHasOptionalAttribute;

	private CustomAttributesBag<CSharpAttributeData> _lazyCustomAttributesBag;

	protected ConstantValue? _lazyDefaultSyntaxValue;

	private Binder WithTypeParametersBinderOpt => (ContainingSymbol as SourceMethodSymbol)?.WithTypeParametersBinder;

	internal sealed override SyntaxReference SyntaxReference => _syntaxRef;

	private ParameterSyntax ParameterSyntax => (ParameterSyntax)(_syntaxRef?.GetSyntax());

	public override bool IsDiscard => false;

	internal sealed override ConstantValue? ExplicitDefaultConstantValue => DefaultSyntaxValue ?? DefaultValueFromAttributes;

	internal sealed override ConstantValue? DefaultValueFromAttributes
	{
		get
		{
			ParameterEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
			if (earlyDecodedWellKnownAttributeData == null || !(earlyDecodedWellKnownAttributeData.DefaultParameterValue != ConstantValue.Unset))
			{
				return null;
			}
			return earlyDecodedWellKnownAttributeData.DefaultParameterValue;
		}
	}

	internal sealed override bool IsIDispatchConstant => GetDecodedWellKnownAttributeData()?.HasIDispatchConstantAttribute ?? false;

	internal override bool IsIUnknownConstant => GetDecodedWellKnownAttributeData()?.HasIUnknownConstantAttribute ?? false;

	internal override bool IsCallerLineNumber => GetEarlyDecodedWellKnownAttributeData()?.HasCallerLineNumberAttribute ?? false;

	internal override bool IsCallerFilePath => GetEarlyDecodedWellKnownAttributeData()?.HasCallerFilePathAttribute ?? false;

	internal override bool IsCallerMemberName => GetEarlyDecodedWellKnownAttributeData()?.HasCallerMemberNameAttribute ?? false;

	internal override int CallerArgumentExpressionParameterIndex => GetEarlyDecodedWellKnownAttributeData()?.CallerArgumentExpressionParameterIndex ?? (-1);

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => (GetDecodedWellKnownAttributeData()?.InterpolatedStringHandlerArguments).NullToEmpty();

	internal override bool HasInterpolatedStringHandlerArgumentError => GetDecodedWellKnownAttributeData()?.InterpolatedStringHandlerArguments.IsDefault ?? false;

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations => DecodeFlowAnalysisAttributes(GetDecodedWellKnownAttributeData());

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => GetDecodedWellKnownAttributeData()?.NotNullIfParameterNotNull ?? ImmutableHashSet<string>.Empty;

	internal override bool HasEnumeratorCancellationAttribute => GetDecodedWellKnownAttributeData()?.HasEnumeratorCancellationAttribute ?? false;

	internal sealed override ScopedKind EffectiveScope
	{
		get
		{
			ScopedKind scopedKind = CalculateEffectiveScopeIgnoringAttributes();
			if (scopedKind != ScopedKind.None && HasUnscopedRefAttribute && UseUpdatedEscapeRules)
			{
				return ScopedKind.None;
			}
			return scopedKind;
		}
	}

	internal override bool HasUnscopedRefAttribute => GetEarlyDecodedWellKnownAttributeData()?.HasUnscopedRefAttribute ?? false;

	private ConstantValue? DefaultSyntaxValue
	{
		get
		{
			if (state.NotePartComplete(CompletionPart.Members))
			{
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
				Interlocked.CompareExchange(ref _lazyDefaultSyntaxValue, MakeDefaultExpression(instance, out Binder binder, out BoundParameterEqualsValue parameterEqualsValue), ConstantValue.Unset);
				state.NotePartComplete(CompletionPart.TypeMembers);
				if (parameterEqualsValue != null)
				{
					if (binder != null)
					{
						SyntaxNode defaultValueSyntaxForIsNullableAnalysisEnabled = GetDefaultValueSyntaxForIsNullableAnalysisEnabled(ParameterSyntax);
						if (defaultValueSyntaxForIsNullableAnalysisEnabled != null)
						{
							NullableWalker.AnalyzeIfNeeded(binder, parameterEqualsValue, defaultValueSyntaxForIsNullableAnalysisEnabled, instance.DiagnosticBag);
						}
					}
					if (!_lazyDefaultSyntaxValue.IsBad)
					{
						VerifyParamDefaultValueMatchesAttributeIfAny(_lazyDefaultSyntaxValue, parameterEqualsValue.Value.Syntax, instance);
						if (_lazyDefaultSyntaxValue.IsDecimal && DefaultValueFromAttributes == null)
						{
							Binder.ReportUseSiteDiagnosticForSynthesizedAttribute(DeclaringCompilation, WellKnownMember.System_Runtime_CompilerServices_DecimalConstantAttribute__ctor, instance, parameterEqualsValue.Value.Syntax.Location);
						}
					}
				}
				AddDeclarationDiagnostics(instance);
				instance.Free();
				state.NotePartComplete(CompletionPart.SynthesizedExplicitImplementations);
			}
			state.SpinWaitComplete(CompletionPart.TypeMembers, default(CancellationToken));
			return _lazyDefaultSyntaxValue;
		}
	}

	public override string MetadataName
	{
		get
		{
			if (!(ContainingSymbol is SourceOrdinaryMethodSymbol { SourcePartialDefinition: var sourcePartialDefinition }))
			{
				return base.MetadataName;
			}
			if ((object)sourcePartialDefinition == null)
			{
				return base.MetadataName;
			}
			return sourcePartialDefinition.Parameters[Ordinal].MetadataName;
		}
	}

	protected virtual IAttributeTargetSymbol AttributeOwner => this;

	IAttributeTargetSymbol IAttributeTargetSymbol.AttributesOwner => AttributeOwner;

	AttributeLocation IAttributeTargetSymbol.DefaultAttributeLocation => AttributeLocation.Parameter;

	AttributeLocation IAttributeTargetSymbol.AllowedAttributeLocations
	{
		get
		{
			if (SynthesizedRecordPropertySymbol.HaveCorrespondingSynthesizedRecordPropertySymbol(this))
			{
				return AttributeLocation.Field | AttributeLocation.Property | AttributeLocation.Parameter;
			}
			return AttributeLocation.Parameter;
		}
	}

	private SourceParameterSymbol? BoundAttributesSource => PartialImplementationPart;

	protected SourceParameterSymbol? PartialImplementationPart
	{
		get
		{
			ImmutableArray<ParameterSymbol> immutableArray = ContainingSymbol.GetPartialImplementationPart()?.GetParameters() ?? default(ImmutableArray<ParameterSymbol>);
			if (immutableArray.IsDefault)
			{
				return null;
			}
			return (SourceParameterSymbol)immutableArray[Ordinal];
		}
	}

	protected SourceParameterSymbol? PartialDefinitionPart
	{
		get
		{
			ImmutableArray<ParameterSymbol> immutableArray = ContainingSymbol.GetPartialDefinitionPart()?.GetParameters() ?? default(ImmutableArray<ParameterSymbol>);
			if (immutableArray.IsDefault)
			{
				return null;
			}
			return (SourceParameterSymbol)immutableArray[Ordinal];
		}
	}

	internal sealed override SyntaxList<AttributeListSyntax> AttributeDeclarationList => ParameterSyntax?.AttributeLists ?? default(SyntaxList<AttributeListSyntax>);

	internal override bool HasDefaultArgumentSyntax => (_parameterSyntaxKind & ParameterFlags.DefaultParameter) != 0;

	internal sealed override bool HasOptionalAttribute
	{
		get
		{
			if (_lazyHasOptionalAttribute == ThreeState.Unknown)
			{
				SourceParameterSymbol boundAttributesSource = BoundAttributesSource;
				if ((object)boundAttributesSource != null)
				{
					_lazyHasOptionalAttribute = boundAttributesSource.HasOptionalAttribute.ToThreeState();
				}
				else if (!GetAttributes().Any())
				{
					_lazyHasOptionalAttribute = ThreeState.False;
				}
			}
			return _lazyHasOptionalAttribute.Value();
		}
	}

	internal override bool IsMetadataOptional
	{
		get
		{
			if (!HasDefaultArgumentSyntax)
			{
				return HasOptionalAttribute;
			}
			return true;
		}
	}

	internal sealed override bool IsMetadataIn
	{
		get
		{
			if (!base.IsMetadataIn)
			{
				return GetDecodedWellKnownAttributeData()?.HasInAttribute ?? false;
			}
			return true;
		}
	}

	internal sealed override bool IsMetadataOut
	{
		get
		{
			if (!base.IsMetadataOut)
			{
				return GetDecodedWellKnownAttributeData()?.HasOutAttribute ?? false;
			}
			return true;
		}
	}

	internal sealed override MarshalPseudoCustomAttributeData MarshallingInformation => GetDecodedWellKnownAttributeData()?.MarshallingInformation;

	protected sealed override bool HasParamsModifier => (_parameterSyntaxKind & ParameterFlags.HasParamsModifier) != 0;

	public sealed override bool IsParamsArray
	{
		get
		{
			if ((_parameterSyntaxKind & ParameterFlags.ParamsParameter) != ParameterFlags.None)
			{
				return base.Type.IsSZArray();
			}
			return false;
		}
	}

	public sealed override bool IsParamsCollection
	{
		get
		{
			if ((_parameterSyntaxKind & ParameterFlags.ParamsParameter) != ParameterFlags.None)
			{
				return !base.Type.IsSZArray();
			}
			return false;
		}
	}

	internal override bool IsExtensionMethodThis => (_parameterSyntaxKind & ParameterFlags.ExtensionThisParameter) != 0;

	public abstract override ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	protected SourceComplexParameterSymbolBase(Symbol owner, int ordinal, RefKind refKind, string name, Location location, SyntaxReference syntaxRef, bool hasParamsModifier, bool isParams, bool isExtensionMethodThis, ScopedKind scope)
		: base(owner, ordinal, refKind, scope, name, location)
	{
		_lazyHasOptionalAttribute = ThreeState.Unknown;
		_syntaxRef = syntaxRef;
		if (hasParamsModifier)
		{
			_parameterSyntaxKind |= ParameterFlags.HasParamsModifier;
		}
		if (isParams)
		{
			_parameterSyntaxKind |= ParameterFlags.ParamsParameter;
		}
		if (isExtensionMethodThis)
		{
			_parameterSyntaxKind |= ParameterFlags.ExtensionThisParameter;
		}
		ParameterSyntax parameterSyntax = ParameterSyntax;
		if (parameterSyntax != null && parameterSyntax.Default != null)
		{
			_parameterSyntaxKind |= ParameterFlags.DefaultParameter;
		}
		_lazyDefaultSyntaxValue = ConstantValue.Unset;
	}

	private static FlowAnalysisAnnotations DecodeFlowAnalysisAttributes(ParameterWellKnownAttributeData attributeData)
	{
		if (attributeData == null)
		{
			return FlowAnalysisAnnotations.None;
		}
		FlowAnalysisAnnotations flowAnalysisAnnotations = FlowAnalysisAnnotations.None;
		if (attributeData.HasAllowNullAttribute)
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.AllowNull;
		}
		if (attributeData.HasDisallowNullAttribute)
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.DisallowNull;
		}
		bool? maybeNullWhenAttribute;
		if (attributeData.HasMaybeNullAttribute)
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.MaybeNull;
		}
		else
		{
			maybeNullWhenAttribute = attributeData.MaybeNullWhenAttribute;
			if (maybeNullWhenAttribute.HasValue)
			{
				bool valueOrDefault = maybeNullWhenAttribute == true;
				flowAnalysisAnnotations = (FlowAnalysisAnnotations)((int)flowAnalysisAnnotations | (valueOrDefault ? 4 : 8));
			}
		}
		if (attributeData.HasNotNullAttribute)
		{
			flowAnalysisAnnotations |= FlowAnalysisAnnotations.NotNull;
		}
		else
		{
			maybeNullWhenAttribute = attributeData.NotNullWhenAttribute;
			if (maybeNullWhenAttribute.HasValue)
			{
				bool valueOrDefault2 = maybeNullWhenAttribute == true;
				flowAnalysisAnnotations = (FlowAnalysisAnnotations)((int)flowAnalysisAnnotations | (valueOrDefault2 ? 16 : 32));
			}
		}
		maybeNullWhenAttribute = attributeData.DoesNotReturnIfAttribute;
		if (maybeNullWhenAttribute.HasValue)
		{
			bool valueOrDefault3 = maybeNullWhenAttribute == true;
			flowAnalysisAnnotations = (FlowAnalysisAnnotations)((int)flowAnalysisAnnotations | (valueOrDefault3 ? 128 : 64));
		}
		return flowAnalysisAnnotations;
	}

	internal static SyntaxNode? GetDefaultValueSyntaxForIsNullableAnalysisEnabled(ParameterSyntax? parameterSyntax)
	{
		return parameterSyntax?.Default?.Value;
	}

	public BoundParameterEqualsValue? BindParameterEqualsValue()
	{
		MakeDefaultExpression(BindingDiagnosticBag.Discarded, out Binder _, out BoundParameterEqualsValue parameterEqualsValue);
		return parameterEqualsValue;
	}

	private Binder GetDefaultParameterValueBinder(SyntaxNode syntax)
	{
		Binder binder = WithTypeParametersBinderOpt;
		if (binder == null)
		{
			binder = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax);
		}
		return binder;
	}

	private void NullableAnalyzeParameterDefaultValueFromAttributes()
	{
		ParameterSyntax parameterSyntax = ParameterSyntax;
		if (parameterSyntax == null)
		{
			return;
		}
		SyntaxNode node = parameterSyntax.AttributeLists.Node;
		if (node != null && NullableWalker.NeedsAnalysis(DeclaringCompilation, node))
		{
			ConstantValue defaultValueFromAttributes = DefaultValueFromAttributes;
			if (!(defaultValueFromAttributes == null) && !defaultValueFromAttributes.IsBad)
			{
				Binder defaultParameterValueBinder = GetDefaultParameterValueBinder(parameterSyntax);
				BoundParameterEqualsValue node2 = new BoundParameterEqualsValue(parameterSyntax, this, ImmutableArray<LocalSymbol>.Empty, new BoundLiteral(parameterSyntax, defaultValueFromAttributes, base.Type));
				BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance(withDiagnostics: true, withDependencies: false);
				NullableWalker.AnalyzeIfNeeded(defaultParameterValueBinder, node2, parameterSyntax, instance.DiagnosticBag);
				AddDeclarationDiagnostics(instance);
				instance.Free();
			}
		}
	}

	private ConstantValue? MakeDefaultExpression(BindingDiagnosticBag diagnostics, out Binder? binder, out BoundParameterEqualsValue? parameterEqualsValue)
	{
		binder = null;
		parameterEqualsValue = null;
		ParameterSyntax parameterSyntax = ParameterSyntax;
		if (parameterSyntax == null)
		{
			return null;
		}
		EqualsValueClauseSyntax equalsValueClauseSyntax = parameterSyntax.Default;
		if (equalsValueClauseSyntax == null)
		{
			return null;
		}
		MessageID.IDS_FeatureOptionalParameter.CheckFeatureAvailability(diagnostics, equalsValueClauseSyntax.EqualsToken);
		binder = GetDefaultParameterValueBinder(equalsValueClauseSyntax);
		binder = binder.CreateBinderForParameterDefaultValue(this, equalsValueClauseSyntax);
		parameterEqualsValue = binder.BindParameterDefaultValue(equalsValueClauseSyntax, this, diagnostics, out var valueBeforeConversion);
		if (valueBeforeConversion.HasErrors)
		{
			return ConstantValue.Bad;
		}
		BoundExpression boundExpression = parameterEqualsValue.Value;
		if (ParameterHelpers.ReportDefaultParameterErrors(binder, ContainingSymbol, parameterSyntax, this, valueBeforeConversion, boundExpression, diagnostics))
		{
			return ConstantValue.Bad;
		}
		if (boundExpression.ConstantValueOpt == null && boundExpression.Kind == BoundKind.Conversion && ((BoundConversion)boundExpression).ConversionKind != ConversionKind.DefaultLiteral && base.Type.IsNullableType())
		{
			boundExpression = binder.GenerateConversionForAssignment(base.Type.GetNullableUnderlyingType(), valueBeforeConversion, diagnostics, Binder.ConversionForAssignmentFlags.DefaultParameter);
		}
		return boundExpression.ConstantValueOpt ?? ConstantValue.Null;
	}

	internal virtual OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		SourceParameterSymbol partialDefinitionPart = PartialDefinitionPart;
		if ((object)partialDefinitionPart != null)
		{
			return OneOrMany.Create<SyntaxList<AttributeListSyntax>>(AttributeDeclarationList, partialDefinitionPart.AttributeDeclarationList);
		}
		return OneOrMany.Create(AttributeDeclarationList);
	}

	internal ParameterWellKnownAttributeData GetDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (ParameterWellKnownAttributeData)customAttributesBag.DecodedWellKnownAttributeData;
	}

	internal ParameterEarlyWellKnownAttributeData GetEarlyDecodedWellKnownAttributeData()
	{
		CustomAttributesBag<CSharpAttributeData> customAttributesBag = _lazyCustomAttributesBag;
		if (customAttributesBag == null || !customAttributesBag.IsEarlyDecodedWellKnownAttributeDataComputed)
		{
			customAttributesBag = GetAttributesBag();
		}
		return (ParameterEarlyWellKnownAttributeData)customAttributesBag.EarlyDecodedWellKnownAttributeData;
	}

	internal sealed override CustomAttributesBag<CSharpAttributeData> GetAttributesBag()
	{
		if (_lazyCustomAttributesBag == null || !_lazyCustomAttributesBag.IsSealed)
		{
			SourceParameterSymbol boundAttributesSource = BoundAttributesSource;
			bool flag;
			if ((object)boundAttributesSource != null)
			{
				CustomAttributesBag<CSharpAttributeData> attributesBag = boundAttributesSource.GetAttributesBag();
				flag = Interlocked.CompareExchange(ref _lazyCustomAttributesBag, attributesBag, null) == null;
			}
			else
			{
				OneOrMany<SyntaxList<AttributeListSyntax>> attributeDeclarations = GetAttributeDeclarations();
				flag = LoadAndValidateAttributes(attributeDeclarations, ref _lazyCustomAttributesBag, AttributeLocation.None, earlyDecodingOnly: false, WithTypeParametersBinderOpt);
			}
			if (flag)
			{
				NullableAnalyzeParameterDefaultValueFromAttributes();
				state.NotePartComplete(CompletionPart.Attributes);
			}
		}
		return _lazyCustomAttributesBag;
	}

	public ImmutableArray<(CSharpAttributeData, BoundAttribute)> BindParameterAttributes()
	{
		return BindAttributes(GetAttributeDeclarations(), WithTypeParametersBinderOpt);
	}

	internal override void EarlyDecodeWellKnownAttributeType(NamedTypeSymbol attributeType, AttributeSyntax attributeSyntax)
	{
		if (CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.OptionalAttribute))
		{
			_lazyHasOptionalAttribute = ThreeState.True;
		}
	}

	internal override void PostEarlyDecodeWellKnownAttributeTypes()
	{
		if (_lazyHasOptionalAttribute == ThreeState.Unknown)
		{
			_lazyHasOptionalAttribute = ThreeState.False;
		}
		base.PostEarlyDecodeWellKnownAttributeTypes();
	}

	internal override (CSharpAttributeData?, BoundAttribute?) EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.DefaultParameterValueAttribute))
		{
			return EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription.DefaultParameterValueAttribute, ref arguments);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.DecimalConstantAttribute))
		{
			return EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription.DecimalConstantAttribute, ref arguments);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.DateTimeConstantAttribute))
		{
			return EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription.DateTimeConstantAttribute, ref arguments);
		}
		if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.UnscopedRefAttribute))
		{
			arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().HasUnscopedRefAttribute = true;
			return (null, null);
		}
		if (!IsOnPartialImplementation(arguments.AttributeSyntax))
		{
			if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CallerLineNumberAttribute))
			{
				arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().HasCallerLineNumberAttribute = true;
			}
			else if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CallerFilePathAttribute))
			{
				arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().HasCallerFilePathAttribute = true;
			}
			else if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CallerMemberNameAttribute))
			{
				arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().HasCallerMemberNameAttribute = true;
			}
			else if (CSharpAttributeData.IsTargetEarlyAttribute(arguments.AttributeType, arguments.AttributeSyntax, AttributeDescription.CallerArgumentExpressionAttribute))
			{
				int callerArgumentExpressionParameterIndex = -1;
				CSharpAttributeData item = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out var _).Item1;
				if (!item.HasErrors && item.CommonConstructorArguments[0].TryDecodeValue<string>(SpecialType.System_String, out var value) && value != null)
				{
					callerArgumentExpressionParameterIndex = GetCallerArgumentExpressionParameterIndex(this, value);
				}
				arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>().CallerArgumentExpressionParameterIndex = callerArgumentExpressionParameterIndex;
			}
		}
		return base.EarlyDecodeWellKnownAttribute(ref arguments);
	}

	internal static int GetCallerArgumentExpressionParameterIndex(ParameterSymbol parameter, string parameterName)
	{
		int num = 0;
		Symbol containingSymbol = parameter.ContainingSymbol;
		if (containingSymbol.IsExtensionBlockMember() && !containingSymbol.IsStatic)
		{
			ParameterSymbol extensionParameter = parameter.ContainingType.ExtensionParameter;
			if ((object)extensionParameter != null && extensionParameter.Name.Equals(parameterName, StringComparison.Ordinal))
			{
				return 0;
			}
			num = 1;
		}
		ImmutableArray<ParameterSymbol> parameters = containingSymbol.GetParameters();
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].Name.Equals(parameterName, StringComparison.Ordinal))
			{
				return i + num;
			}
		}
		return -1;
	}

	private (CSharpAttributeData?, BoundAttribute?) EarlyDecodeAttributeForDefaultParameterValue(AttributeDescription description, ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		var (cSharpAttributeData, item) = arguments.Binder.GetAttribute(arguments.AttributeSyntax, arguments.AttributeType, null, null, out var generatedDiagnostics);
		ConstantValue defaultParameterValue;
		if (cSharpAttributeData.HasErrors)
		{
			defaultParameterValue = ConstantValue.Bad;
			generatedDiagnostics = true;
		}
		else
		{
			defaultParameterValue = DecodeDefaultParameterValueAttribute(description, cSharpAttributeData, arguments.AttributeSyntax, diagnose: false, null);
		}
		ParameterEarlyWellKnownAttributeData orCreateData = arguments.GetOrCreateData<ParameterEarlyWellKnownAttributeData>();
		if (orCreateData.DefaultParameterValue == ConstantValue.Unset)
		{
			orCreateData.DefaultParameterValue = defaultParameterValue;
		}
		if (generatedDiagnostics)
		{
			return (null, null);
		}
		return (cSharpAttributeData, item);
	}

	protected override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		if (attribute.IsTargetAttribute(AttributeDescription.DefaultParameterValueAttribute))
		{
			DecodeDefaultParameterValueAttribute(AttributeDescription.DefaultParameterValueAttribute, ref arguments);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.DecimalConstantAttribute))
		{
			DecodeDefaultParameterValueAttribute(AttributeDescription.DecimalConstantAttribute, ref arguments);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.DateTimeConstantAttribute))
		{
			DecodeDefaultParameterValueAttribute(AttributeDescription.DateTimeConstantAttribute, ref arguments);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.OptionalAttribute))
		{
			if (HasDefaultArgumentSyntax)
			{
				bindingDiagnosticBag.Add(ErrorCode.ERR_DefaultValueUsedWithAttributes, arguments.AttributeSyntaxOpt.Name.Location);
			}
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.ParamArrayAttribute) || attribute.IsTargetAttribute(AttributeDescription.ParamCollectionAttribute))
		{
			bindingDiagnosticBag.Add(ErrorCode.ERR_ExplicitParamArrayOrCollection, arguments.AttributeSyntaxOpt.Name.Location);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.InAttribute))
		{
			arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasInAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.OutAttribute))
		{
			arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasOutAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.MarshalAsAttribute))
		{
			MarshalAsAttributeDecoder<ParameterWellKnownAttributeData, AttributeSyntax, CSharpAttributeData, AttributeLocation>.Decode(ref arguments, AttributeTargets.Parameter, MessageProvider.Instance);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.IDispatchConstantAttribute))
		{
			arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasIDispatchConstantAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.IUnknownConstantAttribute))
		{
			arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasIUnknownConstantAttribute = true;
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.CallerLineNumberAttribute))
		{
			ValidateCallerLineNumberAttribute(arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.CallerFilePathAttribute))
		{
			ValidateCallerFilePathAttribute(arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.CallerMemberNameAttribute))
		{
			ValidateCallerMemberNameAttribute(arguments.AttributeSyntaxOpt, bindingDiagnosticBag);
		}
		else if (attribute.IsTargetAttribute(AttributeDescription.CallerArgumentExpressionAttribute))
		{
			ValidateCallerArgumentExpressionAttribute(arguments.AttributeSyntaxOpt, attribute, bindingDiagnosticBag);
		}
		else
		{
			if (ReportExplicitUseOfReservedAttributes(in arguments, ReservedAttributes.DynamicAttribute | ReservedAttributes.IsReadOnlyAttribute | ReservedAttributes.IsUnmanagedAttribute | ReservedAttributes.IsByRefLikeAttribute | ReservedAttributes.TupleElementNamesAttribute | ReservedAttributes.NullableAttribute | ReservedAttributes.NativeIntegerAttribute | ReservedAttributes.ScopedRefAttribute | ReservedAttributes.RequiresLocationAttribute | ReservedAttributes.ExtensionMarkerAttribute))
			{
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.AllowNullAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasAllowNullAttribute = true;
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.DisallowNullAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasDisallowNullAttribute = true;
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.MaybeNullAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasMaybeNullAttribute = true;
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.MaybeNullWhenAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().MaybeNullWhenAttribute = DecodeMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(attribute);
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.NotNullAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasNotNullAttribute = true;
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.NotNullWhenAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().NotNullWhenAttribute = DecodeMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(attribute);
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.DoesNotReturnIfAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().DoesNotReturnIfAttribute = DecodeMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(attribute);
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.NotNullIfNotNullAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().AddNotNullIfParameterNotNull(attribute.DecodeNotNullIfNotNullAttribute());
				return;
			}
			if (attribute.IsTargetAttribute(AttributeDescription.EnumeratorCancellationAttribute))
			{
				arguments.GetOrCreateData<ParameterWellKnownAttributeData>().HasEnumeratorCancellationAttribute = true;
				ValidateCancellationTokenAttribute(arguments.AttributeSyntaxOpt, (BindingDiagnosticBag)arguments.Diagnostics);
				return;
			}
			int targetAttributeSignatureIndex = attribute.GetTargetAttributeSignatureIndex(AttributeDescription.InterpolatedStringHandlerArgumentAttribute);
			if ((uint)targetAttributeSignatureIndex <= 1u)
			{
				DecodeInterpolatedStringHandlerArgumentAttribute(ref arguments, bindingDiagnosticBag, targetAttributeSignatureIndex);
			}
			else if (attribute.IsTargetAttribute(AttributeDescription.UnscopedRefAttribute))
			{
				if (!UseUpdatedEscapeRules)
				{
					bindingDiagnosticBag.Add(ErrorCode.WRN_UnscopedRefAttributeOldRules, arguments.AttributeSyntaxOpt.Location);
				}
				if (!IsValidUnscopedRefAttributeTarget())
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_UnscopedRefAttributeUnsupportedTarget, arguments.AttributeSyntaxOpt.Location);
				}
				else if (DeclaredScope != ScopedKind.None)
				{
					bindingDiagnosticBag.Add(ErrorCode.ERR_UnscopedScoped, arguments.AttributeSyntaxOpt.Location);
				}
			}
		}
	}

	private bool IsValidUnscopedRefAttributeTarget()
	{
		if (RefKind == RefKind.None)
		{
			if (HasParamsModifier)
			{
				return base.Type.IsRefLikeOrAllowsRefLikeType();
			}
			return false;
		}
		return true;
	}

	private static bool? DecodeMaybeNullWhenOrNotNullWhenOrDoesNotReturnIfAttribute(CSharpAttributeData attribute)
	{
		ImmutableArray<TypedConstant> commonConstructorArguments = attribute.CommonConstructorArguments;
		if (commonConstructorArguments.Length != 1 || !commonConstructorArguments[0].TryDecodeValue<bool>(SpecialType.System_Boolean, out var value))
		{
			return null;
		}
		return value;
	}

	private void DecodeDefaultParameterValueAttribute(AttributeDescription description, ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		CSharpAttributeData attribute = arguments.Attribute;
		AttributeSyntax attributeSyntaxOpt = arguments.AttributeSyntaxOpt;
		BindingDiagnosticBag bindingDiagnosticBag = (BindingDiagnosticBag)arguments.Diagnostics;
		ConstantValue constantValue = DecodeDefaultParameterValueAttribute(description, attribute, attributeSyntaxOpt, diagnose: true, bindingDiagnosticBag);
		if (!constantValue.IsBad)
		{
			VerifyParamDefaultValueMatchesAttributeIfAny(constantValue, attributeSyntaxOpt, bindingDiagnosticBag);
			if (RefKind == RefKind.RefReadOnlyParameter && base.IsOptional && ParameterSyntax.Default == null)
			{
				bindingDiagnosticBag.Add(ErrorCode.WRN_RefReadonlyParameterDefaultValue, attributeSyntaxOpt, Name);
			}
		}
	}

	private void VerifyParamDefaultValueMatchesAttributeIfAny(ConstantValue value, SyntaxNode syntax, BindingDiagnosticBag diagnostics)
	{
		ParameterEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
		if (earlyDecodedWellKnownAttributeData != null)
		{
			ConstantValue defaultParameterValue = earlyDecodedWellKnownAttributeData.DefaultParameterValue;
			if (defaultParameterValue != ConstantValue.Unset && value != defaultParameterValue)
			{
				diagnostics.Add(ErrorCode.ERR_ParamDefaultValueDiffersFromAttribute, syntax.Location);
			}
		}
	}

	private ConstantValue DecodeDefaultParameterValueAttribute(AttributeDescription description, CSharpAttributeData attribute, AttributeSyntax node, bool diagnose, BindingDiagnosticBag diagnosticsOpt)
	{
		if (description.Equals(AttributeDescription.DefaultParameterValueAttribute))
		{
			return DecodeDefaultParameterValueAttribute(attribute, node, diagnose, diagnosticsOpt);
		}
		if (description.Equals(AttributeDescription.DecimalConstantAttribute))
		{
			return attribute.DecodeDecimalConstantValue();
		}
		return attribute.DecodeDateTimeConstantValue();
	}

	private ConstantValue DecodeDefaultParameterValueAttribute(CSharpAttributeData attribute, AttributeSyntax node, bool diagnose, BindingDiagnosticBag diagnosticsOpt)
	{
		if (HasDefaultArgumentSyntax)
		{
			if (diagnose)
			{
				diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueUsedWithAttributes, node.Name.Location);
			}
			return ConstantValue.Bad;
		}
		TypedConstant typedConstant = attribute.CommonConstructorArguments[0];
		SpecialType st = ((typedConstant.Kind == TypedConstantKind.Enum) ? ((NamedTypeSymbol)typedConstant.TypeInternal).EnumUnderlyingType.SpecialType : typedConstant.TypeInternal.SpecialType);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ConstantValueTypeDiscriminator constantValueTypeDiscriminator = ConstantValue.GetDiscriminator(st);
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnosticsOpt, ContainingAssembly);
		if (constantValueTypeDiscriminator == ConstantValueTypeDiscriminator.Bad)
		{
			if (typedConstant.Kind == TypedConstantKind.Array || typedConstant.ValueInternal != null)
			{
				if (diagnose)
				{
					diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueBadValueType, node.Name.Location, typedConstant.TypeInternal);
				}
				return ConstantValue.Bad;
			}
			if (!base.Type.IsReferenceType)
			{
				if (diagnose)
				{
					diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueTypeMustMatch, node.Name.Location);
				}
				return ConstantValue.Bad;
			}
			constantValueTypeDiscriminator = ConstantValueTypeDiscriminator.Nothing;
		}
		else if (!declaringCompilation.Conversions.ClassifyConversionFromType((TypeSymbol)typedConstant.TypeInternal, base.Type, isChecked: false, ref useSiteInfo).Kind.IsImplicitConversion())
		{
			if (diagnose)
			{
				diagnosticsOpt.Add(ErrorCode.ERR_DefaultValueTypeMustMatch, node.Name.Location);
				diagnosticsOpt.Add(node.Name, useSiteInfo);
			}
			return ConstantValue.Bad;
		}
		if (diagnose)
		{
			diagnosticsOpt.Add(node.Name, useSiteInfo);
		}
		return ConstantValue.Create(typedConstant.ValueInternal, constantValueTypeDiscriminator);
	}

	private bool IsValidCallerInfoContext(AttributeSyntax node)
	{
		if (!ContainingSymbol.IsExplicitInterfaceImplementation() && !ContainingSymbol.IsOperator())
		{
			return !IsOnPartialImplementation(node);
		}
		return false;
	}

	private bool IsOnPartialImplementation(AttributeSyntax node)
	{
		SyntaxList<AttributeListSyntax>? syntaxList = (ContainingSymbol.IsPartialImplementation() ? this : PartialImplementationPart)?.AttributeDeclarationList;
		if (syntaxList.HasValue)
		{
			SyntaxList<AttributeListSyntax> valueOrDefault = syntaxList.GetValueOrDefault();
			return valueOrDefault.Any((AttributeListSyntax attrList) => attrList.Attributes.Contains(node));
		}
		return false;
	}

	private void ValidateCallerLineNumberAttribute(AttributeSyntax node, BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (!IsValidCallerInfoContext(node))
		{
			diagnostics.Add(ErrorCode.WRN_CallerLineNumberParamForUnconsumedLocation, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else if (!declaringCompilation.Conversions.HasCallerLineNumberConversion(TypeWithAnnotations.Type, ref useSiteInfo))
		{
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Int32);
			diagnostics.Add(ErrorCode.ERR_NoConversionForCallerLineNumberParam, node.Name.Location, specialType, TypeWithAnnotations.Type);
		}
		else if (!base.HasExplicitDefaultValue && !ContainingSymbol.IsPartialImplementation())
		{
			diagnostics.Add(ErrorCode.ERR_BadCallerLineNumberParamWithoutDefaultValue, node.Name.Location);
		}
		diagnostics.Add(node.Name, useSiteInfo);
	}

	private void ValidateCallerFilePathAttribute(AttributeSyntax node, BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (!IsValidCallerInfoContext(node))
		{
			diagnostics.Add(ErrorCode.WRN_CallerFilePathParamForUnconsumedLocation, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else if (!declaringCompilation.Conversions.HasCallerInfoStringConversion(TypeWithAnnotations.Type, ref useSiteInfo))
		{
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_String);
			diagnostics.Add(ErrorCode.ERR_NoConversionForCallerFilePathParam, node.Name.Location, specialType, TypeWithAnnotations.Type);
		}
		else if (!base.HasExplicitDefaultValue && !ContainingSymbol.IsPartialImplementation())
		{
			diagnostics.Add(ErrorCode.ERR_BadCallerFilePathParamWithoutDefaultValue, node.Name.Location);
		}
		else if (IsCallerLineNumber)
		{
			diagnostics.Add(ErrorCode.WRN_CallerLineNumberPreferredOverCallerFilePath, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		diagnostics.Add(node.Name, useSiteInfo);
	}

	private void ValidateCallerMemberNameAttribute(AttributeSyntax node, BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (!IsValidCallerInfoContext(node))
		{
			diagnostics.Add(ErrorCode.WRN_CallerMemberNameParamForUnconsumedLocation, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else if (!declaringCompilation.Conversions.HasCallerInfoStringConversion(TypeWithAnnotations.Type, ref useSiteInfo))
		{
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_String);
			diagnostics.Add(ErrorCode.ERR_NoConversionForCallerMemberNameParam, node.Name.Location, specialType, TypeWithAnnotations.Type);
		}
		else if (!base.HasExplicitDefaultValue && !ContainingSymbol.IsPartialImplementation())
		{
			diagnostics.Add(ErrorCode.ERR_BadCallerMemberNameParamWithoutDefaultValue, node.Name.Location);
		}
		else if (IsCallerLineNumber)
		{
			diagnostics.Add(ErrorCode.WRN_CallerLineNumberPreferredOverCallerMemberName, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else if (IsCallerFilePath)
		{
			diagnostics.Add(ErrorCode.WRN_CallerFilePathPreferredOverCallerMemberName, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		diagnostics.Add(node.Name, useSiteInfo);
	}

	private void ValidateCallerArgumentExpressionAttribute(AttributeSyntax node, CSharpAttributeData attribute, BindingDiagnosticBag diagnostics)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = new CompoundUseSiteInfo<AssemblySymbol>(diagnostics, ContainingAssembly);
		if (!IsValidCallerInfoContext(node))
		{
			diagnostics.Add(ErrorCode.WRN_CallerArgumentExpressionParamForUnconsumedLocation, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else if (!declaringCompilation.Conversions.HasCallerInfoStringConversion(TypeWithAnnotations.Type, ref useSiteInfo))
		{
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_String);
			diagnostics.Add(ErrorCode.ERR_NoConversionForCallerArgumentExpressionParam, node.Name.Location, specialType, TypeWithAnnotations.Type);
		}
		else if (!base.HasExplicitDefaultValue && !ContainingSymbol.IsPartialImplementation())
		{
			diagnostics.Add(ErrorCode.ERR_BadCallerArgumentExpressionParamWithoutDefaultValue, node.Name.Location);
		}
		else if (IsCallerLineNumber)
		{
			diagnostics.Add(ErrorCode.WRN_CallerLineNumberPreferredOverCallerArgumentExpression, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else if (IsCallerFilePath)
		{
			diagnostics.Add(ErrorCode.WRN_CallerFilePathPreferredOverCallerArgumentExpression, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else if (IsCallerMemberName)
		{
			diagnostics.Add(ErrorCode.WRN_CallerMemberNamePreferredOverCallerArgumentExpression, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		else
		{
			if (attribute.CommonConstructorArguments.Length == 1)
			{
				ParameterEarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = GetEarlyDecodedWellKnownAttributeData();
				if (earlyDecodedWellKnownAttributeData != null && earlyDecodedWellKnownAttributeData.CallerArgumentExpressionParameterIndex == -1)
				{
					diagnostics.Add(ErrorCode.WRN_CallerArgumentExpressionAttributeHasInvalidParameterName, node.Name.Location, ParameterSyntax.Identifier.ValueText);
					goto IL_027b;
				}
			}
			if (GetEarlyDecodedWellKnownAttributeData()?.CallerArgumentExpressionParameterIndex == getOrdinalIncludingExtensionParameter())
			{
				diagnostics.Add(ErrorCode.WRN_CallerArgumentExpressionAttributeSelfReferential, node.Name.Location, ParameterSyntax.Identifier.ValueText);
			}
		}
		goto IL_027b;
		IL_027b:
		diagnostics.Add(node.Name, useSiteInfo);
		int getOrdinalIncludingExtensionParameter()
		{
			int num = 0;
			Symbol containingSymbol = ContainingSymbol;
			if (containingSymbol.IsExtensionBlockMember() && !containingSymbol.IsStatic)
			{
				num = 1;
			}
			return Ordinal + num;
		}
	}

	private void ValidateCancellationTokenAttribute(AttributeSyntax node, BindingDiagnosticBag diagnostics)
	{
		if (needsReporting())
		{
			diagnostics.Add(ErrorCode.WRN_UnconsumedEnumeratorCancellationAttributeUsage, node.Name.Location, ParameterSyntax.Identifier.ValueText);
		}
		bool needsReporting()
		{
			if (!base.Type.Equals(DeclaringCompilation.GetWellKnownType(WellKnownType.System_Threading_CancellationToken)))
			{
				return true;
			}
			if (ContainingSymbol is MethodSymbol { IsAsync: not false } methodSymbol && methodSymbol.ReturnType.OriginalDefinition.Equals(DeclaringCompilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IAsyncEnumerable_T)))
			{
				return false;
			}
			return true;
		}
	}

	private void DecodeInterpolatedStringHandlerArgumentAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments, BindingDiagnosticBag diagnostics, int attributeIndex)
	{
		ImmutableArray<ParameterSymbol> containingSymbolParameters;
		ParameterSymbol extensionParameter;
		if (base.Type is NamedTypeSymbol { IsInterpolatedStringHandlerType: not false })
		{
			if (this is LambdaParameterSymbol)
			{
				diagnostics.Add(ErrorCode.WRN_InterpolatedStringHandlerArgumentAttributeIgnoredOnLambdaParameters, arguments.AttributeSyntaxOpt.Location);
			}
			if (ContainingSymbol is SynthesizedExtensionMarker)
			{
				diagnostics.Add(ErrorCode.ERR_InterpolatedStringHandlerArgumentDisallowed, arguments.AttributeSyntaxOpt.Location);
				setInterpolatedStringHandlerAttributeError(ref arguments);
				return;
			}
			TypedConstant constant = arguments.Attribute.CommonConstructorArguments[0];
			containingSymbolParameters = ContainingSymbol.GetParameters();
			extensionParameter = ContainingType.ExtensionParameter;
			ImmutableArray<int> interpolatedStringHandlerArguments;
			switch (attributeIndex)
			{
			case 0:
			{
				int? num = decodeName(constant, ref arguments);
				if (num.HasValue)
				{
					int valueOrDefault2 = num.GetValueOrDefault();
					interpolatedStringHandlerArguments = ImmutableArray.Create(valueOrDefault2);
					break;
				}
				setInterpolatedStringHandlerAttributeError(ref arguments);
				return;
			}
			case 1:
			{
				if (constant.IsNull)
				{
					setInterpolatedStringHandlerAttributeError(ref arguments);
					diagnostics.Add(ErrorCode.ERR_NullInvalidInterpolatedStringHandlerArgumentName, arguments.AttributeSyntaxOpt.Location);
					return;
				}
				bool flag = false;
				ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(constant.Values.Length);
				foreach (TypedConstant value in constant.Values)
				{
					int? num = decodeName(value, ref arguments);
					if (num.HasValue)
					{
						int valueOrDefault = num.GetValueOrDefault();
						if (!flag)
						{
							instance.Add(valueOrDefault);
							continue;
						}
					}
					flag = true;
				}
				if (flag)
				{
					instance.Free();
					setInterpolatedStringHandlerAttributeError(ref arguments);
					return;
				}
				interpolatedStringHandlerArguments = instance.ToImmutableAndFree();
				break;
			}
			default:
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Source/SourceComplexParameterSymbol.cs", 1366);
			}
			arguments.GetOrCreateData<ParameterWellKnownAttributeData>().InterpolatedStringHandlerArguments = interpolatedStringHandlerArguments;
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_TypeIsNotAnInterpolatedStringHandlerType, arguments.AttributeSyntaxOpt.Location, base.Type);
			setInterpolatedStringHandlerAttributeError(ref arguments);
		}
		int? decodeName(TypedConstant typedConstant, ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> reference)
		{
			if (typedConstant.IsNull)
			{
				diagnostics.Add(ErrorCode.ERR_NullInvalidInterpolatedStringHandlerArgumentName, reference.AttributeSyntaxOpt.Location);
				return null;
			}
			ITypeSymbolInternal typeInternal = typedConstant.TypeInternal;
			if (typeInternal == null || typeInternal.SpecialType != SpecialType.System_String)
			{
				return null;
			}
			string text = typedConstant.DecodeValue<string>(SpecialType.System_String);
			if (text == "")
			{
				bool flag2 = !ContainingSymbol.RequiresInstanceReceiver();
				if (!flag2)
				{
					bool flag3 = ((ContainingSymbol is MethodSymbol { MethodKind: var methodKind } && ((uint)methodKind <= 1u || methodKind == MethodKind.DelegateInvoke)) ? true : false);
					flag2 = flag3;
				}
				if (flag2 || ContainingSymbol.IsExtensionBlockMember())
				{
					diagnostics.Add(ErrorCode.ERR_NotInstanceInvalidInterpolatedStringHandlerArgumentName, reference.AttributeSyntaxOpt.Location, ContainingSymbol);
					return null;
				}
				return -1;
			}
			if (string.Equals(extensionParameter?.Name, text, StringComparison.Ordinal))
			{
				if (!ContainingSymbol.RequiresInstanceReceiver())
				{
					diagnostics.Add(ErrorCode.ERR_NotInstanceInvalidInterpolatedStringHandlerArgumentName, reference.AttributeSyntaxOpt.Location, ContainingSymbol);
					return null;
				}
				return -2;
			}
			ParameterSymbol parameterSymbol = containingSymbolParameters.FirstOrDefault((ParameterSymbol param, string name) => string.Equals(param.Name, name, StringComparison.Ordinal), text);
			if ((object)parameterSymbol == null)
			{
				diagnostics.Add(ErrorCode.ERR_InvalidInterpolatedStringHandlerArgumentName, reference.AttributeSyntaxOpt.Location, text, ContainingSymbol);
				return null;
			}
			if ((object)parameterSymbol == this)
			{
				diagnostics.Add(ErrorCode.ERR_CannotUseSelfAsInterpolatedStringHandlerArgument, reference.AttributeSyntaxOpt.Location);
				return null;
			}
			if (parameterSymbol.Ordinal > Ordinal)
			{
				diagnostics.Add(ErrorCode.WRN_ParameterOccursAfterInterpolatedStringHandlerParameter, reference.AttributeSyntaxOpt.Location, parameterSymbol.Name, Name);
			}
			return parameterSymbol.Ordinal;
		}
		static void setInterpolatedStringHandlerAttributeError(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> reference)
		{
			reference.GetOrCreateData<ParameterWellKnownAttributeData>().InterpolatedStringHandlerArguments = default(ImmutableArray<int>);
		}
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		ParameterWellKnownAttributeData parameterWellKnownAttributeData = (ParameterWellKnownAttributeData)decodedData;
		if (parameterWellKnownAttributeData != null)
		{
			switch (RefKind)
			{
			case RefKind.Ref:
				if (parameterWellKnownAttributeData.HasOutAttribute && !parameterWellKnownAttributeData.HasInAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_OutAttrOnRefParam, GetFirstLocation());
				}
				break;
			case RefKind.Out:
				if (parameterWellKnownAttributeData.HasInAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_InAttrOnOutParam, GetFirstLocation());
				}
				break;
			case RefKind.In:
				if (parameterWellKnownAttributeData.HasOutAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_OutAttrOnInParam, GetFirstLocation());
				}
				break;
			case RefKind.RefReadOnlyParameter:
				if (parameterWellKnownAttributeData.HasOutAttribute)
				{
					diagnostics.Add(ErrorCode.ERR_OutAttrOnRefReadonlyParam, GetFirstLocation());
				}
				break;
			}
		}
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
	}

	internal override void ForceComplete(SourceLocation locationOpt, Predicate<Symbol> filter, CancellationToken cancellationToken)
	{
		GetAttributes();
		_ = ExplicitDefaultConstantValue;
		DoMiscValidation();
		state.SpinWaitComplete(CompletionPart.ComplexParameterSymbolAll, cancellationToken);
	}

	private void DoMiscValidation()
	{
		if (state.NotePartComplete(CompletionPart.StartMemberChecks))
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			if (base.IsParams)
			{
				ParameterSyntax parameterSyntax = ParameterSyntax;
				if (parameterSyntax != null && parameterSyntax.Modifiers.Any(SyntaxKind.ParamsKeyword))
				{
					validateParamsType(instance);
				}
			}
			if (DeclaredScope == ScopedKind.ScopedValue && !base.Type.IsErrorOrRefLikeOrAllowsRefLikeType())
			{
				instance.Add(ErrorCode.ERR_ScopedRefAndRefStructOnly, ParameterSyntax);
			}
			AddDeclarationDiagnostics(instance);
			instance.Free();
			state.NotePartComplete(CompletionPart.FinishMemberChecks);
		}
		state.SpinWaitComplete(CompletionPart.FinishMemberChecks, default(CancellationToken));
		void checkIsAtLeastAsVisible(ParameterSyntax syntax, Binder binder, MethodSymbol method, BindingDiagnosticBag diagnostics)
		{
			if (!isAtLeastAsVisible(syntax, binder, method, diagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_ParamsMemberCannotBeLessVisibleThanDeclaringMember, syntax, method, ContainingSymbol);
			}
		}
		bool isAtLeastAsVisible(ParameterSyntax syntax, Binder binder, MethodSymbol method, BindingDiagnosticBag diagnostics)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = binder.GetNewCompoundUseSiteInfo(diagnostics);
			bool result = method.IsAsRestrictive(ContainingSymbol, ref useSiteInfo) && method.ContainingType.IsAtLeastAsVisibleAs(ContainingSymbol, ref useSiteInfo);
			diagnostics.Add(syntax.Location, useSiteInfo);
			return result;
		}
		void reportInvalidParams(BindingDiagnosticBag diagnostics)
		{
			diagnostics.Add(ErrorCode.ERR_ParamsMustBeCollection, ParameterSyntax.Modifiers.First((SyntaxToken m) => m.IsKind(SyntaxKind.ParamsKeyword)).GetLocation());
		}
		void validateParamsType(BindingDiagnosticBag diagnostics)
		{
			CollectionExpressionTypeKind collectionExpressionTypeKind = ConversionsBase.GetCollectionExpressionTypeKind(DeclaringCompilation, base.Type, out var elementType);
			_ = elementType.Type;
			switch (collectionExpressionTypeKind)
			{
			case CollectionExpressionTypeKind.None:
				reportInvalidParams(diagnostics);
				return;
			case CollectionExpressionTypeKind.ImplementsIEnumerable:
			{
				ParameterSyntax parameterSyntax3 = ParameterSyntax;
				Binder binder2 = GetDefaultParameterValueBinder(parameterSyntax3).WithContainingMemberOrLambda(ContainingSymbol);
				binder2.TryGetCollectionIterationType(parameterSyntax3, base.Type, out elementType);
				if ((object)elementType.Type == null)
				{
					reportInvalidParams(diagnostics);
					return;
				}
				if (!binder2.HasCollectionExpressionApplicableConstructor(parameterSyntax3, base.Type, out MethodSymbol constructor, out bool _, diagnostics, isParamsModifierValidation: true))
				{
					return;
				}
				if ((object)constructor != null)
				{
					checkIsAtLeastAsVisible(parameterSyntax3, binder2, constructor, diagnostics);
				}
				if (!binder2.HasCollectionExpressionApplicableAddMethod(parameterSyntax3, base.Type, out ImmutableArray<MethodSymbol> addMethods, diagnostics))
				{
					return;
				}
				if (addMethods[0].IsExtensionMethod || addMethods[0].IsExtensionBlockMember())
				{
					diagnostics.Add(ErrorCode.ERR_ParamsCollectionExtensionAddMethod, parameterSyntax3, base.Type);
					return;
				}
				MethodSymbol methodSymbol = null;
				foreach (MethodSymbol item in addMethods)
				{
					if (isAtLeastAsVisible(parameterSyntax3, binder2, item, diagnostics))
					{
						methodSymbol = null;
						break;
					}
					if ((object)methodSymbol == null)
					{
						methodSymbol = item;
					}
				}
				if ((object)methodSymbol != null)
				{
					diagnostics.Add(ErrorCode.ERR_ParamsMemberCannotBeLessVisibleThanDeclaringMember, parameterSyntax3, methodSymbol, ContainingSymbol);
				}
				break;
			}
			case CollectionExpressionTypeKind.CollectionBuilder:
			{
				ParameterSyntax parameterSyntax2 = ParameterSyntax;
				Binder binder = GetDefaultParameterValueBinder(parameterSyntax2).WithContainingMemberOrLambda(ContainingSymbol);
				binder.TryGetCollectionIterationType(parameterSyntax2, base.Type, out elementType);
				if ((object)elementType.Type == null)
				{
					reportInvalidParams(diagnostics);
					return;
				}
				MethodSymbol andValidateCollectionBuilderMethod = binder.GetAndValidateCollectionBuilderMethod(parameterSyntax2, (NamedTypeSymbol)base.Type, diagnostics, out TypeSymbol _);
				if ((object)andValidateCollectionBuilderMethod == null)
				{
					return;
				}
				if (ContainingSymbol.ContainingSymbol is NamedTypeSymbol)
				{
					checkIsAtLeastAsVisible(parameterSyntax2, binder, andValidateCollectionBuilderMethod, diagnostics);
				}
				break;
			}
			}
			if (collectionExpressionTypeKind != CollectionExpressionTypeKind.Array)
			{
				MessageID.IDS_FeatureParamsCollections.CheckFeatureAvailability(diagnostics, ParameterSyntax);
			}
		}
	}
}
