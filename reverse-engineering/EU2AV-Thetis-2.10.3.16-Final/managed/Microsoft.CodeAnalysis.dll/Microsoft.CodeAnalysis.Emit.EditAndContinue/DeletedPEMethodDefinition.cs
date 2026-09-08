using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal sealed class DeletedPEMethodDefinition : IDeletedMethodDefinition, IMethodDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IMethodReference, ISignature
{
	private readonly IMethodSymbolInternal _oldMethod;

	private readonly DeletedMethodBody _body;

	public bool IsEncDeleted => true;

	public string? Name => _oldMethod.Name;

	public MethodDefinitionHandle MetadataHandle => MetadataTokens.MethodDefinitionHandle(_oldMethod.MetadataToken);

	public BlobHandle MetadataSignatureHandle => _oldMethod.MetadataSignatureHandle;

	public bool HasDeclarativeSecurity => _oldMethod.HasDeclarativeSecurity;

	public bool IsAbstract => _oldMethod.IsAbstract;

	public bool IsAccessCheckedOnOverride => _oldMethod.IsAccessCheckedOnOverride;

	public bool IsExternal => _oldMethod.IsExternal;

	public bool IsHiddenBySignature => _oldMethod.IsHiddenBySignature;

	public bool IsNewSlot => _oldMethod.IsMetadataNewSlot;

	public bool IsPlatformInvoke => _oldMethod.IsPlatformInvoke;

	public bool IsRuntimeSpecial => _oldMethod.HasRuntimeSpecialName;

	public bool IsSealed => _oldMethod.IsMetadataFinal;

	public bool IsSpecialName => _oldMethod.HasSpecialName;

	public bool IsStatic => _oldMethod.IsStatic;

	public bool IsVirtual => _oldMethod.IsVirtual;

	public bool RequiresSecurityObject => _oldMethod.RequiresSecurityObject;

	public TypeMemberVisibility Visibility => _oldMethod.MetadataVisibility;

	public bool HasBody => true;

	public ITypeDefinition ContainingTypeDefinition
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 103);
		}
	}

	public IEnumerable<IGenericMethodParameter> GenericParameters
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 106);
		}
	}

	public bool IsConstructor
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 109);
		}
	}

	public ImmutableArray<IParameterDefinition> Parameters
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 112);
		}
	}

	public IPlatformInvokeInformation PlatformInvokeData
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 115);
		}
	}

	public bool ReturnValueIsMarshalledExplicitly
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 118);
		}
	}

	public IMarshallingInformation ReturnValueMarshallingInformation
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 121);
		}
	}

	public ImmutableArray<byte> ReturnValueMarshallingDescriptor
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 124);
		}
	}

	public IEnumerable<SecurityAttribute> SecurityAttributes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 127);
		}
	}

	public INamespace ContainingNamespace
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 130);
		}
	}

	public bool AcceptsExtraArguments
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 133);
		}
	}

	public ushort GenericParameterCount
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 136);
		}
	}

	public ImmutableArray<IParameterTypeInformation> ExtraParameters
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 139);
		}
	}

	public IGenericMethodInstanceReference? AsGenericMethodInstanceReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 142);
		}
	}

	public ISpecializedMethodReference? AsSpecializedMethodReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 145);
		}
	}

	public CallingConvention CallingConvention
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 148);
		}
	}

	public ushort ParameterCount
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 151);
		}
	}

	public ImmutableArray<ICustomModifier> ReturnValueCustomModifiers
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 154);
		}
	}

	public ImmutableArray<ICustomModifier> RefCustomModifiers
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 157);
		}
	}

	public bool ReturnValueIsByRef
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 160);
		}
	}

	public DeletedPEMethodDefinition(IMethodSymbolInternal oldMethod, ImmutableArray<byte> bodyIL)
	{
		_oldMethod = oldMethod;
		_body = new DeletedMethodBody(this, bodyIL);
	}

	public MethodImplAttributes GetImplementationAttributes(EmitContext context)
	{
		return _oldMethod.ImplementationAttributes;
	}

	public IMethodBody GetBody(EmitContext context)
	{
		return _body;
	}

	public void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public ISymbolInternal? GetInternalSymbol()
	{
		return _oldMethod;
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public IDefinition? AsDefinition(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 163);
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 166);
	}

	public ImmutableArray<IParameterTypeInformation> GetParameters(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 169);
	}

	public IMethodDefinition GetResolvedMethod(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 172);
	}

	public IEnumerable<ICustomAttribute> GetReturnValueAttributes(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 175);
	}

	public ITypeReference GetType(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 178);
	}

	public sealed override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 182);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedPEMethodDefinition.cs", 186);
	}
}
