using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedParameterSymbolBase : ParameterSymbol
{
	private readonly Symbol? _container;

	private readonly TypeWithAnnotations _type;

	private readonly int _ordinal;

	private readonly string _name;

	private readonly RefKind _refKind;

	private readonly ScopedKind _scope;

	public override TypeWithAnnotations TypeWithAnnotations => _type;

	public override RefKind RefKind => _refKind;

	public sealed override bool IsDiscard => false;

	public override string Name => _name;

	public abstract override ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	public override int Ordinal => _ordinal;

	public override bool IsParamsArray => false;

	public override bool IsParamsCollection => false;

	internal override bool IsMetadataOptional => ExplicitDefaultConstantValue != null;

	public override bool IsImplicitlyDeclared => true;

	internal override ConstantValue? ExplicitDefaultConstantValue => null;

	internal override bool IsIDispatchConstant
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedParameterSymbol.cs", 90);
		}
	}

	internal override bool IsIUnknownConstant
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedParameterSymbol.cs", 95);
		}
	}

	internal override bool IsCallerLineNumber
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedParameterSymbol.cs", 100);
		}
	}

	internal override bool IsCallerFilePath
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedParameterSymbol.cs", 105);
		}
	}

	internal override bool IsCallerMemberName
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedParameterSymbol.cs", 110);
		}
	}

	internal override int CallerArgumentExpressionParameterIndex => -1;

	internal override FlowAnalysisAnnotations FlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

	internal override ImmutableHashSet<string> NotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

	public override Symbol? ContainingSymbol => _container;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes => ImmutableArray<int>.Empty;

	internal override bool HasInterpolatedStringHandlerArgumentError => false;

	internal sealed override ScopedKind DeclaredScope
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Synthesized/SynthesizedParameterSymbol.cs", 225);
		}
	}

	internal sealed override ScopedKind EffectiveScope => _scope;

	internal sealed override bool UseUpdatedEscapeRules
	{
		get
		{
			Symbol container = _container;
			if (!(container is MethodSymbol { UseUpdatedEscapeRules: var useUpdatedEscapeRules }))
			{
				return container?.ContainingModule.UseUpdatedEscapeRules ?? false;
			}
			return useUpdatedEscapeRules;
		}
	}

	public SynthesizedParameterSymbolBase(Symbol? container, TypeWithAnnotations type, int ordinal, RefKind refKind, ScopedKind scope, string name)
	{
		_container = container;
		_type = type;
		_ordinal = ordinal;
		_refKind = refKind;
		_scope = scope;
		_name = name;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		TypeWithAnnotations typeWithAnnotations = TypeWithAnnotations;
		if (typeWithAnnotations.Type.ContainsDynamic() && declaringCompilation.HasDynamicEmitAttributes(BindingDiagnosticBag.Discarded, Location.None) && declaringCompilation.CanEmitBoolean())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(typeWithAnnotations.Type, typeWithAnnotations.CustomModifiers.Length + RefCustomModifiers.Length, RefKind));
		}
		if (declaringCompilation.ShouldEmitNativeIntegerAttributes(typeWithAnnotations.Type))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNativeIntegerAttribute(this, typeWithAnnotations.Type));
		}
		if (ParameterHelpers.RequiresScopedRefAttribute(this))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeScopedRefAttribute(this, EffectiveScope));
		}
		if (typeWithAnnotations.Type.ContainsTupleNames() && declaringCompilation.HasTupleNamesAttributes(BindingDiagnosticBag.Discarded, Location.None) && declaringCompilation.CanEmitSpecialType(SpecialType.System_String))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(typeWithAnnotations.Type));
		}
		if (declaringCompilation.ShouldEmitNullableAttributes(this))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeNullableAttributeIfNecessary(this, GetNullableContextValue(), typeWithAnnotations));
		}
		switch (RefKind)
		{
		case RefKind.In:
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
			break;
		case RefKind.RefReadOnlyParameter:
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeRequiresLocationAttribute(this));
			break;
		}
		if (HasUnscopedRefAttribute && ContainingSymbol is SynthesizedDelegateInvokeMethod)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_CodeAnalysis_UnscopedRefAttribute__ctor));
		}
		if (IsParamsArray)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_ParamArrayAttribute__ctor));
		}
		else if (IsParamsCollection)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeParamCollectionAttribute(this));
		}
		ConstantValue explicitDefaultConstantValue = ExplicitDefaultConstantValue;
		bool flag = explicitDefaultConstantValue != null && DefaultValueFromAttributes == null;
		if (flag)
		{
			Symbol containingSymbol = ContainingSymbol;
			bool flag2 = ((containingSymbol is SynthesizedDelegateInvokeMethod || containingSymbol is SynthesizedClosureMethod) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, explicitDefaultConstantValue.SpecialType switch
			{
				SpecialType.System_Decimal => declaringCompilation.SynthesizeDecimalConstantAttribute(explicitDefaultConstantValue.DecimalValue), 
				SpecialType.System_DateTime => declaringCompilation.SynthesizeDateTimeConstantAttribute(explicitDefaultConstantValue.DateTimeValue), 
				_ => null, 
			});
		}
	}
}
