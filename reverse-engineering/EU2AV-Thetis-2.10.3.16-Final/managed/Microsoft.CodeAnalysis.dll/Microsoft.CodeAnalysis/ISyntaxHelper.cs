using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal interface ISyntaxHelper
{
	bool IsCaseSensitive { get; }

	bool IsValidIdentifier(string name);

	bool IsAnyNamespaceBlock(SyntaxNode node);

	bool IsAttributeList(SyntaxNode node);

	SeparatedSyntaxList<SyntaxNode> GetAttributesOfAttributeList(SyntaxNode node);

	void AddAttributeTargets(SyntaxNode node, ArrayBuilder<SyntaxNode> targets);

	bool IsAttribute(SyntaxNode node);

	SyntaxNode GetNameOfAttribute(SyntaxNode node);

	SyntaxNode RemapAttributeTarget(SyntaxNode target);

	SyntaxNode GetAttributeOwningNode(SyntaxNode attribute);

	bool IsLambdaExpression(SyntaxNode node);

	string GetUnqualifiedIdentifierOfName(SyntaxNode node);

	void AddAliases(GreenNode node, ArrayBuilder<(string aliasName, string symbolName)> aliases, bool global);

	void AddAliases(CompilationOptions options, ArrayBuilder<(string aliasName, string symbolName)> aliases);

	bool ContainsGlobalAliases(SyntaxNode root);
}
