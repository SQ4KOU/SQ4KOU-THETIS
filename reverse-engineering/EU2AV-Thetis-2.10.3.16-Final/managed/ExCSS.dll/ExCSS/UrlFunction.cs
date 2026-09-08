namespace ExCSS;

internal sealed class UrlFunction : DocumentFunction
{
	private readonly Url _expected;

	public UrlFunction(string url)
		: base(FunctionNames.Url, url)
	{
		_expected = Url.Create(base.Data);
	}

	public override bool Matches(Url actual)
	{
		if (!_expected.IsInvalid)
		{
			return _expected.Equals(actual);
		}
		return false;
	}
}
