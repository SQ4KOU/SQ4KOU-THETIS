namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class TypeEarlyWellKnownAttributeData : CommonTypeEarlyWellKnownAttributeData
{
	private bool _hasInterpolatedStringHandlerAttribute;

	private int _inlineArrayLength;

	private CollectionBuilderAttributeData? _collectionBuilder;

	public bool HasInterpolatedStringHandlerAttribute
	{
		get
		{
			return _hasInterpolatedStringHandlerAttribute;
		}
		set
		{
			_hasInterpolatedStringHandlerAttribute = value;
		}
	}

	public int InlineArrayLength
	{
		get
		{
			return _inlineArrayLength;
		}
		set
		{
			if (_inlineArrayLength == 0)
			{
				_inlineArrayLength = value;
			}
		}
	}

	public CollectionBuilderAttributeData? CollectionBuilder
	{
		get
		{
			return _collectionBuilder;
		}
		set
		{
			if (_collectionBuilder == null)
			{
				_collectionBuilder = value;
			}
		}
	}
}
