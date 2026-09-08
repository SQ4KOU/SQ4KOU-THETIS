namespace Microsoft.CodeAnalysis.CSharp;

public class CSharpDiagnosticFormatter : DiagnosticFormatter
{
	public new static CSharpDiagnosticFormatter Instance { get; } = new CSharpDiagnosticFormatter();

	internal CSharpDiagnosticFormatter()
	{
	}

	internal override bool HasDefaultHelpLinkUri(Diagnostic diagnostic)
	{
		return diagnostic.Descriptor.HelpLinkUri == ErrorFacts.GetHelpLink((ErrorCode)diagnostic.Code);
	}
}
