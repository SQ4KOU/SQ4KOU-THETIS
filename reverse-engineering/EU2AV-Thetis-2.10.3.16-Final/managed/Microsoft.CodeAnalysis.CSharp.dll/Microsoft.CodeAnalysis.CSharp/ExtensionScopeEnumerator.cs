namespace Microsoft.CodeAnalysis.CSharp;

internal struct ExtensionScopeEnumerator(Binder binder)
{
	private readonly Binder _binder = binder;

	private ExtensionScope _current = default(ExtensionScope);

	public ExtensionScope Current => _current;

	public bool MoveNext()
	{
		if (_current.Binder == null)
		{
			_current = GetNextScope(_binder);
		}
		else
		{
			Binder binder = _current.Binder;
			_current = GetNextScope(binder.Next);
		}
		return _current.Binder != null;
	}

	private static ExtensionScope GetNextScope(Binder binder)
	{
		for (Binder binder2 = binder; binder2 != null; binder2 = binder2.Next)
		{
			if (binder2.SupportsExtensions)
			{
				return new ExtensionScope(binder2);
			}
		}
		return default(ExtensionScope);
	}
}
