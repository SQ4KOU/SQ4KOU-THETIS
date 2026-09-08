using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class RewrittenParameterSymbol : WrappedParameterSymbol
{
	internal sealed override bool IsCallerLineNumber => _underlyingParameter.IsCallerLineNumber;

	internal sealed override bool IsCallerFilePath => _underlyingParameter.IsCallerFilePath;

	internal sealed override bool IsCallerMemberName => _underlyingParameter.IsCallerMemberName;

	internal sealed override int CallerArgumentExpressionParameterIndex => _underlyingParameter.CallerArgumentExpressionParameterIndex;

	internal override ImmutableArray<int> InterpolatedStringHandlerArgumentIndexes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/RewrittenParameterSymbol.cs", 24);
		}
	}

	internal override bool HasInterpolatedStringHandlerArgumentError
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Symbols/Extensions/RewrittenParameterSymbol.cs", 26);
		}
	}

	public RewrittenParameterSymbol(ParameterSymbol originalParameter)
		: base(originalParameter)
	{
	}
}
