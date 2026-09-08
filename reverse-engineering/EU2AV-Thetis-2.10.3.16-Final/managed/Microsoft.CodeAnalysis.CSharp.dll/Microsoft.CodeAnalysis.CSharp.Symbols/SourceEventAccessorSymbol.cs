using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceEventAccessorSymbol : SourceMemberMethodSymbol
{
	private readonly SourceEventSymbol _event;

	private readonly string _name;

	private readonly ImmutableArray<MethodSymbol> _explicitInterfaceImplementations;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private TypeWithAnnotations _lazyReturnType;

	public override string Name => _name;

	internal override bool IsExplicitInterfaceImplementation => _event.IsExplicitInterfaceImplementation;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public sealed override bool AreLocalsZeroed
	{
		get
		{
			if (!_event.HasSkipLocalsInitAttribute)
			{
				return base.AreLocalsZeroed;
			}
			return false;
		}
	}

	public SourceEventSymbol AssociatedEvent => _event;

	public sealed override Symbol AssociatedSymbol => _event;

	public sealed override bool ReturnsVoid
	{
		get
		{
			LazyMethodChecks();
			return base.ReturnsVoid;
		}
	}

	public sealed override TypeWithAnnotations ReturnTypeWithAnnotations
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public sealed override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	internal Location Location => GetFirstLocation();

	protected abstract override SourceMemberMethodSymbol? BoundAttributesSource { get; }

	public sealed override MethodSymbol? PartialImplementationPart
	{
		get
		{
			SourceEventSymbol sourceEventSymbol = _event;
			if ((object)sourceEventSymbol != null && sourceEventSymbol.IsPartialDefinition)
			{
				SourceEventSymbol otherPartOfPartial = sourceEventSymbol.OtherPartOfPartial;
				if ((object)otherPartOfPartial != null)
				{
					if (MethodKind != MethodKind.EventAdd)
					{
						return otherPartOfPartial.RemoveMethod;
					}
					return otherPartOfPartial.AddMethod;
				}
			}
			return null;
		}
	}

	public sealed override MethodSymbol? PartialDefinitionPart
	{
		get
		{
			SourceEventSymbol sourceEventSymbol = _event;
			if ((object)sourceEventSymbol != null && sourceEventSymbol.IsPartialImplementation)
			{
				SourceEventSymbol otherPartOfPartial = sourceEventSymbol.OtherPartOfPartial;
				if ((object)otherPartOfPartial != null)
				{
					if (MethodKind != MethodKind.EventAdd)
					{
						return otherPartOfPartial.RemoveMethod;
					}
					return otherPartOfPartial.AddMethod;
				}
			}
			return null;
		}
	}

	internal bool IsPartialDefinition => _event.IsPartialDefinition;

	internal bool IsPartialImplementation => _event.IsPartialImplementation;

	public sealed override bool IsExtern => PartialImplementationPart?.IsExtern ?? base.IsExtern;

	public SourceEventAccessorSymbol(SourceEventSymbol @event, SyntaxReference syntaxReference, Location location, EventSymbol explicitlyImplementedEventOpt, string aliasQualifierOpt, bool isAdder, bool isIterator, bool isNullableAnalysisEnabled, bool isExpressionBodied)
		: base(@event.containingType, syntaxReference, location, isIterator, (declarationModifiers: @event.Modifiers, flags: SourceMemberMethodSymbol.MakeFlags(isAdder ? MethodKind.EventAdd : MethodKind.EventRemove, RefKind.None, @event.Modifiers, returnsVoid: false, returnsVoidIsSet: false, isExpressionBodied, isExtensionMethod: false, isNullableAnalysisEnabled, isVarArg: false, @event.IsExplicitInterfaceImplementation, hasThisInitializer: false)))
	{
		_event = @event;
		_parameters = ImmutableArray.Create((ParameterSymbol)new SynthesizedEventAccessorValueParameterSymbol(this, 0));
		string text;
		ImmutableArray<MethodSymbol> explicitInterfaceImplementations;
		if ((object)explicitlyImplementedEventOpt == null)
		{
			text = SourceEventSymbol.GetAccessorName(@event.Name, isAdder);
			explicitInterfaceImplementations = ImmutableArray<MethodSymbol>.Empty;
		}
		else
		{
			MethodSymbol methodSymbol = (isAdder ? explicitlyImplementedEventOpt.AddMethod : explicitlyImplementedEventOpt.RemoveMethod);
			text = ExplicitInterfaceHelpers.GetMemberName(((object)methodSymbol != null) ? methodSymbol.Name : SourceEventSymbol.GetAccessorName(explicitlyImplementedEventOpt.Name, isAdder), explicitlyImplementedEventOpt.ContainingType, aliasQualifierOpt);
			explicitInterfaceImplementations = (((object)methodSymbol == null) ? ImmutableArray<MethodSymbol>.Empty : ImmutableArray.Create(methodSymbol));
		}
		_explicitInterfaceImplementations = explicitInterfaceImplementations;
		_name = GetOverriddenAccessorName(@event, isAdder) ?? text;
	}

	protected sealed override void MethodChecks(BindingDiagnosticBag diagnostics)
	{
		if (!_lazyReturnType.IsDefault)
		{
			return;
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (_event.IsWindowsRuntimeEvent)
		{
			TypeSymbol wellKnownType = declaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationToken);
			Binder.ReportUseSite(wellKnownType, diagnostics, Location);
			if (MethodKind == MethodKind.EventAdd)
			{
				_lazyReturnType = TypeWithAnnotations.Create(wellKnownType);
				SetReturnsVoid(returnsVoid: false);
				return;
			}
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Void);
			Binder.ReportUseSite(specialType, diagnostics, Location);
			_lazyReturnType = TypeWithAnnotations.Create(specialType);
			SetReturnsVoid(returnsVoid: true);
		}
		else
		{
			TypeSymbol specialType2 = declaringCompilation.GetSpecialType(SpecialType.System_Void);
			Binder.ReportUseSite(specialType2, diagnostics, Location);
			_lazyReturnType = TypeWithAnnotations.Create(specialType2);
			SetReturnsVoid(returnsVoid: true);
		}
	}

	public sealed override ImmutableArray<ImmutableArray<TypeWithAnnotations>> GetTypeParameterConstraintTypes()
	{
		return ImmutableArray<ImmutableArray<TypeWithAnnotations>>.Empty;
	}

	public sealed override ImmutableArray<TypeParameterConstraintKind> GetTypeParameterConstraintKinds()
	{
		return ImmutableArray<TypeParameterConstraintKind>.Empty;
	}

	protected string GetOverriddenAccessorName(SourceEventSymbol @event, bool isAdder)
	{
		if (IsOverride)
		{
			EventSymbol overriddenEvent = @event.OverriddenEvent;
			if ((object)overriddenEvent != null)
			{
				return overriddenEvent.GetOwnOrInheritedAccessor(isAdder)?.Name;
			}
		}
		return null;
	}

	internal sealed override int TryGetOverloadResolutionPriority()
	{
		return 0;
	}
}
