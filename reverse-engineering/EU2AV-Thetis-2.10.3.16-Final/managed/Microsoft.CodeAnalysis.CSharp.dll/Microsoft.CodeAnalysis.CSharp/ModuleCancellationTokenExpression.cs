using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ModuleCancellationTokenExpression : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public ModuleCancellationTokenExpression(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ModuleCancellationTokenExpression, syntax, type, hasErrors)
	{
	}

	public ModuleCancellationTokenExpression(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ModuleCancellationTokenExpression, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitModuleCancellationTokenExpression(this);
	}

	public ModuleCancellationTokenExpression Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			ModuleCancellationTokenExpression moduleCancellationTokenExpression = new ModuleCancellationTokenExpression(Syntax, type, base.HasErrors);
			moduleCancellationTokenExpression.CopyAttributes(this);
			return moduleCancellationTokenExpression;
		}
		return this;
	}
}
