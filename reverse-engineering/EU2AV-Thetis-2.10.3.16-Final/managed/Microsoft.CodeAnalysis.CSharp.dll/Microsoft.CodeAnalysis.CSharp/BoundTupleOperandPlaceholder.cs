using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTupleOperandPlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundExpression.cs", 177);
		}
	}

	public new TypeSymbol Type => base.Type;

	public BoundTupleOperandPlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.TupleOperandPlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundTupleOperandPlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.TupleOperandPlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTupleOperandPlaceholder(this);
	}

	public BoundTupleOperandPlaceholder Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundTupleOperandPlaceholder boundTupleOperandPlaceholder = new BoundTupleOperandPlaceholder(Syntax, type, base.HasErrors);
			boundTupleOperandPlaceholder.CopyAttributes(this);
			return boundTupleOperandPlaceholder;
		}
		return this;
	}
}
