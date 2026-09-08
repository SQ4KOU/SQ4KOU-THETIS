using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class StateMachineTypeSymbol : SynthesizedContainer, ISynthesizedMethodBodyImplementationSymbol, ISymbolInternal
{
	private ImmutableArray<CSharpAttributeData> _attributes;

	public readonly MethodSymbol KickoffMethod;

	public override Symbol ContainingSymbol => KickoffMethod.ContainingType;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	IMethodSymbolInternal ISynthesizedMethodBodyImplementationSymbol.Method => KickoffMethod;

	public sealed override bool AreLocalsZeroed => KickoffMethod.AreLocalsZeroed;

	internal override bool HasCodeAnalysisEmbeddedAttribute => false;

	internal override bool HasCompilerLoweringPreserveAttribute => false;

	public StateMachineTypeSymbol(VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, MethodSymbol kickoffMethod, int kickoffMethodOrdinal)
		: base(MakeName(slotAllocatorOpt, compilationState, kickoffMethod, kickoffMethodOrdinal), TypeMap.ConcatMethodTypeParameters(kickoffMethod, null))
	{
		KickoffMethod = kickoffMethod;
	}

	private static string MakeName(VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, MethodSymbol kickoffMethod, int kickoffMethodOrdinal)
	{
		return slotAllocatorOpt?.PreviousStateMachineTypeName ?? GeneratedNames.MakeStateMachineTypeName(kickoffMethod.Name, kickoffMethodOrdinal, compilationState.ModuleBuilderOpt.CurrentGenerationOrdinal);
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_attributes.IsDefault)
		{
			ArrayBuilder<CSharpAttributeData> arrayBuilder = null;
			foreach (CSharpAttributeData attribute in KickoffMethod.ContainingType.GetAttributes())
			{
				if (attribute.IsTargetAttribute(AttributeDescription.DebuggerNonUserCodeAttribute) || attribute.IsTargetAttribute(AttributeDescription.DebuggerStepThroughAttribute))
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<CSharpAttributeData>.GetInstance(2);
					}
					arrayBuilder.Add(attribute);
				}
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _attributes, arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<CSharpAttributeData>.Empty, default(ImmutableArray<CSharpAttributeData>));
		}
		return _attributes;
	}
}
