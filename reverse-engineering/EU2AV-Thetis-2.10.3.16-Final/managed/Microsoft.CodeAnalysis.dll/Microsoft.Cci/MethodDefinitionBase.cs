using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.Cci;

internal abstract class MethodDefinitionBase : IMethodDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IMethodReference, ISignature, IMethodBody
{
	bool IDefinition.IsEncDeleted => false;

	public ITypeDefinition ContainingTypeDefinition { get; }

	public abstract string Name { get; }

	public bool HasBody => true;

	public IEnumerable<IGenericMethodParameter> GenericParameters => SpecializedCollections.EmptyEnumerable<IGenericMethodParameter>();

	public bool HasDeclarativeSecurity => false;

	public bool IsAbstract => false;

	public bool IsAccessCheckedOnOverride => false;

	public bool IsConstructor => false;

	public bool IsExternal => false;

	public bool IsHiddenBySignature => true;

	public bool IsNewSlot => false;

	public bool IsPlatformInvoke => false;

	public virtual bool IsRuntimeSpecial => false;

	public bool IsSealed => false;

	public virtual bool IsSpecialName => false;

	public bool IsStatic => true;

	public bool IsVirtual => false;

	public virtual ImmutableArray<IParameterDefinition> Parameters => ImmutableArray<IParameterDefinition>.Empty;

	public IPlatformInvokeInformation PlatformInvokeData => null;

	public bool RequiresSecurityObject => false;

	public bool ReturnValueIsMarshalledExplicitly => false;

	public IMarshallingInformation ReturnValueMarshallingInformation => null;

	public ImmutableArray<byte> ReturnValueMarshallingDescriptor => default(ImmutableArray<byte>);

	public IEnumerable<SecurityAttribute> SecurityAttributes => null;

	public INamespace ContainingNamespace => null;

	public abstract TypeMemberVisibility Visibility { get; }

	public bool AcceptsExtraArguments => false;

	public ushort GenericParameterCount => 0;

	public ImmutableArray<IParameterTypeInformation> ExtraParameters => ImmutableArray<IParameterTypeInformation>.Empty;

	public IGenericMethodInstanceReference AsGenericMethodInstanceReference => null;

	public ISpecializedMethodReference AsSpecializedMethodReference => null;

	public CallingConvention CallingConvention => CallingConvention.Default;

	public ushort ParameterCount => (ushort)Parameters.Length;

	public ImmutableArray<ICustomModifier> ReturnValueCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public ImmutableArray<ICustomModifier> RefCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public bool ReturnValueIsByRef => false;

	public ushort MaxStack { get; }

	public ImmutableArray<byte> IL { get; }

	public IMethodDefinition MethodDefinition => this;

	public ImmutableArray<ExceptionHandlerRegion> ExceptionRegions => ImmutableArray<ExceptionHandlerRegion>.Empty;

	public bool AreLocalsZeroed => false;

	public bool HasStackalloc => false;

	public ImmutableArray<ILocalDefinition> LocalVariables => ImmutableArray<ILocalDefinition>.Empty;

	public StateMachineMoveNextBodyDebugInfo MoveNextBodyInfo => null;

	public ImmutableArray<SequencePoint> SequencePoints => ImmutableArray<SequencePoint>.Empty;

	public bool HasDynamicLocalVariables => false;

	public ImmutableArray<LocalScope> LocalScopes => ImmutableArray<LocalScope>.Empty;

	public IImportScope ImportScope => null;

	public DebugId MethodId => default(DebugId);

	public ImmutableArray<StateMachineHoistedLocalScope> StateMachineHoistedLocalScopes => default(ImmutableArray<StateMachineHoistedLocalScope>);

	public string StateMachineTypeName => null;

	public ImmutableArray<EncHoistedLocalInfo> StateMachineHoistedLocalSlots => default(ImmutableArray<EncHoistedLocalInfo>);

	public ImmutableArray<ITypeReference> StateMachineAwaiterSlots => default(ImmutableArray<ITypeReference>);

	public ImmutableArray<EncClosureInfo> ClosureDebugInfo => ImmutableArray<EncClosureInfo>.Empty;

	public ImmutableArray<EncLambdaInfo> LambdaDebugInfo => ImmutableArray<EncLambdaInfo>.Empty;

	public ImmutableArray<LambdaRuntimeRudeEditInfo> OrderedLambdaRuntimeRudeEdits => ImmutableArray<LambdaRuntimeRudeEditInfo>.Empty;

	public StateMachineStatesDebugInfo StateMachineStatesDebugInfo => default(StateMachineStatesDebugInfo);

	public ImmutableArray<SourceSpan> CodeCoverageSpans => ImmutableArray<SourceSpan>.Empty;

	public bool IsPrimaryConstructor => false;

	public MethodDefinitionBase(ITypeDefinition containingTypeDefinition, ushort maxStack, ImmutableArray<byte> il)
	{
		ContainingTypeDefinition = containingTypeDefinition;
		MaxStack = maxStack;
		IL = il;
	}

	public IMethodBody GetBody(EmitContext context)
	{
		return this;
	}

	public IDefinition AsDefinition(EmitContext context)
	{
		return this;
	}

	ISymbolInternal IReference.GetInternalSymbol()
	{
		return null;
	}

	public void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit((IMethodDefinition)this);
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		return ContainingTypeDefinition;
	}

	public MethodImplAttributes GetImplementationAttributes(EmitContext context)
	{
		return MethodImplAttributes.IL;
	}

	public ImmutableArray<IParameterTypeInformation> GetParameters(EmitContext context)
	{
		return Parameters.CastArray<IParameterTypeInformation>();
	}

	public IMethodDefinition GetResolvedMethod(EmitContext context)
	{
		return this;
	}

	public IEnumerable<ICustomAttribute> GetReturnValueAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public virtual ITypeReference GetType(EmitContext context)
	{
		return context.Module.GetPlatformType(PlatformType.SystemVoid, context);
	}

	public sealed override bool Equals(object obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/MethodDefinitionBase.cs", 176);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/MethodDefinitionBase.cs", 182);
	}
}
