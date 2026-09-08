using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CSharpSyntaxHelper : AbstractSyntaxHelper
{
	public static readonly ISyntaxHelper Instance = new CSharpSyntaxHelper();

	public override bool IsCaseSensitive => true;

	private CSharpSyntaxHelper()
	{
	}

	public override bool IsValidIdentifier(string name)
	{
		return SyntaxFacts.IsValidIdentifier(name);
	}

	public override bool IsAnyNamespaceBlock(SyntaxNode node)
	{
		return node is Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax;
	}

	public override bool IsAttribute(SyntaxNode node)
	{
		return node is Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax;
	}

	public override SyntaxNode GetNameOfAttribute(SyntaxNode node)
	{
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax)node).Name;
	}

	public override SyntaxNode RemapAttributeTarget(SyntaxNode target)
	{
		if (target is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax variableDeclaratorSyntax)
		{
			CSharpSyntaxNode parent = variableDeclaratorSyntax.Parent;
			if (parent is Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclarationSyntax && parent.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.BaseFieldDeclarationSyntax result)
			{
				return result;
			}
		}
		return target;
	}

	public override SyntaxNode GetAttributeOwningNode(SyntaxNode attribute)
	{
		return attribute.Parent.Parent;
	}

	public override bool IsAttributeList(SyntaxNode node)
	{
		return node is Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax;
	}

	public override void AddAttributeTargets(SyntaxNode node, ArrayBuilder<SyntaxNode> targets)
	{
		CSharpSyntaxNode parent = ((Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax)node).Parent;
		if (parent is Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax fieldDeclarationSyntax)
		{
			targets.AddRange(fieldDeclarationSyntax.Declaration.Variables);
		}
		else if (parent is Microsoft.CodeAnalysis.CSharp.Syntax.EventFieldDeclarationSyntax eventFieldDeclarationSyntax)
		{
			targets.AddRange(eventFieldDeclarationSyntax.Declaration.Variables);
		}
		else
		{
			targets.Add(parent);
		}
	}

	public override SeparatedSyntaxList<SyntaxNode> GetAttributesOfAttributeList(SyntaxNode node)
	{
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax)node).Attributes;
	}

	public override bool IsLambdaExpression(SyntaxNode node)
	{
		return node is Microsoft.CodeAnalysis.CSharp.Syntax.LambdaExpressionSyntax;
	}

	public override string GetUnqualifiedIdentifierOfName(SyntaxNode node)
	{
		return ((Microsoft.CodeAnalysis.CSharp.Syntax.NameSyntax)node).GetUnqualifiedName().Identifier.ValueText;
	}

	public override void AddAliases(GreenNode node, ArrayBuilder<(string aliasName, string symbolName)> aliases, bool global)
	{
		if (node is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax compilationUnitSyntax)
		{
			AddAliases(compilationUnitSyntax.Usings, aliases, global);
			return;
		}
		if (node is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseNamespaceDeclarationSyntax baseNamespaceDeclarationSyntax)
		{
			AddAliases(baseNamespaceDeclarationSyntax.Usings, aliases, global);
			return;
		}
		throw ExceptionUtilities.UnexpectedValue(node.KindText);
	}

	private static void AddAliases(Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax> usings, ArrayBuilder<(string aliasName, string symbolName)> aliases, bool global)
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax item in usings)
		{
			if (item.Alias != null && global == (item.GlobalKeyword != null) && item.NamespaceOrType is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax name)
			{
				string valueText = item.Alias.Name.Identifier.ValueText;
				string valueText2 = GetUnqualifiedName(name).Identifier.ValueText;
				aliases.Add((valueText, valueText2));
			}
		}
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleNameSyntax GetUnqualifiedName(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameSyntax name)
	{
		if (!(name is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AliasQualifiedNameSyntax aliasQualifiedNameSyntax))
		{
			if (!(name is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.QualifiedNameSyntax qualifiedNameSyntax))
			{
				if (name is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleNameSyntax result)
				{
					return result;
				}
				throw ExceptionUtilities.UnexpectedValue(name.KindText);
			}
			return qualifiedNameSyntax.Right;
		}
		return aliasQualifiedNameSyntax.Name;
	}

	public override void AddAliases(CompilationOptions compilation, ArrayBuilder<(string aliasName, string symbolName)> aliases)
	{
	}

	public override bool ContainsGlobalAliases(SyntaxNode root)
	{
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax @using in ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CompilationUnitSyntax)root.Green).Usings)
		{
			if (@using.GlobalKeyword != null && @using.Alias != null)
			{
				return true;
			}
		}
		return false;
	}
}
