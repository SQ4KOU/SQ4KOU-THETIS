using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

public class SKRuntimeEffectUniforms : IEnumerable<string>, IEnumerable, IDisposable
{
	internal readonly struct Variable(int index, string name, SKRuntimeEffectUniformNative uniform)
	{
		public int Index { get; } = index;

		public string Name { get; } = name;

		public int Offset { get; } = (int)uniform.fOffset;

		public SKRuntimeEffectUniformTypeNative Type { get; } = uniform.fType;

		public int Count { get; } = uniform.fCount;

		public SKRuntimeEffectUniformFlagsNative Flags { get; } = uniform.fFlags;

		public int ElementSize => Type switch
		{
			SKRuntimeEffectUniformTypeNative.Float => 4, 
			SKRuntimeEffectUniformTypeNative.Float2 => 8, 
			SKRuntimeEffectUniformTypeNative.Float3 => 12, 
			SKRuntimeEffectUniformTypeNative.Float4 => 16, 
			SKRuntimeEffectUniformTypeNative.Float2x2 => 16, 
			SKRuntimeEffectUniformTypeNative.Float3x3 => 36, 
			SKRuntimeEffectUniformTypeNative.Float4x4 => 64, 
			SKRuntimeEffectUniformTypeNative.Int => 4, 
			SKRuntimeEffectUniformTypeNative.Int2 => 8, 
			SKRuntimeEffectUniformTypeNative.Int3 => 12, 
			SKRuntimeEffectUniformTypeNative.Int4 => 16, 
			_ => throw new ArgumentOutOfRangeException("Type", $"Unknown variable type: '{Type}'"), 
		};

		public int Size => ElementSize * Count;
	}

	private readonly string[] names;

	private readonly Dictionary<string, Variable> uniforms;

	private SKData data;

	public IReadOnlyList<string> Names => names;

	internal IReadOnlyList<Variable> Variables => uniforms.Values.OrderBy((Variable v) => v.Index).ToArray();

	public int Count => names.Length;

	public int Size => (int)data.Size;

	public SKRuntimeEffectUniform this[string name]
	{
		set
		{
			Add(name, value);
		}
	}

	public unsafe SKRuntimeEffectUniforms(SKRuntimeEffect effect)
	{
		if (effect == null)
		{
			throw new ArgumentNullException("effect");
		}
		names = effect.Uniforms.ToArray();
		uniforms = new Dictionary<string, Variable>();
		int uniformSize = effect.UniformSize;
		data = ((uniformSize > 0) ? SKData.Create(effect.UniformSize) : SKData.Empty);
		SKRuntimeEffectUniformNative uniform = default(SKRuntimeEffectUniformNative);
		for (int i = 0; i < names.Length; i++)
		{
			string text = names[i];
			SkiaApi.sk_runtimeeffect_get_uniform_from_index(effect.Handle, i, &uniform);
			uniforms[text] = new Variable(i, text, uniform);
		}
	}

	public void Reset()
	{
		if (data.Size != 0L)
		{
			data = SKData.Create(data.Size);
		}
	}

	public bool Contains(string name)
	{
		return Array.IndexOf(names, name) != -1;
	}

	public void Add(string name, SKRuntimeEffectUniform value)
	{
		int num = Array.IndexOf(names, name);
		if (num == -1)
		{
			throw new ArgumentOutOfRangeException(name, "Variable was not found for name: '" + name + "'.");
		}
		Variable variable = uniforms[name];
		if (!ValidateTypes(value.Type, variable.Type, variable.Flags.HasFlag(SKRuntimeEffectUniformFlagsNative.Array), variable.Count))
		{
			throw new ArgumentOutOfRangeException("value", $"Unable to write a '{value.Type}' value to a '{variable.Type}' uniform.");
		}
		Span<byte> span = data.Span.Slice(variable.Offset, variable.Size);
		value.WriteTo(span);
	}

	public SKData ToData()
	{
		if (data.Size == 0L)
		{
			return SKData.Empty;
		}
		return SKData.CreateCopy(data.Data, data.Size);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<string> GetEnumerator()
	{
		return ((IEnumerable<string>)names).GetEnumerator();
	}

	public void Dispose()
	{
		data.Dispose();
	}

	private bool ValidateTypes(SKRuntimeEffectUniform.DataType valueType, SKRuntimeEffectUniformTypeNative uniformType, bool isArray, int arraySize)
	{
		switch (valueType)
		{
		case SKRuntimeEffectUniform.DataType.Float:
			return (uniformType == SKRuntimeEffectUniformTypeNative.Float && !isArray) ? true : false;
		case SKRuntimeEffectUniform.DataType.FloatArray:
		{
			bool result;
			switch (uniformType)
			{
			case SKRuntimeEffectUniformTypeNative.Float:
				if (isArray)
				{
					result = true;
					break;
				}
				goto default;
			case SKRuntimeEffectUniformTypeNative.Float2:
				result = true;
				break;
			case SKRuntimeEffectUniformTypeNative.Float3:
				result = true;
				break;
			case SKRuntimeEffectUniformTypeNative.Float4:
				result = true;
				break;
			case SKRuntimeEffectUniformTypeNative.Float2x2:
				result = true;
				break;
			case SKRuntimeEffectUniformTypeNative.Float3x3:
				result = true;
				break;
			case SKRuntimeEffectUniformTypeNative.Float4x4:
				result = true;
				break;
			default:
				result = false;
				break;
			}
			return result;
		}
		case SKRuntimeEffectUniform.DataType.Int32:
			return (uniformType == SKRuntimeEffectUniformTypeNative.Int && !isArray) ? true : false;
		case SKRuntimeEffectUniform.DataType.Int32Array:
		{
			bool result;
			switch (uniformType)
			{
			case SKRuntimeEffectUniformTypeNative.Int:
				if (isArray)
				{
					result = true;
					break;
				}
				goto default;
			case SKRuntimeEffectUniformTypeNative.Int2:
				result = true;
				break;
			case SKRuntimeEffectUniformTypeNative.Int3:
				result = true;
				break;
			case SKRuntimeEffectUniformTypeNative.Int4:
				result = true;
				break;
			default:
				result = false;
				break;
			}
			return result;
		}
		case SKRuntimeEffectUniform.DataType.Color:
			return uniformType switch
			{
				SKRuntimeEffectUniformTypeNative.Float3 => true, 
				SKRuntimeEffectUniformTypeNative.Float4 => true, 
				_ => false, 
			};
		default:
			return false;
		}
	}
}
