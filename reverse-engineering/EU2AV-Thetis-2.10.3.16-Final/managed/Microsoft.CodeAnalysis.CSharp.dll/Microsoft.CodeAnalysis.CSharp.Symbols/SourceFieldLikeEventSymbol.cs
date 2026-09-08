using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceFieldLikeEventSymbol : SourceEventSymbol
{
	internal sealed class SourceEventDefinitionAccessorSymbol : SourceEventAccessorSymbol
	{
		public override Accessibility DeclaredAccessibility => base.AssociatedEvent.DeclaredAccessibility;

		public override bool IsImplicitlyDeclared => true;

		internal override bool GenerateDebugInfo => true;

		protected override SourceMemberMethodSymbol? BoundAttributesSource
		{
			get
			{
				if (!IsExtern || MethodKind != MethodKind.EventAdd)
				{
					return null;
				}
				return (SourceMemberMethodSymbol)base.AssociatedEvent.RemoveMethod;
			}
		}

		protected override IAttributeTargetSymbol AttributeOwner
		{
			get
			{
				MethodSymbol partialImplementationPart = PartialImplementationPart;
				if (!(partialImplementationPart is SourceCustomEventAccessorSymbol))
				{
					if (!(partialImplementationPart is SynthesizedEventAccessorSymbol))
					{
						if ((object)partialImplementationPart == null)
						{
							return this;
						}
						return this;
					}
					return base.AssociatedEvent;
				}
				return this;
			}
		}

		internal override MethodImplAttributes ImplementationAttributes => PartialImplementationPart?.ImplementationAttributes ?? base.ImplementationAttributes;

		internal SourceEventDefinitionAccessorSymbol(SourceFieldLikeEventSymbol ev, bool isAdder, BindingDiagnosticBag diagnostics)
			: base(ev, ev.SyntaxReference, ev.Location, null, null, isAdder, isIterator: false, ev.DeclaringCompilation.IsNullableAnalysisEnabledIn(ev.CSharpSyntaxNode), isExpressionBodied: false)
		{
			CheckFeatureAvailabilityAndRuntimeSupport(ev.CSharpSyntaxNode, ev.Location, hasBody: false, diagnostics);
		}

		internal override ExecutableCodeBinder? TryGetBodyBinder(BinderFactory? binderFactoryOpt = null, bool ignoreAccessibility = false)
		{
			return null;
		}

		internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
		{
			MethodSymbol partialImplementationPart = PartialImplementationPart;
			if (!(partialImplementationPart is SourceCustomEventAccessorSymbol sourceCustomEventAccessorSymbol))
			{
				if (!(partialImplementationPart is SynthesizedEventAccessorSymbol synthesizedEventAccessorSymbol))
				{
					if ((object)partialImplementationPart == null)
					{
						return OneOrMany<SyntaxList<AttributeListSyntax>>.Empty;
					}
					return OneOrMany<SyntaxList<AttributeListSyntax>>.Empty;
				}
				return OneOrMany.Create(base.AssociatedEvent.AttributeDeclarationSyntaxList, synthesizedEventAccessorSymbol.AssociatedEvent.AttributeDeclarationSyntaxList);
			}
			return OneOrMany.Create(sourceCustomEventAccessorSymbol.AttributeDeclarationSyntaxList);
		}
	}

	private readonly string _name;

	private readonly TypeWithAnnotations _type;

	private readonly SourceEventAccessorSymbol _addMethod;

	private readonly SourceEventAccessorSymbol _removeMethod;

	protected override bool AccessorsHaveImplementation => false;

	internal override FieldSymbol? AssociatedField => AssociatedEventField;

	internal SourceEventFieldSymbol? AssociatedEventField { get; }

	public override string Name => _name;

	public override TypeWithAnnotations TypeWithAnnotations => _type;

	public override MethodSymbol AddMethod => _addMethod;

	public override MethodSymbol RemoveMethod => _removeMethod;

	internal override bool IsExplicitInterfaceImplementation => false;

	protected override AttributeLocation AllowedAttributeLocations
	{
		get
		{
			AttributeLocation attributeLocation = AttributeLocation.Event;
			if (!base.IsPartial || IsExtern)
			{
				attributeLocation |= AttributeLocation.Method;
			}
			if ((object)AssociatedEventField != null)
			{
				attributeLocation |= AttributeLocation.Field;
			}
			return attributeLocation;
		}
	}

	public override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations => ImmutableArray<EventSymbol>.Empty;

	internal SourceFieldLikeEventSymbol(SourceMemberContainerTypeSymbol containingType, Binder binder, SyntaxTokenList modifiers, VariableDeclaratorSyntax declaratorSyntax, BindingDiagnosticBag diagnostics)
		: base(containingType, declaratorSyntax, modifiers, isFieldLike: true, null, declaratorSyntax.Identifier, diagnostics)
	{
		_name = declaratorSyntax.Identifier.ValueText;
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)declaratorSyntax.Parent;
		_type = BindEventType(binder, variableDeclarationSyntax.Type, instance);
		if (IsOverride)
		{
			EventSymbol overriddenEvent = base.OverriddenEvent;
			if ((object)overriddenEvent != null)
			{
				SourceEventSymbol.CopyEventCustomModifiers(overriddenEvent, ref _type, ContainingAssembly);
			}
		}
		bool flag = declaratorSyntax.Initializer != null;
		bool flag2 = containingType.IsInterfaceType();
		if (flag)
		{
			if (flag2 && !IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_InterfaceEventInitializer, GetFirstLocation(), this);
			}
			else if (IsAbstract)
			{
				diagnostics.Add(ErrorCode.ERR_AbstractEventInitializer, GetFirstLocation(), this);
			}
			else if (IsExtern)
			{
				diagnostics.Add(ErrorCode.ERR_ExternEventInitializer, GetFirstLocation(), this);
			}
			else if (base.IsPartial)
			{
				diagnostics.Add(ErrorCode.ERR_PartialEventInitializer, GetFirstLocation(), this);
			}
		}
		if (flag || (!IsExtern && !IsAbstract && !base.IsPartial))
		{
			AssociatedEventField = MakeAssociatedField(declaratorSyntax);
		}
		if (!IsStatic && ContainingType.IsReadOnly)
		{
			diagnostics.Add(ErrorCode.ERR_FieldlikeEventsInRoStruct, GetFirstLocation());
		}
		if (flag2)
		{
			if ((IsAbstract || IsVirtual) && IsStatic)
			{
				if (!ContainingAssembly.RuntimeSupportsStaticAbstractMembersInInterfaces)
				{
					diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportStaticAbstractMembersInInterfaces, GetFirstLocation());
				}
			}
			else if (IsExtern || IsStatic)
			{
				if (!ContainingAssembly.RuntimeSupportsDefaultInterfaceImplementation)
				{
					diagnostics.Add(ErrorCode.ERR_RuntimeDoesNotSupportDefaultInterfaceImplementation, GetFirstLocation());
				}
			}
			else if (!IsAbstract && !IsPartialDefinition)
			{
				diagnostics.Add(ErrorCode.ERR_EventNeedsBothAccessors, GetFirstLocation(), this);
			}
		}
		if (IsPartialDefinition)
		{
			_addMethod = new SourceEventDefinitionAccessorSymbol(this, isAdder: true, diagnostics);
			_removeMethod = new SourceEventDefinitionAccessorSymbol(this, isAdder: false, diagnostics);
		}
		else
		{
			_addMethod = new SynthesizedEventAccessorSymbol(this, isAdder: true, isExpressionBodied: false);
			_removeMethod = new SynthesizedEventAccessorSymbol(this, isAdder: false, isExpressionBodied: false);
		}
		if (variableDeclarationSyntax.Variables[0] == declaratorSyntax)
		{
			diagnostics.AddRange(instance);
		}
		instance.Free();
	}

	private SourceEventFieldSymbol MakeAssociatedField(VariableDeclaratorSyntax declaratorSyntax)
	{
		return new SourceEventFieldSymbol(this, declaratorSyntax, BindingDiagnosticBag.Discarded);
	}

	internal override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		if ((object)AssociatedField != null)
		{
			AssociatedField.ForceComplete(locationOpt, filter, cancellationToken);
		}
		base.ForceComplete(locationOpt, filter, cancellationToken);
	}
}
