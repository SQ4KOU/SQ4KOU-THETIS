using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundStateMachineInstanceId : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public BoundStateMachineInstanceId(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.StateMachineInstanceId, syntax, type, hasErrors)
	{
	}

	public BoundStateMachineInstanceId(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.StateMachineInstanceId, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitStateMachineInstanceId(this);
	}

	public BoundStateMachineInstanceId Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundStateMachineInstanceId boundStateMachineInstanceId = new BoundStateMachineInstanceId(Syntax, type, base.HasErrors);
			boundStateMachineInstanceId.CopyAttributes(this);
			return boundStateMachineInstanceId;
		}
		return this;
	}
}
