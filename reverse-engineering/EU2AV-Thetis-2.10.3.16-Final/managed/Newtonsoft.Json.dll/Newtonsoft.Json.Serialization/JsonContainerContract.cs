using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

public class JsonContainerContract : JsonContract
{
	private JsonContract? _itemContract;

	private JsonContract? _finalItemContract;

	internal JsonContract? ItemContract
	{
		get
		{
			return _itemContract;
		}
		set
		{
			_itemContract = value;
			if (_itemContract != null)
			{
				_finalItemContract = (_itemContract.UnderlyingType.IsSealed() ? _itemContract : null);
			}
			else
			{
				_finalItemContract = null;
			}
		}
	}

	internal JsonContract? FinalItemContract => _finalItemContract;

	public JsonConverter? ItemConverter { get; set; }

	public bool? ItemIsReference { get; set; }

	public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

	public TypeNameHandling? ItemTypeNameHandling { get; set; }

	[RequiresUnreferencedCode("Newtonsoft.Json relies on reflection over types that may be removed when trimming.")]
	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	internal JsonContainerContract(Type underlyingType)
		: base(underlyingType)
	{
		JsonContainerAttribute cachedAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(underlyingType);
		if (cachedAttribute != null)
		{
			if (cachedAttribute.ItemConverterType != null)
			{
				ItemConverter = JsonTypeReflector.CreateJsonConverterInstance(cachedAttribute.ItemConverterType, cachedAttribute.ItemConverterParameters);
			}
			ItemIsReference = cachedAttribute._itemIsReference;
			ItemReferenceLoopHandling = cachedAttribute._itemReferenceLoopHandling;
			ItemTypeNameHandling = cachedAttribute._itemTypeNameHandling;
		}
	}
}
