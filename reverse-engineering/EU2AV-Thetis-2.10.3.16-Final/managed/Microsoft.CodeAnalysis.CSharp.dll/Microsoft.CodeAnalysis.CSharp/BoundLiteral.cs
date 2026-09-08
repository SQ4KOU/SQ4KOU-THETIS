using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLiteral : BoundExpression
{
	public override object Display
	{
		get
		{
			ConstantValue? constantValueOpt = ConstantValueOpt;
			if ((object)constantValueOpt == null || !constantValueOpt.IsNull)
			{
				return base.Display;
			}
			return MessageID.IDS_NULL.Localize();
		}
	}

	public override ConstantValue? ConstantValueOpt { get; }

	public BoundLiteral(SyntaxNode syntax, ConstantValue? constantValueOpt, TypeSymbol? type, bool hasErrors)
		: base(BoundKind.Literal, syntax, type, hasErrors)
	{
		ConstantValueOpt = constantValueOpt;
	}

	public BoundLiteral(SyntaxNode syntax, ConstantValue? constantValueOpt, TypeSymbol? type)
		: base(BoundKind.Literal, syntax, type)
	{
		ConstantValueOpt = constantValueOpt;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLiteral(this);
	}

	public BoundLiteral Update(ConstantValue? constantValueOpt, TypeSymbol? type)
	{
		if (constantValueOpt != ConstantValueOpt || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundLiteral boundLiteral = new BoundLiteral(Syntax, constantValueOpt, type, base.HasErrors);
			boundLiteral.CopyAttributes(this);
			return boundLiteral;
		}
		return this;
	}
}
