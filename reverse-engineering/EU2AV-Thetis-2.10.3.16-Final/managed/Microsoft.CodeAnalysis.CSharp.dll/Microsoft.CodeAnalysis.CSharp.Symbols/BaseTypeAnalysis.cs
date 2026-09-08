using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class BaseTypeAnalysis
{
	internal static bool TypeDependsOn(NamedTypeSymbol depends, NamedTypeSymbol on)
	{
		PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
		TypeDependsClosure(depends, depends.DeclaringCompilation, instance);
		bool result = instance.Contains(on);
		instance.Free();
		return result;
	}

	private static void TypeDependsClosure(NamedTypeSymbol type, CSharpCompilation currentCompilation, HashSet<Symbol> partialClosure)
	{
		if ((object)type == null)
		{
			return;
		}
		type = type.OriginalDefinition;
		if (!partialClosure.Add(type))
		{
			return;
		}
		if (type.IsInterface)
		{
			foreach (NamedTypeSymbol declaredInterface in type.GetDeclaredInterfaces(null))
			{
				TypeDependsClosure(declaredInterface, currentCompilation, partialClosure);
			}
		}
		else
		{
			TypeDependsClosure(type.GetDeclaredBaseType(null), currentCompilation, partialClosure);
		}
		if (currentCompilation != null && type.IsFromCompilation(currentCompilation))
		{
			TypeDependsClosure(type.ContainingType, currentCompilation, partialClosure);
		}
	}

	internal static bool StructDependsOn(NamedTypeSymbol depends, NamedTypeSymbol on)
	{
		PooledHashSet<NamedTypeSymbol> instance = PooledHashSet<NamedTypeSymbol>.GetInstance();
		PooledHashSet<NamedTypeSymbol> instance2 = PooledHashSet<NamedTypeSymbol>.GetInstance();
		StructDependsClosure(depends, instance, instance2, ConsList<NamedTypeSymbol>.Empty.Prepend(on));
		bool result = instance2.Contains(on);
		instance2.Free();
		instance.Free();
		return result;
	}

	private static void StructDependsClosure(NamedTypeSymbol type, HashSet<NamedTypeSymbol> partialClosure, HashSet<NamedTypeSymbol> typesWithCycle, ConsList<NamedTypeSymbol> on)
	{
		if (typesWithCycle.Contains(type.OriginalDefinition))
		{
			return;
		}
		if (on.ContainsReference(type.OriginalDefinition))
		{
			typesWithCycle.Add(type.OriginalDefinition);
		}
		else if (partialClosure.Add(type))
		{
			if (!type.IsDefinition)
			{
				visitFields(type.OriginalDefinition, partialClosure, typesWithCycle, on.Prepend(type.OriginalDefinition));
			}
			visitFields(type, partialClosure, typesWithCycle, on);
		}
		static void visitFields(NamedTypeSymbol namedTypeSymbol, HashSet<NamedTypeSymbol> partialClosure2, HashSet<NamedTypeSymbol> typesWithCycle2, ConsList<NamedTypeSymbol> consList)
		{
			foreach (Symbol item in namedTypeSymbol.GetMembersUnordered())
			{
				FieldSymbol fieldSymbol = item as FieldSymbol;
				TypeSymbol typeSymbol = fieldSymbol?.NonPointerType();
				if ((object)typeSymbol != null && typeSymbol.TypeKind == TypeKind.Struct && !fieldSymbol.IsStatic)
				{
					StructDependsClosure((NamedTypeSymbol)typeSymbol, partialClosure2, typesWithCycle2, consList);
				}
			}
		}
	}

	internal static ManagedKind GetManagedKind(NamedTypeSymbol type, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		(ThreeState isManaged, bool hasGenerics) tuple = INamedTypeSymbolInternal.Helpers.IsManagedTypeHelper(type);
		ThreeState item = tuple.isManaged;
		bool flag = tuple.hasGenerics;
		bool flag2 = item == ThreeState.True;
		if (item == ThreeState.Unknown)
		{
			PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
			(bool, bool) tuple2 = dependsOnDefinitelyManagedType(type, instance, ref useSiteInfo);
			flag2 = tuple2.Item1;
			flag = flag || tuple2.Item2;
			instance.Free();
		}
		if (flag2)
		{
			return ManagedKind.Managed;
		}
		if (flag)
		{
			return ManagedKind.UnmanagedWithGenerics;
		}
		return ManagedKind.Unmanaged;
		static (bool definitelyManaged, bool hasGenerics) dependsOnDefinitelyManagedType(NamedTypeSymbol namedTypeSymbol, HashSet<Symbol> partialClosure, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			bool flag3 = false;
			if (partialClosure.Add(namedTypeSymbol))
			{
				foreach (Symbol instanceFieldsAndEvent in namedTypeSymbol.GetInstanceFieldsAndEvents())
				{
					FieldSymbol fieldSymbol = instanceFieldsAndEvent.Kind switch
					{
						SymbolKind.Field => (FieldSymbol)instanceFieldsAndEvent, 
						SymbolKind.Event => ((EventSymbol)instanceFieldsAndEvent).AssociatedField, 
						_ => throw ExceptionUtilities.UnexpectedValue(instanceFieldsAndEvent.Kind), 
					};
					if ((object)fieldSymbol != null)
					{
						if (fieldSymbol.RefKind != RefKind.None)
						{
							return (definitelyManaged: true, hasGenerics: flag3);
						}
						TypeSymbol typeSymbol = fieldSymbol.NonPointerType();
						if ((object)typeSymbol != null)
						{
							typeSymbol.AddUseSiteInfo(ref useSiteInfo2);
							if (!(typeSymbol is NamedTypeSymbol namedTypeSymbol2))
							{
								if (typeSymbol.IsManagedType(ref useSiteInfo2))
								{
									return (definitelyManaged: true, hasGenerics: flag3);
								}
							}
							else
							{
								(ThreeState, bool) tuple3 = INamedTypeSymbolInternal.Helpers.IsManagedTypeHelper(namedTypeSymbol2);
								flag3 = flag3 || tuple3.Item2;
								switch (tuple3.Item1)
								{
								case ThreeState.True:
									return (definitelyManaged: true, hasGenerics: flag3);
								case ThreeState.Unknown:
									if (!namedTypeSymbol2.OriginalDefinition.KnownCircularStruct)
									{
										(bool definitelyManaged, bool hasGenerics) tuple4 = dependsOnDefinitelyManagedType(namedTypeSymbol2, partialClosure, ref useSiteInfo2);
										bool item2 = tuple4.definitelyManaged;
										bool item3 = tuple4.hasGenerics;
										flag3 |= item3;
										if (item2)
										{
											return (definitelyManaged: true, hasGenerics: flag3);
										}
									}
									break;
								}
							}
						}
					}
				}
			}
			return (definitelyManaged: false, hasGenerics: flag3);
		}
	}

	internal static TypeSymbol NonPointerType(this FieldSymbol field)
	{
		if (!field.HasPointerType)
		{
			return field.Type;
		}
		return null;
	}
}
