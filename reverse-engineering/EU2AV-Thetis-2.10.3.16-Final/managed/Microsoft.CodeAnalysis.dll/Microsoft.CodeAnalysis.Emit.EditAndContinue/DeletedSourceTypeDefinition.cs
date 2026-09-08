using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal sealed class DeletedSourceTypeDefinition : DeletedSourceDefinition<ITypeDefinition>, ITypeDefinition, IDefinition, IReference, ITypeReference
{
	public ushort Alignment => OldDefinition.Alignment;

	public IEnumerable<IGenericTypeParameter> GenericParameters => OldDefinition.GenericParameters;

	public ushort GenericParameterCount => OldDefinition.GenericParameterCount;

	public bool HasDeclarativeSecurity => OldDefinition.HasDeclarativeSecurity;

	public bool IsAbstract => OldDefinition.IsAbstract;

	public bool IsBeforeFieldInit => OldDefinition.IsBeforeFieldInit;

	public bool IsComObject => OldDefinition.IsComObject;

	public bool IsGeneric => OldDefinition.IsGeneric;

	public bool IsInterface => OldDefinition.IsInterface;

	public bool IsDelegate => OldDefinition.IsDelegate;

	public bool IsRuntimeSpecial => OldDefinition.IsRuntimeSpecial;

	public bool IsSerializable => OldDefinition.IsSerializable;

	public bool IsSpecialName => OldDefinition.IsSpecialName;

	public bool IsWindowsRuntimeImport => OldDefinition.IsWindowsRuntimeImport;

	public bool IsSealed => OldDefinition.IsSealed;

	public LayoutKind Layout => OldDefinition.Layout;

	public IEnumerable<SecurityAttribute> SecurityAttributes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 60);
		}
	}

	public uint SizeOf => OldDefinition.SizeOf;

	public CharSet StringFormat => OldDefinition.StringFormat;

	public bool IsEnum => OldDefinition.IsEnum;

	public bool IsValueType => OldDefinition.IsValueType;

	public Microsoft.Cci.PrimitiveTypeCode TypeCode => OldDefinition.TypeCode;

	public TypeDefinitionHandle TypeDef => OldDefinition.TypeDef;

	public IGenericMethodParameterReference? AsGenericMethodParameterReference => OldDefinition.AsGenericMethodParameterReference;

	public IGenericTypeInstanceReference? AsGenericTypeInstanceReference => OldDefinition.AsGenericTypeInstanceReference;

	public IGenericTypeParameterReference? AsGenericTypeParameterReference => OldDefinition.AsGenericTypeParameterReference;

	public INamespaceTypeReference? AsNamespaceTypeReference => OldDefinition.AsNamespaceTypeReference;

	public INestedTypeReference? AsNestedTypeReference => OldDefinition.AsNestedTypeReference;

	public ISpecializedNestedTypeReference? AsSpecializedNestedTypeReference => OldDefinition.AsSpecializedNestedTypeReference;

	public DeletedSourceTypeDefinition(ITypeDefinition oldDefinition, Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> typesUsedByDeletedMembers)
		: base(oldDefinition, typesUsedByDeletedMembers, (ICustomAttribute?)null)
	{
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public INamespaceTypeDefinition? AsNamespaceTypeDefinition(EmitContext context)
	{
		return OldDefinition.AsNamespaceTypeDefinition(context);
	}

	public INestedTypeDefinition? AsNestedTypeDefinition(EmitContext context)
	{
		return OldDefinition.AsNestedTypeDefinition(context);
	}

	public ITypeDefinition? AsTypeDefinition(EmitContext context)
	{
		return this;
	}

	public ITypeDefinition? GetResolvedType(EmitContext context)
	{
		return OldDefinition.GetResolvedType(context);
	}

	public ITypeReference? GetBaseClass(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 107);
	}

	public IEnumerable<IEventDefinition> GetEvents(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 110);
	}

	public IEnumerable<Microsoft.Cci.MethodImplementation> GetExplicitImplementationOverrides(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 113);
	}

	public IEnumerable<IFieldDefinition> GetFields(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 116);
	}

	public IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 119);
	}

	public IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 122);
	}

	public IEnumerable<IPropertyDefinition> GetProperties(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 125);
	}

	public IEnumerable<TypeReferenceWithAttributes> Interfaces(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceTypeDefinition.cs", 128);
	}
}
