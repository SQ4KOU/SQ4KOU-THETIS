using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LazyObsoleteDiagnosticInfo : LazyDiagnosticInfo
{
	private readonly object _symbolOrSymbolWithAnnotations;

	private readonly Symbol _containingSymbol;

	private readonly BinderFlags _binderFlags;

	internal LazyObsoleteDiagnosticInfo(object symbol, Symbol containingSymbol, BinderFlags binderFlags)
	{
		_symbolOrSymbolWithAnnotations = symbol;
		_containingSymbol = containingSymbol;
		_binderFlags = binderFlags;
	}

	private LazyObsoleteDiagnosticInfo(LazyObsoleteDiagnosticInfo original, DiagnosticSeverity severity)
		: base(original, severity)
	{
		_symbolOrSymbolWithAnnotations = original._symbolOrSymbolWithAnnotations;
		_containingSymbol = original._containingSymbol;
		_binderFlags = original._binderFlags;
	}

	protected override DiagnosticInfo GetInstanceWithSeverityCore(DiagnosticSeverity severity)
	{
		return new LazyObsoleteDiagnosticInfo(this, severity);
	}

	protected override DiagnosticInfo ResolveInfo()
	{
		Symbol symbol = (_symbolOrSymbolWithAnnotations as Symbol) ?? ((TypeWithAnnotations)_symbolOrSymbolWithAnnotations).Type;
		symbol.ForceCompleteObsoleteAttribute();
		if (ObsoleteAttributeHelpers.GetObsoleteDiagnosticKind(symbol, _containingSymbol, forceComplete: true) != ObsoleteDiagnosticKind.Diagnostic)
		{
			return null;
		}
		return ObsoleteAttributeHelpers.CreateObsoleteDiagnostic(symbol, _binderFlags);
	}
}
