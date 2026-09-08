using System.Threading;

namespace Microsoft.CodeAnalysis;

internal sealed class CustomObsoleteDiagnosticInfo : DiagnosticInfo
{
	private DiagnosticDescriptor? _descriptor;

	internal ObsoleteAttributeData Data { get; }

	public override string MessageIdentifier
	{
		get
		{
			string diagnosticId = Data.DiagnosticId;
			if (!string.IsNullOrEmpty(diagnosticId))
			{
				return diagnosticId;
			}
			return base.MessageIdentifier;
		}
	}

	public override DiagnosticDescriptor Descriptor
	{
		get
		{
			if (_descriptor == null)
			{
				Interlocked.CompareExchange(ref _descriptor, CreateDescriptor(), null);
			}
			return _descriptor;
		}
	}

	internal CustomObsoleteDiagnosticInfo(CommonMessageProvider messageProvider, int errorCode, ObsoleteAttributeData data, params object[] arguments)
		: base(messageProvider, errorCode, arguments)
	{
		Data = data;
	}

	private CustomObsoleteDiagnosticInfo(CustomObsoleteDiagnosticInfo baseInfo, DiagnosticSeverity effectiveSeverity)
		: base(baseInfo, effectiveSeverity)
	{
		Data = baseInfo.Data;
	}

	protected override DiagnosticInfo GetInstanceWithSeverityCore(DiagnosticSeverity severity)
	{
		return new CustomObsoleteDiagnosticInfo(this, severity);
	}

	private DiagnosticDescriptor CreateDescriptor()
	{
		DiagnosticDescriptor descriptor = base.Descriptor;
		string diagnosticId = Data.DiagnosticId;
		string urlFormat = Data.UrlFormat;
		if (diagnosticId == null && urlFormat == null)
		{
			return descriptor;
		}
		string messageIdentifier = MessageIdentifier;
		string helpLinkUri = descriptor.HelpLinkUri;
		if (urlFormat != null)
		{
			try
			{
				helpLinkUri = string.Format(urlFormat, messageIdentifier);
			}
			catch
			{
			}
		}
		return new DiagnosticDescriptor(customTags: (diagnosticId != null) ? descriptor.ImmutableCustomTags.Add("CustomObsolete") : descriptor.ImmutableCustomTags, id: messageIdentifier, title: descriptor.Title, messageFormat: descriptor.MessageFormat, category: descriptor.Category, defaultSeverity: descriptor.DefaultSeverity, isEnabledByDefault: descriptor.IsEnabledByDefault, description: descriptor.Description, helpLinkUri: helpLinkUri);
	}
}
