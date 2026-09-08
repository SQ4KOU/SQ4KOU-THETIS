using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class PEAssemblyBuilder : PEAssemblyBuilderBase
{
	public override EmitBaseline? PreviousGeneration => null;

	public override SymbolChanges? EncSymbolChanges => null;

	public override bool FieldRvaSupported => true;

	public override bool MethodImplSupported => true;

	public PEAssemblyBuilder(SourceAssemblySymbol sourceAssembly, EmitOptions emitOptions, OutputKind outputKind, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources)
		: base(sourceAssembly, emitOptions, outputKind, serializationProperties, manifestResources, ImmutableArray<NamedTypeSymbol>.Empty)
	{
	}

	public override INamedTypeSymbolInternal? TryGetOrCreateSynthesizedHotReloadExceptionType()
	{
		return null;
	}

	public override IMethodSymbolInternal GetOrCreateHotReloadExceptionConstructorDefinition()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/PEAssemblyBuilder.cs", 702);
	}

	public override INamedTypeSymbolInternal? GetUsedSynthesizedHotReloadExceptionType()
	{
		return null;
	}
}
