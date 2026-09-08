using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundListPattern : BoundObjectPattern
{
	public ImmutableArray<BoundPattern> Subpatterns { get; }

	public bool HasSlice { get; }

	public BoundExpression? LengthAccess { get; }

	public BoundExpression? IndexerAccess { get; }

	public BoundListPatternReceiverPlaceholder? ReceiverPlaceholder { get; }

	public BoundListPatternIndexPlaceholder? ArgumentPlaceholder { get; }

	internal BoundListPattern WithSubpatterns(ImmutableArray<BoundPattern> subpatterns)
	{
		return Update(subpatterns, HasSlice, LengthAccess, IndexerAccess, ReceiverPlaceholder, ArgumentPlaceholder, base.Variable, base.VariableAccess, base.InputType, base.NarrowedType);
	}

	public BoundListPattern(SyntaxNode syntax, ImmutableArray<BoundPattern> subpatterns, bool hasSlice, BoundExpression? lengthAccess, BoundExpression? indexerAccess, BoundListPatternReceiverPlaceholder? receiverPlaceholder, BoundListPatternIndexPlaceholder? argumentPlaceholder, Symbol? variable, BoundExpression? variableAccess, TypeSymbol inputType, TypeSymbol narrowedType, bool hasErrors = false)
		: base(BoundKind.ListPattern, syntax, variable, variableAccess, inputType, narrowedType, hasErrors || subpatterns.HasErrors() || lengthAccess.HasErrors() || indexerAccess.HasErrors() || receiverPlaceholder.HasErrors() || argumentPlaceholder.HasErrors() || variableAccess.HasErrors())
	{
		Subpatterns = subpatterns;
		HasSlice = hasSlice;
		LengthAccess = lengthAccess;
		IndexerAccess = indexerAccess;
		ReceiverPlaceholder = receiverPlaceholder;
		ArgumentPlaceholder = argumentPlaceholder;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitListPattern(this);
	}

	public BoundListPattern Update(ImmutableArray<BoundPattern> subpatterns, bool hasSlice, BoundExpression? lengthAccess, BoundExpression? indexerAccess, BoundListPatternReceiverPlaceholder? receiverPlaceholder, BoundListPatternIndexPlaceholder? argumentPlaceholder, Symbol? variable, BoundExpression? variableAccess, TypeSymbol inputType, TypeSymbol narrowedType)
	{
		if (subpatterns != Subpatterns || hasSlice != HasSlice || lengthAccess != LengthAccess || indexerAccess != IndexerAccess || receiverPlaceholder != ReceiverPlaceholder || argumentPlaceholder != ArgumentPlaceholder || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(variable, base.Variable) || variableAccess != base.VariableAccess || !TypeSymbol.Equals(inputType, base.InputType, TypeCompareKind.ConsiderEverything) || !TypeSymbol.Equals(narrowedType, base.NarrowedType, TypeCompareKind.ConsiderEverything))
		{
			BoundListPattern boundListPattern = new BoundListPattern(Syntax, subpatterns, hasSlice, lengthAccess, indexerAccess, receiverPlaceholder, argumentPlaceholder, variable, variableAccess, inputType, narrowedType, base.HasErrors);
			boundListPattern.CopyAttributes(this);
			return boundListPattern;
		}
		return this;
	}
}
