using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUtf8String : BoundExpression
{
	public new TypeSymbol Type => base.Type;

	public string Value { get; }

	public BoundUtf8String(SyntaxNode syntax, string value, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Utf8String, syntax, type, hasErrors)
	{
		Value = value;
	}

	public BoundUtf8String(SyntaxNode syntax, string value, TypeSymbol type)
		: base(BoundKind.Utf8String, syntax, type)
	{
		Value = value;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUtf8String(this);
	}

	public BoundUtf8String Update(string value, TypeSymbol type)
	{
		if (value != Value || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundUtf8String boundUtf8String = new BoundUtf8String(Syntax, value, type, base.HasErrors);
			boundUtf8String.CopyAttributes(this);
			return boundUtf8String;
		}
		return this;
	}
}
