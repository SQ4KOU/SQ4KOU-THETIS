using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MergedNamespaceDeclaration : MergedNamespaceOrTypeDeclaration
{
	private readonly ImmutableArray<SingleNamespaceDeclaration> _declarations;

	private ImmutableArray<MergedNamespaceOrTypeDeclaration> _lazyChildren;

	public override DeclarationKind Kind => DeclarationKind.Namespace;

	public ImmutableArray<Location> NameLocations
	{
		get
		{
			if (_declarations.Length == 1)
			{
				return ImmutableArray.Create((Location)_declarations[0].NameLocation);
			}
			ArrayBuilder<Location> instance = ArrayBuilder<Location>.GetInstance();
			foreach (SingleNamespaceDeclaration declaration in _declarations)
			{
				SourceLocation nameLocation = declaration.NameLocation;
				if (nameLocation != null)
				{
					instance.Add(nameLocation);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	public ImmutableArray<SingleNamespaceDeclaration> Declarations => _declarations;

	public new ImmutableArray<MergedNamespaceOrTypeDeclaration> Children
	{
		get
		{
			if (_lazyChildren.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyChildren, MakeChildren());
			}
			return _lazyChildren;
		}
	}

	private MergedNamespaceDeclaration(ImmutableArray<SingleNamespaceDeclaration> declarations)
		: base(declarations.IsEmpty ? string.Empty : declarations[0].Name)
	{
		_declarations = declarations;
	}

	public static MergedNamespaceDeclaration Create(ImmutableArray<SingleNamespaceDeclaration> declarations)
	{
		return new MergedNamespaceDeclaration(declarations);
	}

	public static MergedNamespaceDeclaration Create(SingleNamespaceDeclaration declaration)
	{
		return new MergedNamespaceDeclaration(ImmutableArray.Create(declaration));
	}

	public LexicalSortKey GetLexicalSortKey(CSharpCompilation compilation)
	{
		LexicalSortKey lexicalSortKey = new LexicalSortKey(_declarations[0].NameLocation, compilation);
		for (int i = 1; i < _declarations.Length; i++)
		{
			lexicalSortKey = LexicalSortKey.First(lexicalSortKey, new LexicalSortKey(_declarations[i].NameLocation, compilation));
		}
		return lexicalSortKey;
	}

	protected override ImmutableArray<Declaration> GetDeclarationChildren()
	{
		return StaticCast<Declaration>.From(Children);
	}

	private ImmutableArray<MergedNamespaceOrTypeDeclaration> MakeChildren()
	{
		ArrayBuilder<SingleNamespaceDeclaration> arrayBuilder = null;
		ArrayBuilder<SingleTypeDeclaration> arrayBuilder2 = null;
		bool flag = true;
		bool flag2 = true;
		foreach (SingleNamespaceDeclaration declaration in _declarations)
		{
			foreach (SingleNamespaceOrTypeDeclaration child in declaration.Children)
			{
				if (child is SingleTypeDeclaration singleTypeDeclaration)
				{
					if (arrayBuilder2 == null)
					{
						arrayBuilder2 = ArrayBuilder<SingleTypeDeclaration>.GetInstance();
					}
					else if (flag2 && !singleTypeDeclaration.Identity.Equals(arrayBuilder2[0].Identity))
					{
						flag2 = false;
					}
					arrayBuilder2.Add(singleTypeDeclaration);
				}
				else if (child is SingleNamespaceDeclaration singleNamespaceDeclaration)
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<SingleNamespaceDeclaration>.GetInstance();
					}
					else if (flag && !singleNamespaceDeclaration.Name.Equals(arrayBuilder[0].Name))
					{
						flag = false;
					}
					arrayBuilder.Add(singleNamespaceDeclaration);
				}
			}
		}
		ArrayBuilder<MergedNamespaceOrTypeDeclaration> instance = ArrayBuilder<MergedNamespaceOrTypeDeclaration>.GetInstance();
		addNamespacesToChildren(arrayBuilder, flag, instance);
		addTypesToChildren(arrayBuilder2, flag2, instance);
		return instance.ToImmutableAndFree();
		static void addNamespacesToChildren(ArrayBuilder<SingleNamespaceDeclaration> namespaces, bool allNamespacesHaveSameName, ArrayBuilder<MergedNamespaceOrTypeDeclaration> children)
		{
			if (namespaces != null)
			{
				if (!allNamespacesHaveSameName)
				{
					Dictionary<string, ArrayBuilder<SingleNamespaceDeclaration>> dictionary = new Dictionary<string, ArrayBuilder<SingleNamespaceDeclaration>>(StringOrdinalComparer.Instance);
					foreach (SingleNamespaceDeclaration @namespace in namespaces)
					{
						dictionary.GetOrAdd(@namespace.Name, () => ArrayBuilder<SingleNamespaceDeclaration>.GetInstance()).Add(@namespace);
					}
					namespaces.Free();
					{
						foreach (var (_, arrayBuilder4) in dictionary)
						{
							children.Add(Create(arrayBuilder4.ToImmutableAndFree()));
						}
						return;
					}
				}
				children.Add(Create(namespaces.ToImmutableAndFree()));
			}
		}
		static void addTypesToChildren(ArrayBuilder<SingleTypeDeclaration> types, bool allTypesHaveSameIdentity, ArrayBuilder<MergedNamespaceOrTypeDeclaration> children)
		{
			if (types != null)
			{
				if (allTypesHaveSameIdentity)
				{
					children.Add(new MergedTypeDeclaration(types.ToImmutableAndFree()));
				}
				else
				{
					PooledDictionary<SingleTypeDeclaration.TypeDeclarationIdentity, object> instance2 = PooledDictionary<SingleTypeDeclaration.TypeDeclarationIdentity, object>.GetInstance();
					foreach (SingleTypeDeclaration type in types)
					{
						SingleTypeDeclaration.TypeDeclarationIdentity identity = type.Identity;
						if (instance2.TryGetValue(identity, out var value))
						{
							ArrayBuilder<SingleTypeDeclaration> arrayBuilder3 = value as ArrayBuilder<SingleTypeDeclaration>;
							if (arrayBuilder3 == null)
							{
								arrayBuilder3 = ArrayBuilder<SingleTypeDeclaration>.GetInstance();
								arrayBuilder3.Add((SingleTypeDeclaration)value);
								instance2[identity] = arrayBuilder3;
							}
							arrayBuilder3.Add(type);
						}
						else
						{
							instance2.Add(identity, type);
						}
					}
					foreach (var (_, obj2) in instance2)
					{
						if (obj2 is SingleTypeDeclaration singleTypeDeclaration2)
						{
							children.Add(new MergedTypeDeclaration(ImmutableCollectionsMarshal.AsImmutableArray(new SingleTypeDeclaration[1] { singleTypeDeclaration2 })));
						}
						else
						{
							ArrayBuilder<SingleTypeDeclaration> arrayBuilder4 = (ArrayBuilder<SingleTypeDeclaration>)obj2;
							children.Add(new MergedTypeDeclaration(arrayBuilder4.ToImmutableAndFree()));
						}
					}
					types.Free();
					instance2.Free();
				}
			}
		}
	}
}
