using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis.Symbols;

internal interface IMethodSymbolInternal : ISymbolInternal
{
	bool IsIterator { get; }

	bool IsAsync { get; }

	bool IsGenericMethod { get; }

	bool ReturnsVoid { get; }

	int ParameterCount { get; }

	ImmutableArray<IParameterSymbolInternal> Parameters { get; }

	bool HasDeclarativeSecurity { get; }

	bool IsAccessCheckedOnOverride { get; }

	bool IsExternal { get; }

	bool IsHiddenBySignature { get; }

	bool IsMetadataNewSlot { get; }

	bool IsPlatformInvoke { get; }

	bool IsMetadataFinal { get; }

	bool HasSpecialName { get; }

	bool HasRuntimeSpecialName { get; }

	bool RequiresSecurityObject { get; }

	MethodImplAttributes ImplementationAttributes { get; }

	ISymbolInternal? AssociatedSymbol { get; }

	IMethodSymbolInternal? PartialImplementationPart { get; }

	IMethodSymbolInternal? PartialDefinitionPart { get; }

	BlobHandle MetadataSignatureHandle { get; }

	int CalculateLocalSyntaxOffset(int declaratorPosition, SyntaxTree declaratorTree);

	IMethodSymbolInternal Construct(params ITypeSymbolInternal[] typeArguments);
}
