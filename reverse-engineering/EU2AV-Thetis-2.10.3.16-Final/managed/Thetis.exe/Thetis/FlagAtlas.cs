using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace Thetis;

public static class FlagAtlas
{
	private sealed class AtlasDefinition
	{
		[JsonProperty("sprites")]
		public Dictionary<string, SpriteDefinition> Sprites { get; set; }
	}

	private sealed class SpriteDefinition
	{
		[JsonProperty("x")]
		public int X { get; set; }

		[JsonProperty("y")]
		public int Y { get; set; }

		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }
	}

	private static readonly object _sync = new object();

	private static Bitmap _atlas_image;

	private static Dictionary<string, Rectangle> _sprite_lookup = new Dictionary<string, Rectangle>(StringComparer.OrdinalIgnoreCase);

	public static void Init(Image atlas_image, string json)
	{
		if (atlas_image == null)
		{
			throw new ArgumentNullException("atlas_image");
		}
		if (string.IsNullOrWhiteSpace(json))
		{
			throw new ArgumentException("JSON is null or empty.", "json");
		}
		AtlasDefinition atlasDefinition = JsonConvert.DeserializeObject<AtlasDefinition>(json);
		if (atlasDefinition == null)
		{
			throw new InvalidOperationException("Failed to parse atlas JSON.");
		}
		if (atlasDefinition.Sprites == null || atlasDefinition.Sprites.Count == 0)
		{
			throw new InvalidOperationException("Atlas JSON contains no sprites.");
		}
		Dictionary<string, Rectangle> dictionary = new Dictionary<string, Rectangle>(atlasDefinition.Sprites.Count, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, SpriteDefinition> sprite in atlasDefinition.Sprites)
		{
			if (!string.IsNullOrWhiteSpace(sprite.Key))
			{
				if (sprite.Value == null)
				{
					throw new InvalidOperationException("Sprite entry is null for '" + sprite.Key + "'.");
				}
				Rectangle value = new Rectangle(sprite.Value.X, sprite.Value.Y, sprite.Value.Width, sprite.Value.Height);
				if (value.Width <= 0 || value.Height <= 0)
				{
					throw new InvalidOperationException("Sprite '" + sprite.Key + "' has an invalid size.");
				}
				if (value.X < 0 || value.Y < 0 || value.Right > atlas_image.Width || value.Bottom > atlas_image.Height)
				{
					throw new InvalidOperationException("Sprite '" + sprite.Key + "' is outside the atlas image bounds.");
				}
				dictionary[sprite.Key] = value;
			}
		}
		Bitmap atlas_image2 = new Bitmap(atlas_image);
		lock (_sync)
		{
			if (_atlas_image != null)
			{
				_atlas_image.Dispose();
			}
			_atlas_image = atlas_image2;
			_sprite_lookup = dictionary;
		}
	}

	public static Bitmap GetFlag(string flag_name)
	{
		if (string.IsNullOrWhiteSpace(flag_name))
		{
			throw new ArgumentException("Flag name is null or empty.", "flag_name");
		}
		flag_name = flag_name.Trim();
		if (!flag_name.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
		{
			flag_name += ".png";
		}
		lock (_sync)
		{
			ensureInitialised();
			if (!_sprite_lookup.TryGetValue(flag_name, out var value))
			{
				throw new KeyNotFoundException("Flag not found: " + flag_name);
			}
			return _atlas_image.Clone(value, PixelFormat.Format32bppArgb);
		}
	}

	public static bool ContainsFlag(string flag_name)
	{
		if (string.IsNullOrWhiteSpace(flag_name))
		{
			return false;
		}
		lock (_sync)
		{
			if (_atlas_image == null)
			{
				return false;
			}
			return _sprite_lookup.ContainsKey(flag_name);
		}
	}

	public static Rectangle GetFlagBounds(string flag_name)
	{
		if (string.IsNullOrWhiteSpace(flag_name))
		{
			throw new ArgumentException("Flag name is null or empty.", "flag_name");
		}
		lock (_sync)
		{
			ensureInitialised();
			if (!_sprite_lookup.TryGetValue(flag_name, out var value))
			{
				throw new KeyNotFoundException("Flag not found: " + flag_name);
			}
			return value;
		}
	}

	public static void Clear()
	{
		lock (_sync)
		{
			if (_atlas_image != null)
			{
				_atlas_image.Dispose();
				_atlas_image = null;
			}
			_sprite_lookup = new Dictionary<string, Rectangle>(StringComparer.OrdinalIgnoreCase);
		}
	}

	private static void ensureInitialised()
	{
		if (_atlas_image == null || _sprite_lookup == null || _sprite_lookup.Count == 0)
		{
			throw new InvalidOperationException("clsFlagAtlas has not been initialised.");
		}
	}
}
