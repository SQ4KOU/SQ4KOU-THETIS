using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class SelectMenuDefaultValueTypeConverter : JsonConverter
{
	public static readonly SelectMenuDefaultValueTypeConverter Instance = new SelectMenuDefaultValueTypeConverter();

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (!Enum.TryParse<SelectDefaultValueType>((string)reader.Value, ignoreCase: true, out var result))
		{
			return null;
		}
		return result;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue(((SelectDefaultValueType)value/*cast due to constrained. prefix*/).ToString().ToLower(CultureInfo.InvariantCulture));
	}
}
