using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEventAccessorSymbol : SourceEventAccessorSymbol
{
	private readonly object _methodChecksLockObject = new object();

	public override bool IsImplicitlyDeclared => true;

	internal override bool GenerateDebugInfo => false;

	protected override SourceMemberMethodSymbol BoundAttributesSource
	{
		get
		{
			MethodSymbol partialDefinitionPart = PartialDefinitionPart;
			if ((object)partialDefinitionPart != null)
			{
				return (SourceMemberMethodSymbol)partialDefinitionPart;
			}
			if (MethodKind != MethodKind.EventAdd)
			{
				return null;
			}
			return (SourceMemberMethodSymbol)base.AssociatedEvent.RemoveMethod;
		}
	}

	protected override IAttributeTargetSymbol AttributeOwner => base.AssociatedEvent;

	protected override object MethodChecksLockObject => _methodChecksLockObject;

	internal override MethodImplAttributes ImplementationAttributes
	{
		get
		{
			MethodImplAttributes methodImplAttributes = base.ImplementationAttributes;
			if (!IsAbstract && !base.AssociatedEvent.IsWindowsRuntimeEvent && !ContainingType.IsStructType() && (object)DeclaringCompilation.GetWellKnownTypeMember(WellKnownMember.System_Threading_Interlocked__CompareExchange_T) == null)
			{
				methodImplAttributes |= MethodImplAttributes.Synchronized;
			}
			return methodImplAttributes;
		}
	}

	internal override bool SynthesizesLoweredBoundBody
	{
		get
		{
			if (IsFieldLikeEventAccessor())
			{
				return true;
			}
			return base.SynthesizesLoweredBoundBody;
		}
	}

	internal SynthesizedEventAccessorSymbol(SourceEventSymbol @event, bool isAdder, bool isExpressionBodied, EventSymbol explicitlyImplementedEventOpt = null, string aliasQualifierOpt = null)
		: base(@event, null, @event.Location, explicitlyImplementedEventOpt, aliasQualifierOpt, isAdder, isIterator: false, isNullableAnalysisEnabled: false, isExpressionBodied)
	{
	}

	private bool IsFieldLikeEventAccessor()
	{
		return base.AssociatedEvent.HasAssociatedField;
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		MethodSymbol partialDefinitionPart = PartialDefinitionPart;
		if ((object)partialDefinitionPart != null)
		{
			return OneOrMany.Create(base.AssociatedEvent.AttributeDeclarationSyntaxList, ((SourceEventAccessorSymbol)partialDefinitionPart).AssociatedEvent.AttributeDeclarationSyntaxList);
		}
		MethodSymbol partialImplementationPart = PartialImplementationPart;
		if ((object)partialImplementationPart != null)
		{
			return OneOrMany.Create(base.AssociatedEvent.AttributeDeclarationSyntaxList, ((SourceEventAccessorSymbol)partialImplementationPart).AssociatedEvent.AttributeDeclarationSyntaxList);
		}
		return OneOrMany.Create(base.AssociatedEvent.AttributeDeclarationSyntaxList);
	}

	internal override ExecutableCodeBinder TryGetBodyBinder(BinderFactory binderFactoryOpt = null, bool ignoreAccessibility = false)
	{
		return TryGetBodyBinderFromSyntax(binderFactoryOpt, ignoreAccessibility);
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, BindingDiagnosticBag diagnostics)
	{
		if (IsFieldLikeEventAccessor())
		{
			SourceEventSymbol associatedEvent = base.AssociatedEvent;
			if (associatedEvent.Type.IsDelegateType())
			{
				BoundBlock boundBlock = Microsoft.CodeAnalysis.CSharp.MethodBodySynthesizer.ConstructFieldLikeEventAccessorBody(associatedEvent, MethodKind == MethodKind.EventAdd, compilationState.Compilation, diagnostics);
				if (boundBlock != null)
				{
					compilationState.AddSynthesizedMethod(this, boundBlock);
				}
			}
		}
		else
		{
			base.GenerateMethodBody(compilationState, diagnostics);
		}
	}
}
