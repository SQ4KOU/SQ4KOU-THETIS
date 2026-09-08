using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundFunctionPointerLoad : BoundExpression
{
	public MethodSymbol TargetMethod { get; }

	public TypeSymbol? ConstrainedToTypeOpt { get; }

	public new TypeSymbol Type => base.Type;

	public BoundFunctionPointerLoad(SyntaxNode syntax, MethodSymbol targetMethod, TypeSymbol? constrainedToTypeOpt, TypeSymbol type, bool hasErrors)
		: base(BoundKind.FunctionPointerLoad, syntax, type, hasErrors)
	{
		TargetMethod = targetMethod;
		ConstrainedToTypeOpt = constrainedToTypeOpt;
	}

	public BoundFunctionPointerLoad(SyntaxNode syntax, MethodSymbol targetMethod, TypeSymbol? constrainedToTypeOpt, TypeSymbol type)
		: base(BoundKind.FunctionPointerLoad, syntax, type)
	{
		TargetMethod = targetMethod;
		ConstrainedToTypeOpt = constrainedToTypeOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitFunctionPointerLoad(this);
	}

	public BoundFunctionPointerLoad Update(MethodSymbol targetMethod, TypeSymbol? constrainedToTypeOpt, TypeSymbol type)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(targetMethod, TargetMethod) || !TypeSymbol.Equals(constrainedToTypeOpt, ConstrainedToTypeOpt, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundFunctionPointerLoad boundFunctionPointerLoad = new BoundFunctionPointerLoad(Syntax, targetMethod, constrainedToTypeOpt, type, base.HasErrors);
			boundFunctionPointerLoad.CopyAttributes(this);
			return boundFunctionPointerLoad;
		}
		return this;
	}
}
