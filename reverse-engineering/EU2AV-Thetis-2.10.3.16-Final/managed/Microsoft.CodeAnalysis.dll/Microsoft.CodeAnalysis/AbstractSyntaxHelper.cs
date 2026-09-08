using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal abstract class AbstractSyntaxHelper : ISyntaxHelper
{
	public abstract bool IsCaseSensitive { get; }

	public abstract bool IsValidIdentifier(string name);

	public abstract string GetUnqualifiedIdentifierOfName(SyntaxNode name);

	public abstract bool IsAnyNamespaceBlock(SyntaxNode node);

	public abstract bool IsAttribute(SyntaxNode node);

	public abstract SyntaxNode GetNameOfAttribute(SyntaxNode node);

	public abstract SyntaxNode RemapAttributeTarget(SyntaxNode target);

	public abstract SyntaxNode GetAttributeOwningNode(SyntaxNode attribute);

	public abstract bool IsAttributeList(SyntaxNode node);

	public abstract SeparatedSyntaxList<SyntaxNode> GetAttributesOfAttributeList(SyntaxNode node);

	public abstract void AddAttributeTargets(SyntaxNode node, ArrayBuilder<SyntaxNode> targets);

	public abstract bool IsLambdaExpression(SyntaxNode node);

	public abstract void AddAliases(GreenNode node, ArrayBuilder<(string aliasName, string symbolName)> aliases, bool global);

	public abstract void AddAliases(CompilationOptions options, ArrayBuilder<(string aliasName, string symbolName)> aliases);

	public abstract bool ContainsGlobalAliases(SyntaxNode root);
}
