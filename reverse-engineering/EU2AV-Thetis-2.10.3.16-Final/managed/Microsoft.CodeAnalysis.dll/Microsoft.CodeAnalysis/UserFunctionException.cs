using System;

namespace Microsoft.CodeAnalysis;

internal sealed class UserFunctionException : Exception
{
	public new Exception InnerException => base.InnerException;

	public UserFunctionException(Exception innerException)
		: base("User provided code threw an exception", innerException)
	{
	}
}
