using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal static class SyntaxListPoolExtensions
{
	public static Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> ToTokenListAndFree(this SyntaxListPool pool, SyntaxListBuilder builder)
	{
		GreenNode? node = builder.ToListNode();
		pool.Free(builder);
		return new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(node);
	}
}
