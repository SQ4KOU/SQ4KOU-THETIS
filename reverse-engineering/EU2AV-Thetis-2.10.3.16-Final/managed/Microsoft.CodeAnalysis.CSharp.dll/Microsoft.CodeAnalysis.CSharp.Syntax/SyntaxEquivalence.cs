using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxEquivalence
{
	private static readonly ObjectPool<Stack<(GreenNode? before, GreenNode? after)>> s_equivalenceCheckStack = new ObjectPool<Stack<(GreenNode, GreenNode)>>(() => new Stack<(GreenNode, GreenNode)>());

	internal static bool AreEquivalent(SyntaxTree? before, SyntaxTree? after, Func<SyntaxKind, bool>? ignoreChildNode, bool topLevel)
	{
		if (before == after)
		{
			return true;
		}
		if (before == null || after == null)
		{
			return false;
		}
		return AreEquivalent(before.GetRoot(), after.GetRoot(), ignoreChildNode, topLevel);
	}

	public static bool AreEquivalent(SyntaxNode? before, SyntaxNode? after, Func<SyntaxKind, bool>? ignoreChildNode, bool topLevel)
	{
		if (before == null || after == null)
		{
			return before == after;
		}
		return AreEquivalentRecursive(before.Green, after.Green, ignoreChildNode, topLevel);
	}

	public static bool AreEquivalent(SyntaxTokenList before, SyntaxTokenList after)
	{
		return AreEquivalentRecursive(before.Node, after.Node, null, topLevel: false);
	}

	public static bool AreEquivalent(SyntaxToken before, SyntaxToken after)
	{
		if (before.RawKind == after.RawKind)
		{
			if (before.Node != null)
			{
				return AreTokensEquivalent(before.Node, after.Node, null);
			}
			return true;
		}
		return false;
	}

	private static bool AreTokensEquivalent(GreenNode? before, GreenNode? after, Func<SyntaxKind, bool>? ignoreChildNode)
	{
		if (before == null || after == null)
		{
			if (before == null)
			{
				return after == null;
			}
			return false;
		}
		if (before.IsMissing != after.IsMissing)
		{
			return false;
		}
		switch ((SyntaxKind)(ushort)before.RawKind)
		{
		case SyntaxKind.IdentifierToken:
			if (((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)before).ValueText != ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)after).ValueText)
			{
				return false;
			}
			break;
		case SyntaxKind.NumericLiteralToken:
		case SyntaxKind.CharacterLiteralToken:
		case SyntaxKind.StringLiteralToken:
		case SyntaxKind.InterpolatedStringTextToken:
		case SyntaxKind.SingleLineRawStringLiteralToken:
		case SyntaxKind.MultiLineRawStringLiteralToken:
		case SyntaxKind.Utf8StringLiteralToken:
		case SyntaxKind.Utf8SingleLineRawStringLiteralToken:
		case SyntaxKind.Utf8MultiLineRawStringLiteralToken:
			if (((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)before).Text != ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)after).Text)
			{
				return false;
			}
			break;
		}
		return AreNullableDirectivesEquivalent(before, after, ignoreChildNode);
	}

	private static bool AreEquivalentRecursive(GreenNode? before, GreenNode? after, Func<SyntaxKind, bool>? ignoreChildNode, bool topLevel)
	{
		Stack<(GreenNode? before, GreenNode? after)> stack = s_equivalenceCheckStack.Allocate();
		stack.Push((before, after));
		try
		{
			while (stack.Count > 0)
			{
				(GreenNode, GreenNode) tuple = stack.Pop();
				if (!areEquivalentSingleLevel(tuple.Item1, tuple.Item2))
				{
					return false;
				}
			}
			return true;
		}
		finally
		{
			stack.Clear();
			s_equivalenceCheckStack.Free(stack);
		}
		bool areEquivalentSingleLevel(GreenNode? greenNode, GreenNode? greenNode2)
		{
			if (greenNode == greenNode2)
			{
				return true;
			}
			if (greenNode == null || greenNode2 == null)
			{
				return false;
			}
			if (greenNode.RawKind != greenNode2.RawKind)
			{
				return false;
			}
			if (greenNode.IsToken)
			{
				return AreTokensEquivalent(greenNode, greenNode2, ignoreChildNode);
			}
			if (topLevel)
			{
				SyntaxKind syntaxKind = (SyntaxKind)greenNode.RawKind;
				if (syntaxKind == SyntaxKind.Block || syntaxKind == SyntaxKind.ArrowExpressionClause)
				{
					return AreNullableDirectivesEquivalent(greenNode, greenNode2, ignoreChildNode);
				}
				if ((ushort)greenNode.RawKind == 8873)
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax obj = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax)greenNode;
					Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax fieldDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax)greenNode2;
					bool num = obj.Modifiers.Any(8350);
					bool flag = fieldDeclarationSyntax.Modifiers.Any(8350);
					if (!num && !flag)
					{
						ignoreChildNode = (SyntaxKind childKind) => childKind == SyntaxKind.EqualsValueClause;
					}
				}
			}
			if (ignoreChildNode != null)
			{
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator enumerator = greenNode.ChildNodesAndTokens().GetEnumerator();
				Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator enumerator2 = greenNode2.ChildNodesAndTokens().GetEnumerator();
				GreenNode greenNode3;
				GreenNode greenNode4;
				while (true)
				{
					greenNode3 = null;
					greenNode4 = null;
					while (enumerator.MoveNext())
					{
						GreenNode current = enumerator.Current;
						if (current != null && (current.IsToken || !ignoreChildNode((SyntaxKind)current.RawKind)))
						{
							greenNode3 = current;
							break;
						}
					}
					while (enumerator2.MoveNext())
					{
						GreenNode current2 = enumerator2.Current;
						if (current2 != null && (current2.IsToken || !ignoreChildNode((SyntaxKind)current2.RawKind)))
						{
							greenNode4 = current2;
							break;
						}
					}
					if (greenNode3 == null || greenNode4 == null)
					{
						break;
					}
					stack.Push((greenNode3, greenNode4));
				}
				return greenNode3 == greenNode4;
			}
			int slotCount = greenNode.SlotCount;
			if (slotCount != greenNode2.SlotCount)
			{
				return false;
			}
			for (int num2 = slotCount - 1; num2 >= 0; num2--)
			{
				GreenNode slot = greenNode.GetSlot(num2);
				GreenNode slot2 = greenNode2.GetSlot(num2);
				stack.Push((slot, slot2));
			}
			return true;
		}
	}

	private static bool AreNullableDirectivesEquivalent(GreenNode before, GreenNode after, Func<SyntaxKind, bool>? ignoreChildNode)
	{
		if (ignoreChildNode != null && ignoreChildNode(SyntaxKind.NullableDirectiveTrivia))
		{
			return true;
		}
		using (IEnumerator<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax> enumerator = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)before).GetDirectives().GetEnumerator())
		{
			using IEnumerator<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax> enumerator2 = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode)after).GetDirectives().GetEnumerator();
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax directiveTriviaSyntax;
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax directiveTriviaSyntax2;
			do
			{
				directiveTriviaSyntax = getNextNullableDirective(enumerator);
				directiveTriviaSyntax2 = getNextNullableDirective(enumerator2);
				if (directiveTriviaSyntax == null || directiveTriviaSyntax2 == null)
				{
					return directiveTriviaSyntax == directiveTriviaSyntax2;
				}
			}
			while (AreEquivalentRecursive(directiveTriviaSyntax, directiveTriviaSyntax2, ignoreChildNode, topLevel: false));
			return false;
		}
		static Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax? getNextNullableDirective(IEnumerator<Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax> enumerator3)
		{
			while (enumerator3.MoveNext())
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DirectiveTriviaSyntax current = enumerator3.Current;
				if (current.Kind == SyntaxKind.NullableDirectiveTrivia)
				{
					return current;
				}
			}
			return null;
		}
	}
}
