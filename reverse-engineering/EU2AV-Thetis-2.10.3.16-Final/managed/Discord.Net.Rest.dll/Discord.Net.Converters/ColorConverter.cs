using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Discord.Net.Converters;

internal class ColorConverter : JsonConverter
{
	public static readonly ColorConverter Instance = new ColorConverter();

	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return null;
		}
		return new Color(uint.Parse(reader.Value.ToString().TrimStart('#'), NumberStyles.HexNumber));
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		writer.WriteValue($"#{(uint)value:X}");
	}
}
