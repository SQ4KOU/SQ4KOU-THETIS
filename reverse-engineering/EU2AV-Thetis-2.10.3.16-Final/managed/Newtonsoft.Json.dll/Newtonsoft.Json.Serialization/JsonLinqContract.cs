using System;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.Serialization;

public class JsonLinqContract : JsonContract
{
	[RequiresUnreferencedCode("Newtonsoft.Json relies on reflection over types that may be removed when trimming.")]
	public JsonLinqContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Linq;
	}
}
