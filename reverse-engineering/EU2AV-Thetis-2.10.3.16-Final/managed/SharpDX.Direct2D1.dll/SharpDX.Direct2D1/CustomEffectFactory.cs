using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpDX.Direct2D1;

internal class CustomEffectFactory
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate int CreateCustomEffectDelegate(out IntPtr nativeCustomEffectPtr);

	private enum SaveOptions
	{
		None
	}

	private class XNode
	{
		private List<XNode> nodes = new List<XNode>();

		public bool HasChildren => nodes.Count > 0;

		public IEnumerable<XNode> Children => nodes;

		public void Add(XNode node)
		{
			nodes.Add(node);
		}

		public override string ToString()
		{
			return ToString(SaveOptions.None);
		}

		public string ToString(SaveOptions options)
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		protected virtual void ToString(StringBuilder builder)
		{
			foreach (XNode child in Children)
			{
				child.ToString(builder);
			}
		}
	}

	private class XDocument : XNode
	{
	}

	private class XElement : XNode
	{
		private List<XElementAttribute> attributes = new List<XElementAttribute>();

		public string Name { get; set; }

		public XElement()
		{
		}

		public XElement(string name)
		{
			Name = name;
		}

		public void SetAttributeValue(string name, object value)
		{
			attributes.Add(new XElementAttribute(name, value));
		}

		protected override void ToString(StringBuilder builder)
		{
			builder.AppendFormat("<{0}", Name);
			if (attributes.Count > 0)
			{
				foreach (XElementAttribute attribute in attributes)
				{
					builder.Append(" ");
					attribute.ToString(builder);
				}
			}
			if (base.HasChildren)
			{
				builder.AppendLine(">");
				base.ToString(builder);
				builder.AppendFormat("</{0}>", Name);
				builder.AppendLine();
			}
			else
			{
				builder.AppendLine("/>");
			}
		}
	}

	private class XElementAttribute
	{
		public string Name { get; set; }

		public object Value { get; set; }

		public XElementAttribute()
		{
		}

		public XElementAttribute(string name, object value)
		{
			Name = name;
			Value = value;
		}

		public void ToString(StringBuilder builder)
		{
			builder.AppendFormat("{0}='{1}'", Name, Value);
		}
	}

	private Type customEffectType;

	private CreateCustomEffectDelegate callback;

	private XDocument xml;

	public Guid Guid { get; private set; }

	public CustomEffectFactoryDelegate Factory { get; private set; }

	public IntPtr NativePointer { get; private set; }

	public PropertyBinding[] Bindings { get; private set; }

	public CustomEffectFactory(CustomEffectFactoryDelegate factory, Type customEffectType)
		: this(factory, customEffectType, Utilities.GetGuidFromType(customEffectType))
	{
	}

	public CustomEffectFactory(CustomEffectFactoryDelegate factory, Type customEffectType, Guid effectId)
	{
		this.customEffectType = customEffectType;
		Guid = effectId;
		Factory = factory;
		callback = CreateCustomEffectImpl;
		NativePointer = Marshal.GetFunctionPointerForDelegate((Delegate)callback);
		InitializeBindings();
		InitializeXml();
	}

	public string ToXml()
	{
		return $"<?xml version='1.0'?>\r\n{xml.ToString(SaveOptions.None)}";
	}

	private void InitializeBindings()
	{
		List<PropertyBinding> list = new List<PropertyBinding>();
		foreach (PropertyInfo declaredProperty in customEffectType.GetTypeInfo().DeclaredProperties)
		{
			PropertyBinding propertyBinding = PropertyBinding.Get(customEffectType, declaredProperty);
			if (propertyBinding != null)
			{
				list.Add(propertyBinding);
			}
		}
		list.Sort((PropertyBinding left, PropertyBinding right) => left.Attribute.Order.CompareTo(right.Attribute.Order));
		Bindings = list.ToArray();
	}

	private static XElement CreateXmlProperty(string name, string type, string value = null)
	{
		XElement xElement = new XElement("Property");
		xElement.SetAttributeValue("name", name);
		xElement.SetAttributeValue("type", type);
		if (value != null)
		{
			xElement.SetAttributeValue("value", value);
		}
		return xElement;
	}

	private void InitializeXml()
	{
		xml = new XDocument();
		XElement xElement = new XElement("Effect");
		xml.Add(xElement);
		TypeInfo typeInfo = customEffectType.GetTypeInfo();
		CustomEffectAttribute customAttribute = Utilities.GetCustomAttribute<CustomEffectAttribute>(typeInfo, inherited: true);
		xElement.Add(CreateXmlProperty("DisplayName", "string", (customAttribute != null) ? customAttribute.DisplayName : typeInfo.Name));
		xElement.Add(CreateXmlProperty("Author", "string", (customAttribute != null) ? customAttribute.Author : string.Empty));
		xElement.Add(CreateXmlProperty("Category", "string", (customAttribute != null) ? customAttribute.Category : string.Empty));
		xElement.Add(CreateXmlProperty("Description", "string", (customAttribute != null) ? customAttribute.Description : string.Empty));
		XElement xElement2 = new XElement("Inputs");
		foreach (CustomEffectInputAttribute customAttribute2 in Utilities.GetCustomAttributes<CustomEffectInputAttribute>(typeInfo, inherited: true))
		{
			XElement xElement3 = new XElement("Input");
			xElement3.SetAttributeValue("name", customAttribute2.Input);
			xElement2.Add(xElement3);
		}
		xElement.Add(xElement2);
		PropertyBinding[] bindings = Bindings;
		foreach (PropertyBinding propertyBinding in bindings)
		{
			XElement xElement4 = CreateXmlProperty(propertyBinding.PropertyName, propertyBinding.TypeName);
			xElement4.Add(CreateXmlProperty("DisplayName", "string", propertyBinding.PropertyName));
			xElement4.Add(CreateXmlProperty("Min", propertyBinding.TypeName, (propertyBinding.Attribute.Min != null) ? propertyBinding.Attribute.Min.ToString() : string.Empty));
			xElement4.Add(CreateXmlProperty("Max", propertyBinding.TypeName, (propertyBinding.Attribute.Max != null) ? propertyBinding.Attribute.Max.ToString() : string.Empty));
			xElement4.Add(CreateXmlProperty("Default", propertyBinding.TypeName, (propertyBinding.Attribute.Default != null) ? propertyBinding.Attribute.Default.ToString() : string.Empty));
			xElement.Add(xElement4);
		}
	}

	private int CreateCustomEffectImpl(out IntPtr nativeCustomEffectPtr)
	{
		nativeCustomEffectPtr = IntPtr.Zero;
		try
		{
			CustomEffect customEffect = Factory();
			nativeCustomEffectPtr = CustomEffectShadow.ToIntPtr(customEffect);
		}
		catch (SharpDXException ex)
		{
			return ex.ResultCode.Code;
		}
		catch (Exception)
		{
			return Result.Fail.Code;
		}
		return Result.Ok.Code;
	}
}
