using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.Cci;

internal abstract class ParameterDefinitionBase : IParameterDefinition, IDefinition, IReference, INamedEntity, IParameterTypeInformation, IParameterListEntry
{
	public bool HasDefaultValue => false;

	public bool IsIn => false;

	public virtual bool IsMarshalledExplicitly => false;

	public bool IsOptional => false;

	public bool IsOut => false;

	public virtual IMarshallingInformation? MarshallingInformation => null;

	public virtual ImmutableArray<byte> MarshallingDescriptor => default(ImmutableArray<byte>);

	public bool IsEncDeleted => false;

	public abstract string Name { get; }

	public virtual ImmutableArray<ICustomModifier> CustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public virtual ImmutableArray<ICustomModifier> RefCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public virtual bool IsByReference => false;

	public abstract ushort Index { get; }

	public IDefinition? AsDefinition(EmitContext context)
	{
		return this;
	}

	public void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public virtual IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return Array.Empty<ICustomAttribute>();
	}

	public MetadataConstant? GetDefaultValue(EmitContext context)
	{
		return null;
	}

	public ISymbolInternal? GetInternalSymbol()
	{
		return null;
	}

	public abstract ITypeReference GetType(EmitContext context);

	public sealed override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/ParameterDefinitionBase.cs", 40);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/ParameterDefinitionBase.cs", 46);
	}
}
