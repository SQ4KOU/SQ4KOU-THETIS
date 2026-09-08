using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal struct NativeIntegerTypeDecoder
{
	private readonly ImmutableArray<bool> _transformFlags;

	private int _index;

	private bool _hitErrorType;

	internal static TypeSymbol TransformType(TypeSymbol type, EntityHandle handle, PEModuleSymbol containingModule, TypeSymbol? containingType)
	{
		if ((object)containingType == null || containingType.SpecialType != SpecialType.System_Runtime_CompilerServices_RuntimeFeature)
		{
			AssemblySymbol containingAssembly = type.ContainingAssembly;
			if ((object)containingAssembly == null || !containingAssembly.RuntimeSupportsNumericIntPtr)
			{
				if (!containingModule.Module.HasNativeIntegerAttribute(handle, out var transformFlags))
				{
					return type;
				}
				return TransformType(type, transformFlags);
			}
		}
		return type;
	}

	internal static TypeSymbol TransformType(TypeSymbol type, ImmutableArray<bool> transformFlags)
	{
		NativeIntegerTypeDecoder nativeIntegerTypeDecoder = new NativeIntegerTypeDecoder(transformFlags);
		try
		{
			TypeSymbol result = nativeIntegerTypeDecoder.TransformType(type);
			if (nativeIntegerTypeDecoder._hitErrorType)
			{
				return type;
			}
			if (nativeIntegerTypeDecoder._index == transformFlags.Length)
			{
				return result;
			}
			return new UnsupportedMetadataTypeSymbol();
		}
		catch (UnsupportedSignatureContent)
		{
			return new UnsupportedMetadataTypeSymbol();
		}
	}

	private NativeIntegerTypeDecoder(ImmutableArray<bool> transformFlags)
	{
		_transformFlags = transformFlags;
		_index = 0;
		_hitErrorType = false;
	}

	private TypeWithAnnotations? TransformTypeWithAnnotations(TypeWithAnnotations type)
	{
		TypeSymbol typeSymbol = TransformType(type.Type);
		if ((object)typeSymbol != null)
		{
			return type.WithTypeAndModifiers(typeSymbol, type.CustomModifiers);
		}
		return null;
	}

	private TypeSymbol? TransformType(TypeSymbol type)
	{
		switch (type.TypeKind)
		{
		case TypeKind.Array:
			return TransformArrayType((ArrayTypeSymbol)type);
		case TypeKind.Pointer:
			return TransformPointerType((PointerTypeSymbol)type);
		case TypeKind.FunctionPointer:
			return TransformFunctionPointerType((FunctionPointerTypeSymbol)type);
		case TypeKind.Dynamic:
		case TypeKind.TypeParameter:
			return type;
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Interface:
		case TypeKind.Struct:
			return TransformNamedType((NamedTypeSymbol)type);
		default:
			_hitErrorType = true;
			return null;
		}
	}

	private NamedTypeSymbol? TransformNamedType(NamedTypeSymbol type)
	{
		if (!type.IsGenericType)
		{
			SpecialType specialType = type.SpecialType;
			if ((uint)(specialType - 21) <= 1u)
			{
				if (_index >= _transformFlags.Length)
				{
					throw new UnsupportedSignatureContent();
				}
				bool num = _transformFlags[_index++];
				bool isNativeIntegerWrapperType = type.IsNativeIntegerWrapperType;
				if (!num)
				{
					if (isNativeIntegerWrapperType)
					{
						return type.NativeIntegerUnderlyingType;
					}
				}
				else if (!isNativeIntegerWrapperType)
				{
					return type.AsNativeInteger();
				}
				return type;
			}
		}
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		type.GetAllTypeArgumentsNoUseSiteDiagnostics(instance);
		bool flag = false;
		for (int i = 0; i < instance.Count; i++)
		{
			TypeWithAnnotations type2 = instance[i];
			TypeWithAnnotations? typeWithAnnotations = TransformTypeWithAnnotations(type2);
			if (typeWithAnnotations.HasValue)
			{
				TypeWithAnnotations valueOrDefault = typeWithAnnotations.GetValueOrDefault();
				if (!type2.IsSameAs(valueOrDefault))
				{
					instance[i] = valueOrDefault;
					flag = true;
				}
				continue;
			}
			return null;
		}
		NamedTypeSymbol result = (flag ? type.WithTypeArguments(instance.ToImmutable()) : type);
		instance.Free();
		return result;
	}

	private ArrayTypeSymbol? TransformArrayType(ArrayTypeSymbol type)
	{
		TypeWithAnnotations? typeWithAnnotations = TransformTypeWithAnnotations(type.ElementTypeWithAnnotations);
		if (typeWithAnnotations.HasValue)
		{
			TypeWithAnnotations valueOrDefault = typeWithAnnotations.GetValueOrDefault();
			return type.WithElementType(valueOrDefault);
		}
		return null;
	}

	private PointerTypeSymbol? TransformPointerType(PointerTypeSymbol type)
	{
		TypeWithAnnotations? typeWithAnnotations = TransformTypeWithAnnotations(type.PointedAtTypeWithAnnotations);
		if (typeWithAnnotations.HasValue)
		{
			TypeWithAnnotations valueOrDefault = typeWithAnnotations.GetValueOrDefault();
			return type.WithPointedAtType(valueOrDefault);
		}
		return null;
	}

	private FunctionPointerTypeSymbol? TransformFunctionPointerType(FunctionPointerTypeSymbol type)
	{
		TypeWithAnnotations? typeWithAnnotations = TransformTypeWithAnnotations(type.Signature.ReturnTypeWithAnnotations);
		if (typeWithAnnotations.HasValue)
		{
			TypeWithAnnotations valueOrDefault = typeWithAnnotations.GetValueOrDefault();
			ImmutableArray<TypeWithAnnotations> substitutedParameterTypes = ImmutableArray<TypeWithAnnotations>.Empty;
			bool flag = false;
			if (type.Signature.ParameterCount > 0)
			{
				ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance(type.Signature.ParameterCount);
				foreach (ParameterSymbol parameter in type.Signature.Parameters)
				{
					typeWithAnnotations = TransformTypeWithAnnotations(parameter.TypeWithAnnotations);
					if (typeWithAnnotations.HasValue)
					{
						TypeWithAnnotations valueOrDefault2 = typeWithAnnotations.GetValueOrDefault();
						flag = flag || !valueOrDefault2.IsSameAs(parameter.TypeWithAnnotations);
						instance.Add(valueOrDefault2);
						continue;
					}
					return null;
				}
				if (flag)
				{
					substitutedParameterTypes = instance.ToImmutableAndFree();
				}
				else
				{
					substitutedParameterTypes = type.Signature.ParameterTypesWithAnnotations;
					instance.Free();
				}
			}
			if (flag || !valueOrDefault.IsSameAs(type.Signature.ReturnTypeWithAnnotations))
			{
				return type.SubstituteTypeSymbol(valueOrDefault, substitutedParameterTypes, default(ImmutableArray<CustomModifier>), default(ImmutableArray<ImmutableArray<CustomModifier>>));
			}
			return type;
		}
		return null;
	}
}
