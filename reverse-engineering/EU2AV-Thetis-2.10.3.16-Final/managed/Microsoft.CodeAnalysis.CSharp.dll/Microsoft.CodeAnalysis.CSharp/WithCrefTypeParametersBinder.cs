using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class WithCrefTypeParametersBinder : WithTypeParametersBinder
{
	private readonly CrefSyntax _crefSyntax;

	private MultiDictionary<string, TypeParameterSymbol> _lazyTypeParameterMap;

	protected override MultiDictionary<string, TypeParameterSymbol> TypeParameterMap
	{
		get
		{
			if (_lazyTypeParameterMap == null)
			{
				MultiDictionary<string, TypeParameterSymbol> value = CreateTypeParameterMap();
				Interlocked.CompareExchange(ref _lazyTypeParameterMap, value, null);
			}
			return _lazyTypeParameterMap;
		}
	}

	internal WithCrefTypeParametersBinder(CrefSyntax crefSyntax, Binder next)
		: base(next)
	{
		_crefSyntax = crefSyntax;
	}

	private MultiDictionary<string, TypeParameterSymbol> CreateTypeParameterMap()
	{
		MultiDictionary<string, TypeParameterSymbol> multiDictionary = new MultiDictionary<string, TypeParameterSymbol>();
		switch (_crefSyntax.Kind())
		{
		case SyntaxKind.TypeCref:
			AddTypeParameters(((TypeCrefSyntax)_crefSyntax).Type, multiDictionary);
			break;
		case SyntaxKind.QualifiedCref:
		{
			QualifiedCrefSyntax qualifiedCrefSyntax = (QualifiedCrefSyntax)_crefSyntax;
			AddTypeParameters(qualifiedCrefSyntax.Member, multiDictionary);
			AddTypeParameters(qualifiedCrefSyntax.Container, multiDictionary);
			break;
		}
		case SyntaxKind.NameMemberCref:
		case SyntaxKind.IndexerMemberCref:
		case SyntaxKind.OperatorMemberCref:
		case SyntaxKind.ConversionOperatorMemberCref:
		case SyntaxKind.ExtensionMemberCref:
			AddTypeParameters((MemberCrefSyntax)_crefSyntax, multiDictionary);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(_crefSyntax.Kind());
		}
		return multiDictionary;
	}

	private void AddTypeParameters(TypeSyntax typeSyntax, MultiDictionary<string, TypeParameterSymbol> map)
	{
		switch (typeSyntax.Kind())
		{
		case SyntaxKind.AliasQualifiedName:
			AddTypeParameters(((AliasQualifiedNameSyntax)typeSyntax).Name, map);
			break;
		case SyntaxKind.QualifiedName:
		{
			QualifiedNameSyntax qualifiedNameSyntax = (QualifiedNameSyntax)typeSyntax;
			AddTypeParameters(qualifiedNameSyntax.Right, map);
			AddTypeParameters(qualifiedNameSyntax.Left, map);
			break;
		}
		case SyntaxKind.GenericName:
			AddTypeParameters(((GenericNameSyntax)typeSyntax).TypeArgumentList.Arguments, map);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(typeSyntax.Kind());
		case SyntaxKind.IdentifierName:
		case SyntaxKind.PredefinedType:
			break;
		}
	}

	private void AddTypeParameters(MemberCrefSyntax memberSyntax, MultiDictionary<string, TypeParameterSymbol> map)
	{
		if (memberSyntax is NameMemberCrefSyntax nameMemberCrefSyntax)
		{
			AddTypeParameters(nameMemberCrefSyntax.Name, map);
		}
		else if (memberSyntax is ExtensionMemberCrefSyntax extensionMemberCrefSyntax)
		{
			TypeArgumentListSyntax typeArgumentList = extensionMemberCrefSyntax.TypeArgumentList;
			if (typeArgumentList != null)
			{
				AddTypeParameters(typeArgumentList.Arguments, map);
			}
			AddTypeParameters(extensionMemberCrefSyntax.Member, map);
		}
	}

	private static void AddTypeParameters(SeparatedSyntaxList<TypeSyntax> typeArguments, MultiDictionary<string, TypeParameterSymbol> map)
	{
		for (int num = typeArguments.Count - 1; num >= 0; num--)
		{
			if (typeArguments[num].Kind() == SyntaxKind.IdentifierName)
			{
				IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)typeArguments[num];
				string valueText = identifierNameSyntax.Identifier.ValueText;
				if (SyntaxFacts.IsValidIdentifier(valueText) && !map.ContainsKey(valueText))
				{
					TypeParameterSymbol v = new CrefTypeParameterSymbol(valueText, num, identifierNameSyntax);
					map.Add(valueText, v);
				}
			}
		}
	}

	internal override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (!CanConsiderTypeParameters(options))
		{
			return;
		}
		foreach (KeyValuePair<string, MultiDictionary<string, TypeParameterSymbol>.ValueSet> item in TypeParameterMap)
		{
			foreach (TypeParameterSymbol item2 in item.Value)
			{
				result.AddSymbol(item2, item.Key, 0);
			}
		}
	}
}
