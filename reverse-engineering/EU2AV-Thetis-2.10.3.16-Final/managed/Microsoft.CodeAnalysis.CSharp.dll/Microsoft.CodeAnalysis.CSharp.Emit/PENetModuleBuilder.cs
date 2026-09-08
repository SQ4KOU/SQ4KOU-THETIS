using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class PENetModuleBuilder : PEModuleBuilder
{
	public override EmitBaseline? PreviousGeneration => null;

	public override SymbolChanges? EncSymbolChanges => null;

	public override bool FieldRvaSupported => true;

	public override bool MethodImplSupported => true;

	public override ISourceAssemblySymbolInternal? SourceAssemblyOpt => null;

	internal PENetModuleBuilder(SourceModuleSymbol sourceModule, EmitOptions emitOptions, ModulePropertiesForSerialization serializationProperties, IEnumerable<ResourceDescription> manifestResources)
		: base(sourceModule, emitOptions, OutputKind.NetModule, serializationProperties, manifestResources)
	{
	}

	internal override SynthesizedAttributeData SynthesizeEmbeddedAttribute()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/PENetModuleBuilder.cs", 31);
	}

	protected override void AddEmbeddedResourcesFromAddedModules(ArrayBuilder<ManagedResource> builder, DiagnosticBag diagnostics)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/PENetModuleBuilder.cs", 36);
	}

	public override INamedTypeSymbolInternal? TryGetOrCreateSynthesizedHotReloadExceptionType()
	{
		return null;
	}

	public override IMethodSymbolInternal GetOrCreateHotReloadExceptionConstructorDefinition()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/PENetModuleBuilder.cs", 49);
	}

	public override INamedTypeSymbolInternal? GetUsedSynthesizedHotReloadExceptionType()
	{
		return null;
	}

	public override IEnumerable<IFileReference> GetFiles(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IFileReference>();
	}
}
