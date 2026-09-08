using System;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp;

public sealed class SyntaxTokenParser : IDisposable
{
	public readonly struct Result
	{
		internal readonly DirectiveStack ContextStartDirectiveStack;

		public SyntaxToken Token { get; }

		public SyntaxKind ContextualKind
		{
			get
			{
				SyntaxKind syntaxKind = Token.ContextualKind();
				if (syntaxKind != Token.Kind())
				{
					return syntaxKind;
				}
				return SyntaxKind.None;
			}
		}

		internal Result(SyntaxToken token, DirectiveStack contextStartDirectiveStack)
		{
			Token = token;
			ContextStartDirectiveStack = contextStartDirectiveStack;
		}
	}

	private Lexer _lexer;

	internal SyntaxTokenParser(Lexer lexer)
	{
		_lexer = lexer;
	}

	public void Dispose()
	{
		Interlocked.CompareExchange(ref _lexer, null, _lexer)?.Dispose();
	}

	public Result ParseNextToken()
	{
		DirectiveStack directives = _lexer.Directives;
		int position = _lexer.TextWindow.Position;
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token = _lexer.Lex(LexerMode.Syntax);
		return new Result(new SyntaxToken(null, token, position, 0), directives);
	}

	public Result ParseLeadingTrivia()
	{
		DirectiveStack directives = _lexer.Directives;
		int position = _lexer.TextWindow.Position;
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MissingToken(_lexer.LexSyntaxLeadingTrivia().Node, SyntaxKind.None, null);
		return new Result(new SyntaxToken(null, token, position, 0), directives);
	}

	public Result ParseTrailingTrivia()
	{
		DirectiveStack directives = _lexer.Directives;
		int position = _lexer.TextWindow.Position;
		Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxFactory.MissingToken(null, SyntaxKind.None, _lexer.LexSyntaxTrailingTrivia().Node);
		return new Result(new SyntaxToken(null, token, position, 0), directives);
	}

	public void SkipForwardTo(int position)
	{
		if (position < _lexer.TextWindow.Position)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		_lexer.TextWindow.Reset(position);
	}

	public void ResetTo(Result result)
	{
		_lexer.Reset(result.Token.Position, result.ContextStartDirectiveStack);
	}
}
