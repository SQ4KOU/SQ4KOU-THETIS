using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class MemberRefMetadataDecoder : MetadataDecoder
{
	private readonly TypeSymbol _containingType;

	public MemberRefMetadataDecoder(PEModuleSymbol moduleSymbol, TypeSymbol containingType)
		: base(moduleSymbol, containingType as PENamedTypeSymbol)
	{
		_containingType = containingType;
	}

	protected override TypeSymbol GetGenericMethodTypeParamSymbol(int position)
	{
		return IndexedTypeParameterSymbol.GetTypeParameter(position);
	}

	protected override TypeSymbol GetGenericTypeParamSymbol(int position)
	{
		if (_containingType is PENamedTypeSymbol)
		{
			return base.GetGenericTypeParamSymbol(position);
		}
		if (_containingType is NamedTypeSymbol namedType)
		{
			GetGenericTypeParameterSymbol(position, namedType, out var _, out var typeArgument);
			if ((object)typeArgument != null)
			{
				return typeArgument;
			}
			return new UnsupportedMetadataTypeSymbol();
		}
		return new UnsupportedMetadataTypeSymbol();
	}

	private static void GetGenericTypeParameterSymbol(int position, NamedTypeSymbol namedType, out int cumulativeArity, out TypeParameterSymbol typeArgument)
	{
		cumulativeArity = namedType.Arity;
		typeArgument = null;
		int num = 0;
		NamedTypeSymbol containingType = namedType.ContainingType;
		if ((object)containingType != null)
		{
			GetGenericTypeParameterSymbol(position, containingType, out var cumulativeArity2, out typeArgument);
			cumulativeArity += cumulativeArity2;
			num = cumulativeArity2;
		}
		if (num <= position && position < cumulativeArity)
		{
			typeArgument = namedType.TypeParameters[position - num];
		}
	}

	internal Symbol FindMember(EntityHandle memberRefOrMethodDef, bool methodsOnly)
	{
		try
		{
			string targetMemberName;
			BlobHandle signature;
			switch (memberRefOrMethodDef.Kind)
			{
			case HandleKind.MemberReference:
			{
				MemberReferenceHandle memberRef = (MemberReferenceHandle)memberRefOrMethodDef;
				targetMemberName = Module.GetMemberRefNameOrThrow(memberRef);
				signature = Module.GetSignatureOrThrow(memberRef);
				break;
			}
			case HandleKind.MethodDefinition:
			{
				MethodDefinitionHandle methodDef = (MethodDefinitionHandle)memberRefOrMethodDef;
				targetMemberName = Module.GetMethodDefNameOrThrow(methodDef);
				signature = Module.GetMethodSignatureOrThrow(methodDef);
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(memberRefOrMethodDef.Kind);
			}
			BlobReader signatureReader = DecodeSignatureHeaderOrThrow(signature, out var signatureHeader);
			switch (signatureHeader.RawValue & 0xF)
			{
			case 0:
			case 5:
			{
				ParamInfo<TypeSymbol>[] targetParamInfo = DecodeSignatureParametersOrThrow(ref signatureReader, signatureHeader, out var typeParameterCount);
				return FindMethodBySignature(_containingType, targetMemberName, signatureHeader, typeParameterCount, targetParamInfo);
			}
			case 6:
			{
				if (methodsOnly)
				{
					return null;
				}
				FieldInfo<TypeSymbol> fieldInfo = DecodeFieldSignature(ref signatureReader);
				return FindFieldBySignature(_containingType, targetMemberName, in fieldInfo);
			}
			default:
				return null;
			}
		}
		catch (BadImageFormatException)
		{
			return null;
		}
	}

	private static FieldSymbol FindFieldBySignature(TypeSymbol targetTypeSymbol, string targetMemberName, in FieldInfo<TypeSymbol> fieldInfo)
	{
		foreach (Symbol member in targetTypeSymbol.GetMembers(targetMemberName))
		{
			if (member is FieldSymbol fieldSymbol && fieldSymbol.RefKind != RefKind.None == fieldInfo.IsByRef && CustomModifiersMatch(fieldSymbol.RefCustomModifiers, fieldInfo.RefCustomModifiers))
			{
				TypeWithAnnotations typeWithAnnotations2;
				TypeWithAnnotations typeWithAnnotations = (typeWithAnnotations2 = fieldSymbol.TypeWithAnnotations);
				if (TypeSymbol.Equals(typeWithAnnotations.Type, fieldInfo.Type, TypeCompareKind.CLRSignatureCompareOptions) && CustomModifiersMatch(typeWithAnnotations2.CustomModifiers, fieldInfo.CustomModifiers))
				{
					return fieldSymbol;
				}
			}
		}
		return null;
	}

	private static MethodSymbol FindMethodBySignature(TypeSymbol targetTypeSymbol, string targetMemberName, SignatureHeader targetMemberSignatureHeader, int targetMemberTypeParamCount, ParamInfo<TypeSymbol>[] targetParamInfo)
	{
		foreach (Symbol member in targetTypeSymbol.GetMembers(targetMemberName))
		{
			if (member is MethodSymbol methodSymbol && (byte)methodSymbol.CallingConvention == targetMemberSignatureHeader.RawValue && targetMemberTypeParamCount == methodSymbol.Arity && MethodSymbolMatchesParamInfo(methodSymbol, targetParamInfo))
			{
				return methodSymbol;
			}
		}
		return null;
	}

	private static bool MethodSymbolMatchesParamInfo(MethodSymbol candidateMethod, ParamInfo<TypeSymbol>[] targetParamInfo)
	{
		int num = targetParamInfo.Length - 1;
		if (candidateMethod.ParameterCount != num)
		{
			return false;
		}
		TypeMap candidateMethodTypeMap = new TypeMap(candidateMethod.TypeParameters, IndexedTypeParameterSymbol.Take(candidateMethod.Arity), allowAlpha: true);
		if (!ReturnTypesMatch(candidateMethod, candidateMethodTypeMap, ref targetParamInfo[0]))
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			if (!ParametersMatch(candidateMethod.Parameters[i], candidateMethodTypeMap, ref targetParamInfo[i + 1]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ParametersMatch(ParameterSymbol candidateParam, TypeMap candidateMethodTypeMap, ref ParamInfo<TypeSymbol> targetParam)
	{
		if (candidateParam.RefKind != RefKind.None != targetParam.IsByRef)
		{
			return false;
		}
		TypeWithAnnotations typeWithAnnotations = candidateParam.TypeWithAnnotations.SubstituteType(candidateMethodTypeMap);
		if (!TypeSymbol.Equals(typeWithAnnotations.Type, targetParam.Type, TypeCompareKind.CLRSignatureCompareOptions))
		{
			return false;
		}
		if (!CustomModifiersMatch(typeWithAnnotations.CustomModifiers, targetParam.CustomModifiers) || !CustomModifiersMatch(candidateMethodTypeMap.SubstituteCustomModifiers(candidateParam.RefCustomModifiers), targetParam.RefCustomModifiers))
		{
			return false;
		}
		return true;
	}

	private static bool ReturnTypesMatch(MethodSymbol candidateMethod, TypeMap candidateMethodTypeMap, ref ParamInfo<TypeSymbol> targetReturnParam)
	{
		if (candidateMethod.ReturnsByRef != targetReturnParam.IsByRef)
		{
			return false;
		}
		TypeWithAnnotations returnTypeWithAnnotations = candidateMethod.ReturnTypeWithAnnotations;
		TypeSymbol type = targetReturnParam.Type;
		TypeWithAnnotations typeWithAnnotations = returnTypeWithAnnotations.SubstituteType(candidateMethodTypeMap);
		if (!TypeSymbol.Equals(typeWithAnnotations.Type, type, TypeCompareKind.CLRSignatureCompareOptions))
		{
			return false;
		}
		if (!CustomModifiersMatch(typeWithAnnotations.CustomModifiers, targetReturnParam.CustomModifiers) || !CustomModifiersMatch(candidateMethodTypeMap.SubstituteCustomModifiers(candidateMethod.RefCustomModifiers), targetReturnParam.RefCustomModifiers))
		{
			return false;
		}
		return true;
	}

	private static bool CustomModifiersMatch(ImmutableArray<CustomModifier> candidateCustomModifiers, ImmutableArray<ModifierInfo<TypeSymbol>> targetCustomModifiers)
	{
		if (targetCustomModifiers.IsDefault || targetCustomModifiers.IsEmpty)
		{
			if (!candidateCustomModifiers.IsDefault)
			{
				return candidateCustomModifiers.IsEmpty;
			}
			return true;
		}
		if (candidateCustomModifiers.IsDefault)
		{
			return false;
		}
		int length = candidateCustomModifiers.Length;
		if (targetCustomModifiers.Length != length)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			ModifierInfo<TypeSymbol> modifierInfo = targetCustomModifiers[i];
			CustomModifier customModifier = candidateCustomModifiers[i];
			if (modifierInfo.IsOptional != customModifier.IsOptional || !object.Equals(modifierInfo.Modifier, ((CSharpCustomModifier)customModifier).ModifierSymbol))
			{
				return false;
			}
		}
		return true;
	}
}
