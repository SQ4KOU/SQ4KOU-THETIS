using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class CSharpSyntaxNode : GreenNode
{
	private static readonly ConditionalWeakTable<SyntaxNode, SmallDictionary<Microsoft.CodeAnalysis.SyntaxTrivia, SyntaxNode>> s_structuresTable = new ConditionalWeakTable<SyntaxNode, SmallDictionary<Microsoft.CodeAnalysis.SyntaxTrivia, SyntaxNode>>();

	public override string Language => "C#";

	public SyntaxKind Kind => (SyntaxKind)base.RawKind;

	public override string KindText => Kind.ToString();

	public override int RawContextualKind => base.RawKind;

	public override bool IsSkippedTokensTrivia => Kind == SyntaxKind.SkippedTokensTrivia;

	public override bool IsDocumentationCommentTrivia => SyntaxFacts.IsDocumentationCommentTrivia(Kind);

	internal CSharpSyntaxNode(SyntaxKind kind)
		: base((ushort)kind)
	{
		GreenStats.NoteGreen(this);
	}

	internal CSharpSyntaxNode(SyntaxKind kind, int fullWidth)
		: base((ushort)kind, fullWidth)
	{
		GreenStats.NoteGreen(this);
	}

	internal CSharpSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics)
		: base((ushort)kind, diagnostics)
	{
		GreenStats.NoteGreen(this);
	}

	internal CSharpSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, int fullWidth)
		: base((ushort)kind, diagnostics, fullWidth)
	{
		GreenStats.NoteGreen(this);
	}

	internal CSharpSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base((ushort)kind, diagnostics, annotations)
	{
		GreenStats.NoteGreen(this);
	}

	internal CSharpSyntaxNode(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations, int fullWidth)
		: base((ushort)kind, diagnostics, annotations, fullWidth)
	{
		GreenStats.NoteGreen(this);
	}

	public override int GetSlotOffset(int index)
	{
		int num = 0;
		for (int i = 0; i < index; i++)
		{
			GreenNode slot = GetSlot(i);
			if (slot != null)
			{
				num += slot.FullWidth;
			}
		}
		return num;
	}

	public SyntaxToken GetFirstToken()
	{
		return (SyntaxToken)GetFirstTerminal();
	}

	public SyntaxToken GetLastToken()
	{
		return (SyntaxToken)GetLastTerminal();
	}

	public SyntaxToken GetLastNonmissingToken()
	{
		return (SyntaxToken)GetLastNonmissingTerminal();
	}

	public virtual GreenNode GetLeadingTrivia()
	{
		return null;
	}

	public override GreenNode GetLeadingTriviaCore()
	{
		return GetLeadingTrivia();
	}

	public virtual GreenNode GetTrailingTrivia()
	{
		return null;
	}

	public override GreenNode GetTrailingTriviaCore()
	{
		return GetTrailingTrivia();
	}

	public abstract TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor);

	public abstract void Accept(CSharpSyntaxVisitor visitor);

	internal virtual DirectiveStack ApplyDirectives(DirectiveStack stack)
	{
		return ApplyDirectives(this, stack);
	}

	internal static DirectiveStack ApplyDirectives(GreenNode node, DirectiveStack stack)
	{
		if (node.ContainsDirectives)
		{
			int i = 0;
			for (int slotCount = node.SlotCount; i < slotCount; i++)
			{
				GreenNode slot = node.GetSlot(i);
				if (slot != null)
				{
					stack = ApplyDirectivesToListOrNode(slot, stack);
				}
			}
		}
		return stack;
	}

	internal static DirectiveStack ApplyDirectivesToListOrNode(GreenNode listOrNode, DirectiveStack stack)
	{
		if (listOrNode.RawKind == 1)
		{
			return ApplyDirectives(listOrNode, stack);
		}
		return ((CSharpSyntaxNode)listOrNode).ApplyDirectives(stack);
	}

	internal virtual IList<DirectiveTriviaSyntax> GetDirectives()
	{
		if (base.ContainsDirectives)
		{
			List<DirectiveTriviaSyntax> list = new List<DirectiveTriviaSyntax>(32);
			GetDirectives(this, list);
			return list;
		}
		return SpecializedCollections.EmptyList<DirectiveTriviaSyntax>();
	}

	private static void GetDirectives(GreenNode node, List<DirectiveTriviaSyntax> directives)
	{
		if (node == null || !node.ContainsDirectives)
		{
			return;
		}
		if (node is DirectiveTriviaSyntax item)
		{
			directives.Add(item);
			return;
		}
		if (node is SyntaxToken syntaxToken)
		{
			GetDirectives(syntaxToken.GetLeadingTrivia(), directives);
			GetDirectives(syntaxToken.GetTrailingTrivia(), directives);
			return;
		}
		int i = 0;
		for (int slotCount = node.SlotCount; i < slotCount; i++)
		{
			GetDirectives(node.GetSlot(i), directives);
		}
	}

	protected void SetFactoryContext(SyntaxFactoryContext context)
	{
		if (context.IsInAsync)
		{
			SetFlags(NodeFlags.FactoryContextIsInAsync);
		}
		if (context.IsInQuery)
		{
			SetFlags(NodeFlags.FactoryContextIsInQuery);
		}
		if (context.IsInFieldKeywordContext)
		{
			SetFlags(NodeFlags.FactoryContextIsInFieldKeywordContext);
		}
	}

	public sealed override Microsoft.CodeAnalysis.SyntaxToken CreateSeparator(SyntaxNode element)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(SyntaxKind.CommaToken);
	}

	public override bool IsTriviaWithEndOfLine()
	{
		if (Kind != SyntaxKind.EndOfLineTrivia)
		{
			return Kind == SyntaxKind.SingleLineCommentTrivia;
		}
		return true;
	}

	public override SyntaxNode GetStructure(Microsoft.CodeAnalysis.SyntaxTrivia trivia)
	{
		if (!trivia.HasStructure)
		{
			return null;
		}
		SyntaxNode parent = trivia.Token.Parent;
		if (parent == null)
		{
			return Microsoft.CodeAnalysis.CSharp.Syntax.StructuredTriviaSyntax.Create(trivia);
		}
		SmallDictionary<Microsoft.CodeAnalysis.SyntaxTrivia, SyntaxNode> orCreateValue = s_structuresTable.GetOrCreateValue(parent);
		SyntaxNode value;
		lock (orCreateValue)
		{
			if (!orCreateValue.TryGetValue(trivia, out value))
			{
				value = Microsoft.CodeAnalysis.CSharp.Syntax.StructuredTriviaSyntax.Create(trivia);
				orCreateValue.Add(trivia, value);
			}
		}
		return value;
	}
}
