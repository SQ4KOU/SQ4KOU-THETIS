using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal class DeclarationComputer
{
	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, ArrayBuilder<SyntaxNode>? executableCodeBlocks, CancellationToken cancellationToken)
	{
		ISymbol declaredSymbol = GetDeclaredSymbol(model, node, getSymbol, cancellationToken);
		return GetDeclarationInfo(node, declaredSymbol, executableCodeBlocks);
	}

	internal static DeclarationInfo GetDeclarationInfo(SyntaxNode node, ISymbol? declaredSymbol, ArrayBuilder<SyntaxNode>? executableCodeBlocks)
	{
		ImmutableArray<SyntaxNode> executableCodeBlocks2 = ImmutableArray<SyntaxNode>.Empty;
		if (executableCodeBlocks != null)
		{
			executableCodeBlocks.RemoveAll((SyntaxNode c) => c == null);
			executableCodeBlocks2 = executableCodeBlocks.ToImmutable();
		}
		return new DeclarationInfo(node, executableCodeBlocks2, declaredSymbol);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken)
	{
		return GetDeclarationInfo(model, node, getSymbol, (ArrayBuilder<SyntaxNode>?)null, cancellationToken);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, SyntaxNode executableCodeBlock, CancellationToken cancellationToken)
	{
		ArrayBuilder<SyntaxNode> instance = ArrayBuilder<SyntaxNode>.GetInstance();
		instance.Add(executableCodeBlock);
		DeclarationInfo declarationInfo = GetDeclarationInfo(model, node, getSymbol, instance, cancellationToken);
		instance.Free();
		return declarationInfo;
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken, params SyntaxNode[] executableCodeBlocks)
	{
		ArrayBuilder<SyntaxNode> instance = ArrayBuilder<SyntaxNode>.GetInstance();
		instance.AddRange(executableCodeBlocks);
		DeclarationInfo declarationInfo = GetDeclarationInfo(model, node, getSymbol, instance, cancellationToken);
		instance.Free();
		return declarationInfo;
	}

	private static ISymbol? GetDeclaredSymbol(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken)
	{
		if (!getSymbol)
		{
			return null;
		}
		ISymbol symbol = model.GetDeclaredSymbol(node, cancellationToken);
		if (symbol is INamespaceSymbol namespaceSymbol && namespaceSymbol.ConstituentNamespaces.Length > 1)
		{
			IAssemblySymbol assembly = model.Compilation.Assembly;
			INamespaceSymbol namespaceSymbol2 = namespaceSymbol.ConstituentNamespaces.FirstOrDefault((INamespaceSymbol ns, IAssemblySymbol assemblyToScope) => ns.ContainingAssembly == assemblyToScope, assembly);
			if (namespaceSymbol2 != null)
			{
				symbol = namespaceSymbol2;
			}
		}
		return symbol;
	}
}
