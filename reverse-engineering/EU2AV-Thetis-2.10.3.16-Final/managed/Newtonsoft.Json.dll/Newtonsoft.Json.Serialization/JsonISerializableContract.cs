using System;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.Serialization;

public class JsonISerializableContract : JsonContainerContract
{
	public ObjectConstructor<object>? ISerializableCreator { get; set; }

	[RequiresUnreferencedCode("Newtonsoft.Json relies on reflection over types that may be removed when trimming.")]
	[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
	public JsonISerializableContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Serializable;
	}
}
