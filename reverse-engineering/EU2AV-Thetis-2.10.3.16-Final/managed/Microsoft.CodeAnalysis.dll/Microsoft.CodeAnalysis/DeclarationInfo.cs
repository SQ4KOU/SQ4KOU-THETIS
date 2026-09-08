using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal readonly struct DeclarationInfo
{
	public SyntaxNode DeclaredNode { get; }

	public ImmutableArray<SyntaxNode> ExecutableCodeBlocks { get; }

	public ISymbol? DeclaredSymbol { get; }

	internal DeclarationInfo(SyntaxNode declaredNode, ImmutableArray<SyntaxNode> executableCodeBlocks, ISymbol? declaredSymbol)
	{
		DeclaredNode = declaredNode;
		ExecutableCodeBlocks = executableCodeBlocks;
		DeclaredSymbol = declaredSymbol;
	}
}
