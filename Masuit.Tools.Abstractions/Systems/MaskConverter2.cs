#if NET5_0_OR_GREATER
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Masuit.Tools.Systems.Text.Json;

public class MaskConverter : JsonConverter<string>
{
	/// <summary>Reads and converts the JSON to type <typeparamref name="T" />.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.GetString();
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Mask());
	}
}

public class MaskEmailConverter : JsonConverter<string>
{
	/// <summary>Reads and converts the JSON to type <typeparamref name="T" />.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.GetString();
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.MaskEmail());
	}
}

#endif
