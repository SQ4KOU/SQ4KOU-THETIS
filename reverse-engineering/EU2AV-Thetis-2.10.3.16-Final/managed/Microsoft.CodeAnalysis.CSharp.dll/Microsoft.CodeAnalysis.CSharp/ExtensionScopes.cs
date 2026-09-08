namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct ExtensionScopes(Binder binder)
{
	private readonly Binder _binder = binder;

	public ExtensionScopeEnumerator GetEnumerator()
	{
		return new ExtensionScopeEnumerator(_binder);
	}
}
