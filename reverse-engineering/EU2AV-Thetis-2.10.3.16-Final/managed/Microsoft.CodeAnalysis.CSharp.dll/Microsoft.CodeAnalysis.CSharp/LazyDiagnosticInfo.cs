using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class LazyDiagnosticInfo : DiagnosticInfo
{
	private DiagnosticInfo? _lazyInfo;

	protected LazyDiagnosticInfo()
		: base(Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance, -1)
	{
	}

	internal sealed override DiagnosticInfo GetResolvedInfo()
	{
		if (_lazyInfo == null)
		{
			Interlocked.CompareExchange(ref _lazyInfo, ResolveInfo() ?? CSDiagnosticInfo.VoidDiagnosticInfo, null);
		}
		return _lazyInfo;
	}

	protected LazyDiagnosticInfo(LazyDiagnosticInfo original, DiagnosticSeverity severity)
		: base(original, severity)
	{
		_lazyInfo = original._lazyInfo;
	}

	protected abstract override DiagnosticInfo GetInstanceWithSeverityCore(DiagnosticSeverity severity);

	protected abstract DiagnosticInfo? ResolveInfo();
}
