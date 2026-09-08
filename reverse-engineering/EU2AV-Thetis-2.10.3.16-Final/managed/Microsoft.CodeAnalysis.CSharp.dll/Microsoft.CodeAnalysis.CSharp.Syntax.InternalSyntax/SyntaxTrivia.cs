using System.IO;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class SyntaxTrivia : CSharpSyntaxNode
{
	public readonly string Text;

	public override bool IsTrivia => true;

	public override int Width => base.FullWidth;

	internal SyntaxTrivia(SyntaxKind kind, string text, DiagnosticInfo[]? diagnostics = null, SyntaxAnnotation[]? annotations = null)
		: base(kind, diagnostics, annotations, text.Length)
	{
		Text = text;
		if (kind == SyntaxKind.PreprocessingMessageTrivia)
		{
			SetFlags(NodeFlags.ContainsSkippedText);
		}
	}

	internal static SyntaxTrivia Create(SyntaxKind kind, string text)
	{
		return new SyntaxTrivia(kind, text);
	}

	public override string ToFullString()
	{
		return Text;
	}

	public override string ToString()
	{
		return Text;
	}

	internal override GreenNode GetSlot(int index)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Syntax/InternalSyntax/SyntaxTrivia.cs", 43);
	}

	public override int GetLeadingTriviaWidth()
	{
		return 0;
	}

	public override int GetTrailingTriviaWidth()
	{
		return 0;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
	{
		return new SyntaxTrivia(base.Kind, Text, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
	{
		return new SyntaxTrivia(base.Kind, Text, GetDiagnostics(), annotations);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTrivia(this);
	}

	protected override void WriteTriviaTo(TextWriter writer)
	{
		writer.Write(Text);
	}

	public static implicit operator Microsoft.CodeAnalysis.SyntaxTrivia(SyntaxTrivia trivia)
	{
		return new Microsoft.CodeAnalysis.SyntaxTrivia(default(Microsoft.CodeAnalysis.SyntaxToken), trivia, 0, 0);
	}

	public override bool IsEquivalentTo(GreenNode? other)
	{
		if (!base.IsEquivalentTo(other))
		{
			return false;
		}
		if (Text != ((SyntaxTrivia)other).Text)
		{
			return false;
		}
		return true;
	}

	internal override SyntaxNode CreateRed(SyntaxNode? parent, int position)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Syntax/InternalSyntax/SyntaxTrivia.cs", 112);
	}
}
