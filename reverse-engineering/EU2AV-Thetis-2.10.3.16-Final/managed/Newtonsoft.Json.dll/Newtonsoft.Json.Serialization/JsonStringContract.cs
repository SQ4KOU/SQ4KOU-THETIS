using System;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.Serialization;

public class JsonStringContract : JsonPrimitiveContract
{
	[RequiresUnreferencedCode("Newtonsoft.Json relies on reflection over types that may be removed when trimming.")]
	public JsonStringContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.String;
	}
}
