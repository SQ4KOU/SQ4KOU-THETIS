#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using Svg.DataTypes;

namespace Svg;

[SvgElement("image")]
public class SvgImage : SvgVisualElement
{
	private const string MimeTypeSvg = "image/svg+xml";

	private bool _gettingBounds;

	private GraphicsPath _path;

	internal static List<Type> SvgImageClassNames = new List<Type> { typeof(SvgImage) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgImageProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["preserveAspectRatio"] = new SvgPropertyDescriptor<SvgImage, SvgAspectRatio>(DescriptorType.Property, "preserveAspectRatio", "http://www.w3.org/2000/svg", new SvgPreserveAspectRatioConverter(), (SvgImage t) => t.AspectRatio, delegate(SvgImage t, SvgAspectRatio v)
		{
			t.AspectRatio = v;
		}),
		["x"] = new SvgPropertyDescriptor<SvgImage, SvgUnit>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgImage t) => t.X, delegate(SvgImage t, SvgUnit v)
		{
			t.X = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgImage, SvgUnit>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgImage t) => t.Y, delegate(SvgImage t, SvgUnit v)
		{
			t.Y = v;
		}),
		["width"] = new SvgPropertyDescriptor<SvgImage, SvgUnit>(DescriptorType.Property, "width", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgImage t) => t.Width, delegate(SvgImage t, SvgUnit v)
		{
			t.Width = v;
		}),
		["height"] = new SvgPropertyDescriptor<SvgImage, SvgUnit>(DescriptorType.Property, "height", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgImage t) => t.Height, delegate(SvgImage t, SvgUnit v)
		{
			t.Height = v;
		}),
		["href"] = new SvgPropertyDescriptor<SvgImage, string>(DescriptorType.Property, "href", "http://www.w3.org/1999/xlink", new StringConverter(), (SvgImage t) => t.Href, delegate(SvgImage t, string v)
		{
			t.Href = v;
		})
	};

	public SvgPoint Location => new SvgPoint(X, Y);

	[SvgAttribute("preserveAspectRatio")]
	public SvgAspectRatio AspectRatio
	{
		get
		{
			return GetAttribute("preserveAspectRatio", inherited: false, new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid));
		}
		set
		{
			Attributes["preserveAspectRatio"] = value;
		}
	}

	[SvgAttribute("x")]
	public virtual SvgUnit X
	{
		get
		{
			return GetAttribute<SvgUnit>("x", inherited: false);
		}
		set
		{
			Attributes["x"] = value;
		}
	}

	[SvgAttribute("y")]
	public virtual SvgUnit Y
	{
		get
		{
			return GetAttribute<SvgUnit>("y", inherited: false);
		}
		set
		{
			Attributes["y"] = value;
		}
	}

	[SvgAttribute("width")]
	public virtual SvgUnit Width
	{
		get
		{
			return GetAttribute<SvgUnit>("width", inherited: false);
		}
		set
		{
			Attributes["width"] = value;
		}
	}

	[SvgAttribute("height")]
	public virtual SvgUnit Height
	{
		get
		{
			return GetAttribute<SvgUnit>("height", inherited: false);
		}
		set
		{
			Attributes["height"] = value;
		}
	}

	[SvgAttribute("href", "http://www.w3.org/1999/xlink")]
	public virtual string Href
	{
		get
		{
			return GetAttribute<string>("href", inherited: false);
		}
		set
		{
			Attributes["href"] = value;
		}
	}

	internal ExternalType ResolveExternalImages => SvgDocument.ResolveExternalImages;

	public override RectangleF Bounds
	{
		get
		{
			if (_gettingBounds)
			{
				return default(RectangleF);
			}
			_gettingBounds = true;
			RectangleF result = TransformedBounds(new RectangleF(Location.ToDeviceValue(null, this), new SizeF(Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this), Height.ToDeviceValue(null, UnitRenderingType.Vertical, this))));
			_gettingBounds = false;
			return result;
		}
	}

	internal override string AttributeName => "image";

	internal override List<Type> ClassNames => SvgImageClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgImageProperties;

	private SvgDocument LoadSvg(Stream stream, Uri baseUri)
	{
		SvgDocument svgDocument = SvgDocument.Open<SvgDocument>(stream);
		svgDocument.BaseUri = baseUri;
		return svgDocument;
	}

	public override SvgElement DeepCopy()
	{
		return DeepCopy<SvgImage>();
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		if (_path == null)
		{
			RectangleF rect = new RectangleF(Location.ToDeviceValue(renderer, this), SvgUnit.GetDeviceSize(Width, Height, renderer, this));
			_path = new GraphicsPath();
			_path.StartFigure();
			_path.AddRectangle(rect);
			_path.CloseFigure();
		}
		return _path;
	}

	protected override void Render(ISvgRenderer renderer)
	{
		if (!Visible || !Displayable || !(Width.Value > 0f) || !(Height.Value > 0f) || Href == null)
		{
			return;
		}
		object image = GetImage(Href);
		Image image2 = image as Image;
		SvgFragment svgFragment = image as SvgFragment;
		if (image2 == null && svgFragment == null)
		{
			return;
		}
		try
		{
			if (!PushTransforms(renderer))
			{
				return;
			}
			RectangleF rectangleF = ((image2 == null) ? new RectangleF(new PointF(0f, 0f), svgFragment.GetDimensions(renderer)) : new RectangleF(0f, 0f, image2.Width, image2.Height));
			RectangleF rectangleF2 = new RectangleF(Location.ToDeviceValue(renderer, this), new SizeF(Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this), Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this)));
			RectangleF destRect = rectangleF2;
			renderer.SetClip(new Region(rectangleF2), CombineMode.Intersect);
			SetClip(renderer);
			SvgAspectRatio aspectRatio = AspectRatio;
			if (aspectRatio.Align != SvgPreserveAspectRatio.none)
			{
				float val = rectangleF2.Width / rectangleF.Width;
				float val2 = rectangleF2.Height / rectangleF.Height;
				float num = 0f;
				float num2 = 0f;
				if (aspectRatio.Slice)
				{
					val = Math.Max(val, val2);
					val2 = Math.Max(val, val2);
				}
				else
				{
					val = Math.Min(val, val2);
					val2 = Math.Min(val, val2);
				}
				switch (aspectRatio.Align)
				{
				case SvgPreserveAspectRatio.xMidYMin:
					num = (rectangleF2.Width - rectangleF.Width * val) / 2f;
					break;
				case SvgPreserveAspectRatio.xMaxYMin:
					num = rectangleF2.Width - rectangleF.Width * val;
					break;
				case SvgPreserveAspectRatio.xMinYMid:
					num2 = (rectangleF2.Height - rectangleF.Height * val2) / 2f;
					break;
				case SvgPreserveAspectRatio.xMidYMid:
					num = (rectangleF2.Width - rectangleF.Width * val) / 2f;
					num2 = (rectangleF2.Height - rectangleF.Height * val2) / 2f;
					break;
				case SvgPreserveAspectRatio.xMaxYMid:
					num = rectangleF2.Width - rectangleF.Width * val;
					num2 = (rectangleF2.Height - rectangleF.Height * val2) / 2f;
					break;
				case SvgPreserveAspectRatio.xMinYMax:
					num2 = rectangleF2.Height - rectangleF.Height * val2;
					break;
				case SvgPreserveAspectRatio.xMidYMax:
					num = (rectangleF2.Width - rectangleF.Width * val) / 2f;
					num2 = rectangleF2.Height - rectangleF.Height * val2;
					break;
				case SvgPreserveAspectRatio.xMaxYMax:
					num = rectangleF2.Width - rectangleF.Width * val;
					num2 = rectangleF2.Height - rectangleF.Height * val2;
					break;
				}
				destRect = new RectangleF(rectangleF2.X + num, rectangleF2.Y + num2, rectangleF.Width * val, rectangleF.Height * val2);
			}
			if (image2 != null)
			{
				float num3 = SvgElement.FixOpacityValue(Opacity);
				if (num3 == 1f)
				{
					renderer.DrawImage(image2, destRect, rectangleF, GraphicsUnit.Pixel);
				}
				else
				{
					renderer.DrawImage(image2, destRect, rectangleF, GraphicsUnit.Pixel, num3);
				}
			}
			else
			{
				renderer.TranslateTransform(destRect.X, destRect.Y, MatrixOrder.Prepend);
				renderer.ScaleTransform(destRect.Width / rectangleF.Width, destRect.Height / rectangleF.Height, MatrixOrder.Prepend);
				try
				{
					renderer.SetBoundable(new GenericBoundable(rectangleF));
					svgFragment.RenderElement(renderer);
				}
				finally
				{
					renderer.PopBoundable();
				}
			}
			ResetClip(renderer);
		}
		finally
		{
			PopTransforms(renderer);
			image2?.Dispose();
		}
	}

	public object GetImage()
	{
		return GetImage(Href);
	}

	public object GetImage(string uriString)
	{
		string uriString2 = ((uriString.Length > 65519) ? uriString.Substring(0, 65519) : uriString);
		try
		{
			Uri uri = new Uri(uriString2, UriKind.RelativeOrAbsolute);
			if (uri.IsAbsoluteUri && uri.Scheme == "data")
			{
				return GetImageFromDataUri(uriString);
			}
			if (!uri.IsAbsoluteUri)
			{
				uri = new Uri(OwnerDocument.BaseUri, uri);
			}
			if (!ResolveExternalImages.AllowsResolving(uri))
			{
				Trace.TraceWarning("Trying to resolve image from '{0}', but resolving external resources of that type is disabled.", uri);
				return null;
			}
			if (uri.IsFile)
			{
				using (FileStream stream = File.OpenRead(uri.AbsolutePath))
				{
					if (!uri.LocalPath.EndsWith(".svg", StringComparison.InvariantCultureIgnoreCase))
					{
						using Image original = Image.FromStream(stream);
						return new Bitmap(original);
					}
					return LoadSvg(stream, uri);
				}
			}
			if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
			{
				using (HttpResponseMessage httpResponseMessage = SvgElement.HttpClient.GetAsync(uri).Result)
				{
					using Stream stream2 = httpResponseMessage.Content.ReadAsStreamAsync().Result;
					if (!uri.LocalPath.EndsWith(".svg", StringComparison.InvariantCultureIgnoreCase) && !(httpResponseMessage.Content.Headers.ContentType.MediaType == "image/svg+xml"))
					{
						using Image original2 = Image.FromStream(stream2);
						return new Bitmap(original2);
					}
					return LoadSvg(stream2, uri);
				}
			}
			throw new NotSupportedException();
		}
		catch (Exception ex)
		{
			Trace.TraceError("Error loading image: '{0}', error: {1} ", uriString, ex.Message);
			return null;
		}
	}

	private object GetImageFromDataUri(string uriString)
	{
		int num = 5;
		int num2 = uriString.IndexOf(",", num);
		if (num2 < 0 || num2 + 1 >= uriString.Length)
		{
			throw new Exception("Invalid data URI");
		}
		string text = "text/plain";
		string text2 = "US-ASCII";
		bool flag = false;
		List<string> list = new List<string>(uriString.Substring(num, num2 - num).Split(';'));
		if (list[0].Contains("/"))
		{
			text = list[0].Trim();
			list.RemoveAt(0);
			text2 = string.Empty;
		}
		if (list.Count > 0 && list[list.Count - 1].Trim().Equals("base64", StringComparison.InvariantCultureIgnoreCase))
		{
			flag = true;
			list.RemoveAt(list.Count - 1);
		}
		foreach (string item in list)
		{
			string[] array = item.Split('=');
			if (array.Length >= 2 && array[0].Trim().Equals("charset", StringComparison.InvariantCultureIgnoreCase))
			{
				text2 = array[1].Trim();
			}
		}
		string s = uriString.Substring(num2 + 1);
		if (text.Equals("image/svg+xml", StringComparison.InvariantCultureIgnoreCase))
		{
			if (flag)
			{
				s = (string.IsNullOrEmpty(text2) ? Encoding.UTF8 : Encoding.GetEncoding(text2)).GetString(Convert.FromBase64String(s));
			}
			using MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(s));
			return LoadSvg(stream, OwnerDocument.BaseUri);
		}
		if (text.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase) || text.StartsWith("img/", StringComparison.InvariantCultureIgnoreCase))
		{
			using (MemoryStream stream2 = new MemoryStream(flag ? Convert.FromBase64String(s) : Encoding.Default.GetBytes(s)))
			{
				using Image original = Image.FromStream(stream2);
				return new Bitmap(original);
			}
		}
		return null;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgImageProperty in SvgImageProperties)
		{
			yield return svgImageProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgImageProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgImageProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgImageProperties.TryGetValue(attributeName, out var value2))
		{
			try
			{
				value2.SetValue(this, context, culture, value);
			}
			catch
			{
				Trace.TraceWarning($"Attribute '{attributeName}' cannot be set - type '{GetType().FullName}' cannot convert from string '{value}'.");
			}
			return true;
		}
		return base.SetValue(attributeName, context, culture, value);
	}
}
