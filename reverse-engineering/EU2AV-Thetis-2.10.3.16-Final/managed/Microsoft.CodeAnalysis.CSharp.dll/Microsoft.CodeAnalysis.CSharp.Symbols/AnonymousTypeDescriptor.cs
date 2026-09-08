using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal readonly struct AnonymousTypeDescriptor(ImmutableArray<AnonymousTypeField> fields, Location location) : IEquatable<AnonymousTypeDescriptor>
{
	public readonly Location Location = location;

	public readonly ImmutableArray<AnonymousTypeField> Fields = fields;

	public readonly string Key = ComputeKey(fields, (AnonymousTypeField f) => f.Name);

	internal static string ComputeKey<T>(ImmutableArray<T> fields, Func<T, string> getName)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		foreach (T item in fields)
		{
			instance.Builder.Append('|');
			instance.Builder.Append(getName(item));
		}
		return instance.ToStringAndFree();
	}

	[Conditional("DEBUG")]
	internal void AssertIsGood()
	{
		foreach (AnonymousTypeField field in Fields)
		{
			_ = field;
		}
	}

	public bool Equals(AnonymousTypeDescriptor desc)
	{
		return Equals(desc, TypeCompareKind.ConsiderEverything);
	}

	internal bool Equals(AnonymousTypeDescriptor other, TypeCompareKind comparison)
	{
		if (Key != other.Key)
		{
			return false;
		}
		return Fields.SequenceEqual(other.Fields, comparison, (AnonymousTypeField x, AnonymousTypeField y, TypeCompareKind comparison2) => AnonymousTypeField.Equals(in x, in y, comparison2));
	}

	public override bool Equals(object? obj)
	{
		if (obj is AnonymousTypeDescriptor)
		{
			return Equals((AnonymousTypeDescriptor)obj, TypeCompareKind.ConsiderEverything);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Key.GetHashCode();
	}

	internal AnonymousTypeDescriptor WithNewFieldsTypes(ImmutableArray<TypeWithAnnotations> newFieldTypes)
	{
		return new AnonymousTypeDescriptor(Fields.ZipAsArray(newFieldTypes, (AnonymousTypeField field, TypeWithAnnotations type) => field.WithType(type)), Location);
	}

	internal AnonymousTypeDescriptor SubstituteTypes(AbstractTypeMap map, out bool changed)
	{
		ImmutableArray<TypeWithAnnotations> immutableArray = Fields.SelectAsArray((AnonymousTypeField f) => f.TypeWithAnnotations);
		ImmutableArray<TypeWithAnnotations> immutableArray2 = map.SubstituteTypes(immutableArray);
		changed = immutableArray != immutableArray2;
		if (!changed)
		{
			return this;
		}
		return WithNewFieldsTypes(immutableArray2);
	}
}
