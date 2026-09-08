using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundInterpolatedStringArgumentPlaceholder : BoundValuePlaceholderBase
{
	public const int InstanceParameter = -1;

	public const int ExtensionReceiver = -2;

	public const int TrailingConstructorValidityParameter = -3;

	public const int UnspecifiedParameter = -4;

	public sealed override bool IsEquivalentToThisReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundInterpolatedStringArgumentPlaceholder.cs", 14);
		}
	}

	public new TypeSymbol Type => base.Type;

	public int ArgumentIndex { get; }

	public BoundInterpolatedStringArgumentPlaceholder(SyntaxNode syntax, int argumentIndex, TypeSymbol type, bool hasErrors)
		: base(BoundKind.InterpolatedStringArgumentPlaceholder, syntax, type, hasErrors)
	{
		ArgumentIndex = argumentIndex;
	}

	public BoundInterpolatedStringArgumentPlaceholder(SyntaxNode syntax, int argumentIndex, TypeSymbol type)
		: base(BoundKind.InterpolatedStringArgumentPlaceholder, syntax, type)
	{
		ArgumentIndex = argumentIndex;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitInterpolatedStringArgumentPlaceholder(this);
	}

	public BoundInterpolatedStringArgumentPlaceholder Update(int argumentIndex, TypeSymbol type)
	{
		if (argumentIndex != ArgumentIndex || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundInterpolatedStringArgumentPlaceholder boundInterpolatedStringArgumentPlaceholder = new BoundInterpolatedStringArgumentPlaceholder(Syntax, argumentIndex, type, base.HasErrors);
			boundInterpolatedStringArgumentPlaceholder.CopyAttributes(this);
			return boundInterpolatedStringArgumentPlaceholder;
		}
		return this;
	}
}
