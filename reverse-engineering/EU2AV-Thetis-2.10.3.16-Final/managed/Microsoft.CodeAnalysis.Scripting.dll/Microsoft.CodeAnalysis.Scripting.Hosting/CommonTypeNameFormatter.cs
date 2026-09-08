using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal abstract class CommonTypeNameFormatter
{
	protected abstract string GenericParameterOpening { get; }

	protected abstract string GenericParameterClosing { get; }

	protected abstract string ArrayOpening { get; }

	protected abstract string ArrayClosing { get; }

	protected abstract CommonPrimitiveFormatter PrimitiveFormatter { get; }

	protected abstract string GetPrimitiveTypeName(SpecialType type);

	public virtual string FormatTypeName(Type type, CommonTypeNameFormatterOptions options)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		string primitiveTypeName = GetPrimitiveTypeName(ObjectFormatterHelpers.GetPrimitiveSpecialType(type));
		if (primitiveTypeName != null)
		{
			return primitiveTypeName;
		}
		if (type.IsGenericParameter)
		{
			return type.Name;
		}
		if (type.IsArray)
		{
			return FormatArrayTypeName(type, null, options);
		}
		System.Reflection.TypeInfo typeInfo = type.GetTypeInfo();
		if (typeInfo.IsGenericType)
		{
			return FormatGenericTypeName(typeInfo, options);
		}
		return FormatNonGenericTypeName(typeInfo, options);
	}

	private static string FormatNonGenericTypeName(System.Reflection.TypeInfo typeInfo, CommonTypeNameFormatterOptions options)
	{
		if (options.ShowNamespaces)
		{
			return typeInfo.FullName.Replace('+', '.');
		}
		if (typeInfo.DeclaringType == null)
		{
			return typeInfo.Name;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		do
		{
			instance.Push(typeInfo.Name);
			typeInfo = typeInfo.DeclaringType?.GetTypeInfo();
		}
		while (typeInfo != null);
		instance.ReverseContents();
		string result = string.Join(".", instance);
		instance.Free();
		return result;
	}

	public virtual string FormatTypeArguments(Type[] typeArguments, CommonTypeNameFormatterOptions options)
	{
		if (typeArguments == null)
		{
			throw new ArgumentNullException("typeArguments");
		}
		if (typeArguments.Length == 0)
		{
			throw new ArgumentException(null, "typeArguments");
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append(GenericParameterOpening);
		bool flag = true;
		foreach (Type type in typeArguments)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				builder.Append(", ");
			}
			builder.Append(FormatTypeName(type, options));
		}
		builder.Append(GenericParameterClosing);
		return instance.ToStringAndFree();
	}

	public virtual string FormatArrayTypeName(Type arrayType, Array arrayOpt, CommonTypeNameFormatterOptions options)
	{
		if (arrayType == null)
		{
			throw new ArgumentNullException("arrayType");
		}
		StringBuilder stringBuilder = new StringBuilder();
		Type elementType = arrayType.GetElementType();
		while (elementType.IsArray)
		{
			elementType = elementType.GetElementType();
		}
		stringBuilder.Append(FormatTypeName(elementType, options));
		Type type = arrayType;
		do
		{
			if (arrayOpt != null)
			{
				stringBuilder.Append(ArrayOpening);
				int arrayRank = type.GetArrayRank();
				bool flag = false;
				for (int i = 0; i < arrayRank; i++)
				{
					if (arrayOpt.GetLowerBound(i) > 0)
					{
						flag = true;
						break;
					}
				}
				for (int j = 0; j < arrayRank; j++)
				{
					int lowerBound = arrayOpt.GetLowerBound(j);
					int length = arrayOpt.GetLength(j);
					if (j > 0)
					{
						stringBuilder.Append(", ");
					}
					if (flag)
					{
						AppendArrayBound(stringBuilder, lowerBound, options.ArrayBoundRadix);
						stringBuilder.Append("..");
						AppendArrayBound(stringBuilder, length + lowerBound, options.ArrayBoundRadix);
					}
					else
					{
						AppendArrayBound(stringBuilder, length, options.ArrayBoundRadix);
					}
				}
				stringBuilder.Append(ArrayClosing);
				arrayOpt = null;
			}
			else
			{
				AppendArrayRank(stringBuilder, type);
			}
			type = type.GetElementType();
		}
		while (type.IsArray);
		return stringBuilder.ToString();
	}

	private void AppendArrayBound(StringBuilder sb, long bound, int numberRadix)
	{
		CommonPrimitiveFormatterOptions options = new CommonPrimitiveFormatterOptions(numberRadix, includeCodePoints: false, quoteStringsAndCharacters: true, escapeNonPrintableCharacters: true, CultureInfo.InvariantCulture);
		string value = ((bound >= int.MinValue && bound <= int.MaxValue) ? PrimitiveFormatter.FormatPrimitive((int)bound, options) : PrimitiveFormatter.FormatPrimitive(bound, options));
		sb.Append(value);
	}

	private void AppendArrayRank(StringBuilder sb, Type arrayType)
	{
		sb.Append(ArrayOpening);
		int arrayRank = arrayType.GetArrayRank();
		if (arrayRank > 1)
		{
			sb.Append(',', arrayRank - 1);
		}
		sb.Append(ArrayClosing);
	}

	private string FormatGenericTypeName(System.Reflection.TypeInfo typeInfo, CommonTypeNameFormatterOptions options)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		Type[] genericArguments = (typeInfo.IsGenericTypeDefinition ? typeInfo.GenericTypeParameters : typeInfo.GenericTypeArguments);
		if (typeInfo.DeclaringType != null)
		{
			ArrayBuilder<System.Reflection.TypeInfo> instance2 = ArrayBuilder<System.Reflection.TypeInfo>.GetInstance();
			do
			{
				instance2.Add(typeInfo);
				typeInfo = typeInfo.DeclaringType?.GetTypeInfo();
			}
			while (typeInfo != null);
			if (options.ShowNamespaces)
			{
				string text = instance2.Last().Namespace;
				if (text != null)
				{
					builder.Append(text + ".");
				}
			}
			int genericArgIndex = 0;
			for (int num = instance2.Count - 1; num >= 0; num--)
			{
				AppendTypeInstantiation(builder, instance2[num], genericArguments, ref genericArgIndex, options);
				if (num > 0)
				{
					builder.Append('.');
				}
			}
			instance2.Free();
		}
		else
		{
			int genericArgIndex2 = 0;
			AppendTypeInstantiation(builder, typeInfo, genericArguments, ref genericArgIndex2, options);
		}
		return instance.ToStringAndFree();
	}

	private void AppendTypeInstantiation(StringBuilder builder, System.Reflection.TypeInfo typeInfo, Type[] genericArguments, ref int genericArgIndex, CommonTypeNameFormatterOptions options)
	{
		int num = (typeInfo.IsGenericTypeDefinition ? typeInfo.GenericTypeParameters.Length : typeInfo.GenericTypeArguments.Length) - genericArgIndex;
		if (num > 0)
		{
			string name = typeInfo.Name;
			int num2 = name.IndexOf('`');
			if (num2 > 0)
			{
				builder.Append(name.Substring(0, num2));
			}
			else
			{
				builder.Append(name);
			}
			builder.Append(GenericParameterOpening);
			for (int i = 0; i < num; i++)
			{
				if (i > 0)
				{
					builder.Append(", ");
				}
				builder.Append(FormatTypeName(genericArguments[genericArgIndex++], options));
			}
			builder.Append(GenericParameterClosing);
		}
		else
		{
			builder.Append(typeInfo.Name);
		}
	}
}
