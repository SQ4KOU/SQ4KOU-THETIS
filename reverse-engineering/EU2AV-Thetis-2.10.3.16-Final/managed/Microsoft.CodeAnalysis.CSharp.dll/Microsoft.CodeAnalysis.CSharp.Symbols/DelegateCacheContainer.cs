using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class DelegateCacheContainer : SynthesizedContainer
{
	private sealed class CLRSignatureComparer : IEqualityComparer<(TypeSymbol? constrainedToTypeOpt, TypeSymbol delegateType, MethodSymbol targetMethod)>
	{
		public static readonly CLRSignatureComparer Instance = new CLRSignatureComparer();

		public bool Equals((TypeSymbol? constrainedToTypeOpt, TypeSymbol delegateType, MethodSymbol targetMethod) x, (TypeSymbol? constrainedToTypeOpt, TypeSymbol delegateType, MethodSymbol targetMethod) y)
		{
			EqualityComparer<Symbol> cLRSignature = SymbolEqualityComparer.CLRSignature;
			if (cLRSignature.Equals(x.delegateType, y.delegateType) && cLRSignature.Equals(x.targetMethod, y.targetMethod))
			{
				return cLRSignature.Equals(x.constrainedToTypeOpt, y.constrainedToTypeOpt);
			}
			return false;
		}

		public int GetHashCode((TypeSymbol? constrainedToTypeOpt, TypeSymbol delegateType, MethodSymbol targetMethod) conversion)
		{
			EqualityComparer<Symbol> cLRSignature = SymbolEqualityComparer.CLRSignature;
			int num = Hash.Combine(cLRSignature.GetHashCode(conversion.delegateType), cLRSignature.GetHashCode(conversion.targetMethod));
			var (typeSymbol, _, _) = conversion;
			if ((object)typeSymbol != null)
			{
				num = Hash.Combine(num, cLRSignature.GetHashCode(typeSymbol));
			}
			return num;
		}
	}

	private readonly Symbol _containingSymbol;

	private readonly NamedTypeSymbol? _constructedContainer;

	private readonly Dictionary<(TypeSymbol?, TypeSymbol, MethodSymbol), FieldSymbol> _delegateFields = new Dictionary<(TypeSymbol, TypeSymbol, MethodSymbol), FieldSymbol>(CLRSignatureComparer.Instance);

	public override Symbol ContainingSymbol => _containingSymbol;

	public override bool AreLocalsZeroed
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Lowering/LocalRewriter/DelegateCacheContainer.cs", 48);
		}
	}

	public override TypeKind TypeKind => TypeKind.Class;

	public override bool IsStatic => true;

	internal override bool IsRecord => false;

	internal override bool IsRecordStruct => false;

	internal DelegateCacheContainer(NamedTypeSymbol containingType, int generationOrdinal)
		: base(GeneratedNames.DelegateCacheContainerType(generationOrdinal))
	{
		_containingSymbol = containingType;
	}

	internal DelegateCacheContainer(NamedTypeSymbol containingType, Symbol owner, int topLevelMethodOrdinal, int ownerUniqueId, int generationOrdinal)
	{
		string name = GeneratedNames.DelegateCacheContainerType(generationOrdinal, owner.Name, topLevelMethodOrdinal, ownerUniqueId);
		ImmutableArray<TypeParameterSymbol> typeParametersToAlphaRename;
		if (!(owner is NamedTypeSymbol namedTypeSymbol))
		{
			NamedTypeSymbol containingType2 = owner.ContainingType;
			typeParametersToAlphaRename = (((object)containingType2 != null && containingType2.IsExtension) ? containingType2.TypeParameters : ImmutableArray<TypeParameterSymbol>.Empty).Concat(TypeMap.ConcatMethodTypeParameters((MethodSymbol)owner, null));
		}
		else
		{
			typeParametersToAlphaRename = namedTypeSymbol.TypeParameters;
		}
		base._002Ector(name, typeParametersToAlphaRename);
		_containingSymbol = containingType;
		_constructedContainer = Construct(base.ConstructedFromTypeParameters);
	}

	internal override bool HasPossibleWellKnownCloneMethod()
	{
		return false;
	}

	internal FieldSymbol GetOrAddCacheField(SyntheticBoundNodeFactory factory, BoundDelegateCreationExpression boundDelegateCreation)
	{
		MethodSymbol methodOpt = boundDelegateCreation.MethodOpt;
		TypeSymbol type = boundDelegateCreation.Type;
		TypeSymbol item = (((methodOpt.IsAbstract || methodOpt.IsVirtual) && boundDelegateCreation.Argument is BoundTypeExpression boundTypeExpression) ? boundTypeExpression.Type : null);
		if (_delegateFields.TryGetValue((item, type, methodOpt), out FieldSymbol value))
		{
			return value;
		}
		TypeSymbol type2 = (TypeParameters.IsEmpty ? type : base.TypeMap.SubstituteType(type).Type);
		string name = GeneratedNames.DelegateCacheContainerFieldName(_delegateFields.Count, methodOpt.Name);
		value = new SynthesizedFieldSymbol(this, type2, name, DeclarationModifiers.Public, isReadOnly: false, isStatic: true);
		factory.AddField(this, value);
		if (!TypeParameters.IsEmpty)
		{
			value = value.AsMember(_constructedContainer);
		}
		_delegateFields.Add((item, type, methodOpt), value);
		return value;
	}
}
