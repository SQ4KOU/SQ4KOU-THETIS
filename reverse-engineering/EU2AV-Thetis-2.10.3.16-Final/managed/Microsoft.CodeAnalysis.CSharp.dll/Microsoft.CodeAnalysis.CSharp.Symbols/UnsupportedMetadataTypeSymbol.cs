using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class UnsupportedMetadataTypeSymbol : ErrorTypeSymbol
{
	private readonly BadImageFormatException? _mrEx;

	internal override DiagnosticInfo ErrorInfo => new CSDiagnosticInfo(ErrorCode.ERR_BogusType, string.Empty);

	internal override bool MangleName => false;

	internal override bool IsFileLocal => false;

	internal override FileIdentifier? AssociatedFileIdentifier => null;

	internal UnsupportedMetadataTypeSymbol(BadImageFormatException? mrEx = null)
	{
		_mrEx = mrEx;
	}

	protected override NamedTypeSymbol WithTupleDataCore(TupleExtraData newData)
	{
		return this;
	}
}
