using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit.NoPia;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedEvent : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedEvent
{
	protected override bool IsRuntimeSpecial => base.UnderlyingEvent.AdaptedEventSymbol.HasRuntimeSpecialName;

	protected override bool IsSpecialName => base.UnderlyingEvent.AdaptedEventSymbol.HasSpecialName;

	protected override EmbeddedType ContainingType => base.AnAccessor.ContainingType;

	protected override TypeMemberVisibility Visibility => base.UnderlyingEvent.AdaptedEventSymbol.MetadataVisibility;

	protected override string Name => base.UnderlyingEvent.AdaptedEventSymbol.MetadataName;

	public EmbeddedEvent(EventSymbol underlyingEvent, EmbeddedMethod adder, EmbeddedMethod remover)
		: base(underlyingEvent, adder, remover, (EmbeddedMethod)null)
	{
	}

	protected override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return base.UnderlyingEvent.AdaptedEventSymbol.GetCustomAttributesToEmit(moduleBuilder);
	}

	protected override ITypeReference GetType(PEModuleBuilder moduleBuilder, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return moduleBuilder.Translate(base.UnderlyingEvent.AdaptedEventSymbol.Type, syntaxNodeOpt, diagnostics);
	}

	protected override void EmbedCorrespondingComEventInterfaceMethodInternal(SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding)
	{
		NamedTypeSymbol adaptedNamedTypeSymbol = ContainingType.UnderlyingNamedType.AdaptedNamedTypeSymbol;
		foreach (CSharpAttributeData attribute in adaptedNamedTypeSymbol.GetAttributes())
		{
			if (attribute.GetTargetAttributeSignatureIndex(AttributeDescription.ComEventInterfaceAttribute) != 0)
			{
				continue;
			}
			bool flag = false;
			NamedTypeSymbol namedTypeSymbol = null;
			DiagnosticInfo errorInfo = attribute.ErrorInfo;
			if (errorInfo != null)
			{
				diagnostics.Add(errorInfo, syntaxNodeOpt?.Location ?? NoLocation.Singleton);
			}
			if (!attribute.HasErrors)
			{
				namedTypeSymbol = attribute.CommonConstructorArguments[0].ValueInternal as NamedTypeSymbol;
				if ((object)namedTypeSymbol != null)
				{
					flag = EmbedMatchingInterfaceMethods(namedTypeSymbol, syntaxNodeOpt, diagnostics);
					foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in namedTypeSymbol.AllInterfacesNoUseSiteDiagnostics)
					{
						if (EmbedMatchingInterfaceMethods(allInterfacesNoUseSiteDiagnostic, syntaxNodeOpt, diagnostics))
						{
							flag = true;
						}
					}
				}
			}
			if (!flag & isUsedForComAwareEventBinding)
			{
				if ((object)namedTypeSymbol == null)
				{
					EmbeddedTypesManager.Error(diagnostics, ErrorCode.ERR_MissingSourceInterface, syntaxNodeOpt, adaptedNamedTypeSymbol, base.UnderlyingEvent.AdaptedEventSymbol);
					break;
				}
				CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.DiscardedDependencies;
				namedTypeSymbol.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
				diagnostics.Add((syntaxNodeOpt == null) ? NoLocation.Singleton : syntaxNodeOpt.Location, useSiteInfo.Diagnostics);
				EmbeddedTypesManager.Error(diagnostics, ErrorCode.ERR_MissingMethodOnSourceInterface, syntaxNodeOpt, namedTypeSymbol, base.UnderlyingEvent.AdaptedEventSymbol.MetadataName, base.UnderlyingEvent.AdaptedEventSymbol);
			}
			break;
		}
	}

	private bool EmbedMatchingInterfaceMethods(NamedTypeSymbol sourceInterface, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		bool result = false;
		foreach (Symbol member in sourceInterface.GetMembers(base.UnderlyingEvent.AdaptedEventSymbol.MetadataName))
		{
			if (member.Kind == SymbolKind.Method)
			{
				TypeManager.EmbedMethodIfNeedTo(((MethodSymbol)member).GetCciAdapter(), syntaxNodeOpt, diagnostics);
				result = true;
			}
		}
		return result;
	}
}
