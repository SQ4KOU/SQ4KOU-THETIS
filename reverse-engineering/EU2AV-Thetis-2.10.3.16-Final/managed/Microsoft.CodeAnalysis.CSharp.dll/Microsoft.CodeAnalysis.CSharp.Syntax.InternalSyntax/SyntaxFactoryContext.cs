namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class SyntaxFactoryContext
{
	internal bool IsInAsync;

	internal bool ForceConditionalAccessExpression;

	internal bool IsInQuery;

	internal bool IsInFieldKeywordContext;
}
