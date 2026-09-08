namespace Microsoft.CodeAnalysis;

internal static class RefKindExtensions
{
	internal const RefKind StrictIn = (RefKind)5;

	internal static string ToParameterDisplayString(this RefKind kind)
	{
		return kind switch
		{
			RefKind.Out => "out", 
			RefKind.Ref => "ref", 
			RefKind.In => "in", 
			RefKind.RefReadOnlyParameter => "ref readonly", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static string ToArgumentDisplayString(this RefKind kind)
	{
		return kind switch
		{
			RefKind.Out => "out", 
			RefKind.Ref => "ref", 
			RefKind.In => "in", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static string ToParameterPrefix(this RefKind kind)
	{
		return kind switch
		{
			RefKind.Out => "out ", 
			RefKind.Ref => "ref ", 
			RefKind.In => "in ", 
			RefKind.RefReadOnlyParameter => "ref readonly ", 
			RefKind.None => string.Empty, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}
}
