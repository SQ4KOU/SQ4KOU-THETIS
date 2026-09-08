namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class InstrumentationState
{
	public bool IsSuppressed { get; set; }

	public Instrumenter Instrumenter { get; set; } = Microsoft.CodeAnalysis.CSharp.Instrumenter.NoOp;

	public void RemoveCodeCoverageInstrumenter()
	{
		Instrumenter = recurse(Instrumenter);
		static Instrumenter recurse(Instrumenter instrumenter)
		{
			if (instrumenter is CodeCoverageInstrumenter codeCoverageInstrumenter)
			{
				Instrumenter previous = codeCoverageInstrumenter.Previous;
				return recurse(previous);
			}
			if (instrumenter is CompoundInstrumenter compoundInstrumenter)
			{
				return compoundInstrumenter.WithPrevious(recurse(compoundInstrumenter.Previous));
			}
			return instrumenter;
		}
	}
}
