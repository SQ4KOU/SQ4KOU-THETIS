using System;
using System.ComponentModel;
using System.Globalization;

namespace Svg;

internal interface ISvgPropertyDescriptor
{
	DescriptorType DescriptorType { get; }

	string AttributeName { get; }

	string AttributeNamespace { get; }

	TypeConverter Converter { get; }

	Type Type { get; }

	object GetValue(object component);

	void SetValue(object component, ITypeDescriptorContext context, CultureInfo culture, object value);
}
