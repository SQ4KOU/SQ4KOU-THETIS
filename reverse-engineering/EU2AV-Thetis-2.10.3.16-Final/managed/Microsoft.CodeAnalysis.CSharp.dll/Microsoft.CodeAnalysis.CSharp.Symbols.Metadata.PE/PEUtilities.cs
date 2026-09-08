using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal static class PEUtilities
{
	internal static DiagnosticInfo? DeriveCompilerFeatureRequiredAttributeDiagnostic(Symbol symbol, PEModuleSymbol module, EntityHandle handle, CompilerFeatureRequiredFeatures allowedFeatures, MetadataDecoder decoder)
	{
		string firstUnsupportedCompilerFeatureFromToken = module.Module.GetFirstUnsupportedCompilerFeatureFromToken(handle, decoder, allowedFeatures);
		if (firstUnsupportedCompilerFeatureFromToken == null)
		{
			return null;
		}
		return new CSDiagnosticInfo(ErrorCode.ERR_UnsupportedCompilerFeature, symbol, firstUnsupportedCompilerFeatureFromToken);
	}
}
