using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedBackingFieldSymbol : SynthesizedBackingFieldSymbolBase
{
	private readonly SourcePropertySymbolBase _property;

	private int _inferredNullableAnnotation = 3;

	internal override bool HasInitializer { get; }

	protected override IAttributeTargetSymbol AttributeOwner => _property.AttributesOwner;

	internal override Location ErrorLocation => _property.Location;

	public override Symbol AssociatedSymbol => _property;

	public override ImmutableArray<Location> Locations => _property.Locations;

	public override RefKind RefKind => _property.RefKind;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _property.RefCustomModifiers;

	internal bool InfersNullableAnnotation
	{
		get
		{
			if (FlowAnalysisAnnotations != FlowAnalysisAnnotations.None)
			{
				return false;
			}
			if (_property.TypeWithAnnotations.NullableAnnotation != NullableAnnotation.NotAnnotated || !_property.UsesFieldKeyword)
			{
				return false;
			}
			return true;
		}
	}

	internal override bool HasPointerType => _property.HasPointerType;

	public override Symbol ContainingSymbol => _property.ContainingSymbol;

	public override NamedTypeSymbol ContainingType => _property.ContainingType;

	public SynthesizedBackingFieldSymbol(SourcePropertySymbolBase property, string name, bool isReadOnly, bool isStatic, bool hasInitializer)
		: base(name, isReadOnly, isStatic)
	{
		_property = property;
		HasInitializer = hasInitializer;
	}

	protected override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return ((_property as SourcePropertySymbol)?.SourcePartialDefinitionPart ?? _property).GetAttributeDeclarations();
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _property.TypeWithAnnotations;
	}

	internal NullableAnnotation GetInferredNullableAnnotation()
	{
		if (_inferredNullableAnnotation == 3)
		{
			NullableAnnotation value = ComputeInferredNullableAnnotation();
			Interlocked.CompareExchange(ref _inferredNullableAnnotation, (int)value, 3);
		}
		return (NullableAnnotation)_inferredNullableAnnotation;
	}

	private NullableAnnotation ComputeInferredNullableAnnotation()
	{
		MethodSymbol getMethod = _property.GetMethod;
		SourcePropertyAccessorSymbol getAccessor = getMethod as SourcePropertyAccessorSymbol;
		if ((object)getAccessor == null)
		{
			return NullableAnnotation.Annotated;
		}
		getAccessor = ((SourcePropertyAccessorSymbol)getAccessor.PartialImplementationPart) ?? getAccessor;
		if (getAccessor.IsAutoPropertyAccessor)
		{
			return NullableAnnotation.NotAnnotated;
		}
		ExecutableCodeBinder binder = getAccessor.TryGetBodyBinder() ?? throw ExceptionUtilities.UnexpectedValue(getAccessor);
		BoundNode boundGetAccessor = binder.BindMethodBody(getAccessor.SyntaxNode, BindingDiagnosticBag.Discarded);
		DiagnosticBag diagnosticBag = nullableAnalyzeAndFilterDiagnostics(NullableAnnotation.Annotated);
		if (diagnosticBag.IsEmptyWithoutResolution)
		{
			diagnosticBag.Free();
			return NullableAnnotation.Annotated;
		}
		DiagnosticBag diagnosticBag2 = nullableAnalyzeAndFilterDiagnostics(NullableAnnotation.NotAnnotated);
		if (diagnosticBag2.IsEmptyWithoutResolution)
		{
			diagnosticBag.Free();
			diagnosticBag2.Free();
			return NullableAnnotation.NotAnnotated;
		}
		HashSet<Diagnostic> hashSet = new HashSet<Diagnostic>(diagnosticBag2.AsEnumerable(), SameDiagnosticComparer.Instance);
		diagnosticBag2.Free();
		foreach (Diagnostic item in diagnosticBag.AsEnumerable())
		{
			if (!hashSet.Contains(item))
			{
				diagnosticBag.Free();
				return NullableAnnotation.NotAnnotated;
			}
		}
		diagnosticBag.Free();
		return NullableAnnotation.Annotated;
		DiagnosticBag nullableAnalyzeAndFilterDiagnostics(NullableAnnotation assumedNullableAnnotation)
		{
			DiagnosticBag incoming = DiagnosticBag.GetInstance();
			NullableWalker.AnalyzeIfNeeded(binder, boundGetAccessor, boundGetAccessor.Syntax, incoming, (getAccessor, new NullableWalker.GetterNullResilienceData(_property.BackingField, assumedNullableAnnotation)));
			if (incoming.IsEmptyWithoutResolution)
			{
				return incoming;
			}
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			DeclaringCompilation.FilterAndAppendAndFreeDiagnostics(instance, ref incoming, default(CancellationToken));
			return instance;
		}
	}

	protected sealed override void DecodeWellKnownAttributeImpl(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
		if (arguments.Attribute.IsTargetAttribute(AttributeDescription.FixedBufferAttribute))
		{
			((BindingDiagnosticBag)arguments.Diagnostics).Add(ErrorCode.ERR_DoNotUseFixedBufferAttrOnProperty, arguments.AttributeSyntaxOpt.Name.Location);
		}
		else
		{
			base.DecodeWellKnownAttributeImpl(ref arguments);
		}
	}

	internal override void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, BindingDiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
		base.PostDecodeWellKnownAttributes(boundAttributes, allAttributeSyntaxNodes, diagnostics, symbolPart, decodedData);
		if (!allAttributeSyntaxNodes.IsEmpty && _property.IsAutoPropertyOrUsesFieldKeyword)
		{
			CheckForFieldTargetedAttribute(diagnostics);
		}
	}

	private void CheckForFieldTargetedAttribute(BindingDiagnosticBag diagnostics)
	{
		LanguageVersion languageVersion = DeclaringCompilation.LanguageVersion;
		if (languageVersion.AllowAttributesOnBackingFields())
		{
			return;
		}
		foreach (SyntaxList<AttributeListSyntax> attributeDeclaration in GetAttributeDeclarations())
		{
			foreach (AttributeListSyntax item in attributeDeclaration)
			{
				AttributeTargetSpecifierSyntax? target = item.Target;
				if (target != null && target.GetAttributeLocation() == AttributeLocation.Field)
				{
					diagnostics.Add(new CSDiagnosticInfo(ErrorCode.WRN_AttributesOnBackingFieldsNotAvailable, languageVersion.ToDisplayString(), new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureAttributesOnBackingFields.RequiredVersion())), item.Target.Location);
				}
			}
		}
	}
}
