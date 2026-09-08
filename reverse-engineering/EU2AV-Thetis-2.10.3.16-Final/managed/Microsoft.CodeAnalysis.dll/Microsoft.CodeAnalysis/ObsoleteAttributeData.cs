namespace Microsoft.CodeAnalysis;

internal sealed class ObsoleteAttributeData
{
	public static readonly ObsoleteAttributeData Uninitialized = new ObsoleteAttributeData(ObsoleteAttributeKind.Uninitialized, null, isError: false, null, null);

	public static readonly ObsoleteAttributeData WindowsExperimental = new ObsoleteAttributeData(ObsoleteAttributeKind.WindowsExperimental, null, isError: false, null, null);

	public const string DiagnosticIdPropertyName = "DiagnosticId";

	public const string UrlFormatPropertyName = "UrlFormat";

	public const string MessagePropertyName = "Message";

	public readonly ObsoleteAttributeKind Kind;

	public readonly bool IsError;

	public readonly string? Message;

	public readonly string? DiagnosticId;

	public readonly string? UrlFormat;

	internal bool IsUninitialized => this == Uninitialized;

	public ObsoleteAttributeData(ObsoleteAttributeKind kind, string? message, bool isError, string? diagnosticId, string? urlFormat)
	{
		Kind = kind;
		Message = message;
		IsError = isError;
		DiagnosticId = diagnosticId;
		UrlFormat = urlFormat;
	}
}
