#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;
using ExCSS;
using Svg.Css;
using Svg.Exceptions;

namespace Svg;

public class SvgDocument : SvgFragment, ITypeDescriptorContext, IServiceProvider
{
	private static int? pointsPerInch;

	private SvgElementIdManager _idManager;

	private Dictionary<string, IEnumerable<SvgFontFace>> _fontDefns;

	private Uri baseUri;

	internal static List<Type> SvgDocumentClassNames = new List<Type> { typeof(SvgDocument) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgDocumentProperties = new Dictionary<string, ISvgPropertyDescriptor>();

	public static bool DisableDtdProcessing { get; set; }

	public static ExternalType ResolveExternalXmlEntites { get; set; } = ExternalType.None;

	public static ExternalType ResolveExternalImages { get; set; } = ExternalType.Local | ExternalType.Remote;

	public static ExternalType ResolveExternalElements { get; set; } = ExternalType.Local | ExternalType.Remote;

	public static int PointsPerInch
	{
		get
		{
			int? num = pointsPerInch;
			if (!num.HasValue)
			{
				int? num2 = (pointsPerInch = GetSystemDpi());
				return num2.Value;
			}
			return num.GetValueOrDefault();
		}
		set
		{
			pointsPerInch = value;
		}
	}

	public override SvgUnit X => 0f;

	public override SvgUnit Y => 0f;

	public override SvgOverflow Overflow => GetAttribute("overflow", inherited: false, SvgOverflow.Visible);

	public Uri BaseUri
	{
		get
		{
			return baseUri;
		}
		set
		{
			if (value != null && !value.IsAbsoluteUri)
			{
				throw new ArgumentException("BaseUri is not absolute.");
			}
			baseUri = value;
		}
	}

	protected internal virtual SvgElementIdManager IdManager
	{
		get
		{
			if (_idManager == null)
			{
				_idManager = new SvgElementIdManager(this);
			}
			return _idManager;
		}
	}

	public int Ppi { get; set; }

	public string ExternalCSSHref { get; set; }

	IContainer ITypeDescriptorContext.Container
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	object ITypeDescriptorContext.Instance => this;

	PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public static bool SkipGdiPlusCapabilityCheck { get; set; }

	internal SvgFontManager FontManager { get; private set; }

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgDocumentClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgDocumentProperties;

	private static int GetSystemDpi()
	{
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			return GetWin32SystemDpi();
		}
		return 96;
	}

	internal Dictionary<string, IEnumerable<SvgFontFace>> FontDefns()
	{
		if (_fontDefns == null)
		{
			_fontDefns = (from f in Descendants().OfType<SvgFontFace>()
				group f by f.FontFamily into family
				select (family)).ToDictionary((Func<IGrouping<string, SvgFontFace>, string>)((IGrouping<string, SvgFontFace> f) => f.Key), (Func<IGrouping<string, SvgFontFace>, IEnumerable<SvgFontFace>>)((IGrouping<string, SvgFontFace> f) => f));
		}
		return _fontDefns;
	}

	public SvgDocument()
	{
		Ppi = PointsPerInch;
		base.Namespaces.Add(string.Empty, "http://www.w3.org/2000/svg");
		base.Namespaces.Add("xlink", "http://www.w3.org/1999/xlink");
		base.Namespaces.Add("xml", "http://www.w3.org/XML/1998/namespace");
	}

	public void OverwriteIdManager(SvgElementIdManager manager)
	{
		_idManager = manager;
	}

	void ITypeDescriptorContext.OnComponentChanged()
	{
		throw new NotImplementedException();
	}

	bool ITypeDescriptorContext.OnComponentChanging()
	{
		throw new NotImplementedException();
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		throw new NotImplementedException();
	}

	public virtual SvgElement GetElementById(string id)
	{
		return IdManager.GetElementById(id);
	}

	public virtual TSvgElement GetElementById<TSvgElement>(string id) where TSvgElement : SvgElement
	{
		return GetElementById(id) as TSvgElement;
	}

	public static SvgDocument Open(string path)
	{
		return Open<SvgDocument>(path, new SvgOptions());
	}

	public static T Open<T>(string path) where T : SvgDocument, new()
	{
		return Open<T>(path, new SvgOptions());
	}

	[Obsolete("Use Open<T>(string path, SvgOptions svgOptions)")]
	public static T Open<T>(string path, Dictionary<string, string> entities) where T : SvgDocument, new()
	{
		return Open<T>(path, new SvgOptions(entities));
	}

	public static T Open<T>(string path, SvgOptions svgOptions) where T : SvgDocument, new()
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentNullException("path");
		}
		if (!File.Exists(path))
		{
			throw new FileNotFoundException("The specified document cannot be found.", path);
		}
		using FileStream stream = File.OpenRead(path);
		T val = Open<T>(stream, svgOptions);
		val.BaseUri = new Uri(System.IO.Path.GetFullPath(path));
		return val;
	}

	public static T Open<T>(Stream stream) where T : SvgDocument, new()
	{
		return Open<T>(stream, new SvgOptions());
	}

	[Obsolete("Use Open<T>(Stream stream, SvgOptions svgOptions)")]
	public static T Open<T>(Stream stream, Dictionary<string, string> entities) where T : SvgDocument, new()
	{
		return Open<T>(stream, new SvgOptions(entities));
	}

	public static T Open<T>(Stream stream, SvgOptions svgOptions) where T : SvgDocument, new()
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return Create<T>(new SvgTextReader(stream, svgOptions.Entities)
		{
			XmlResolver = new SvgDtdResolver(),
			WhitespaceHandling = WhitespaceHandling.Significant,
			DtdProcessing = (DisableDtdProcessing ? DtdProcessing.Ignore : DtdProcessing.Parse)
		}, svgOptions.Css);
	}

	public static T FromSvg<T>(string svg) where T : SvgDocument, new()
	{
		if (string.IsNullOrEmpty(svg))
		{
			throw new ArgumentNullException("svg");
		}
		using StringReader reader = new StringReader(svg);
		return Create<T>(new SvgTextReader(reader, null)
		{
			XmlResolver = new SvgDtdResolver(),
			WhitespaceHandling = WhitespaceHandling.Significant,
			DtdProcessing = (DisableDtdProcessing ? DtdProcessing.Ignore : DtdProcessing.Parse)
		});
	}

	public static T Open<T>(XmlReader reader) where T : SvgDocument, new()
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		using XmlReader reader2 = XmlReader.Create(reader, new XmlReaderSettings
		{
			XmlResolver = new SvgDtdResolver(),
			DtdProcessing = DtdProcessing.Parse
		});
		return Create<T>(reader2);
	}

	private static T Create<T>(XmlReader reader, string css = null) where T : SvgDocument, new()
	{
		List<ISvgNode> list = new List<ISvgNode>();
		SvgElementFactory elementFactory = new SvgElementFactory();
		T val = Create<T>(reader, elementFactory, list);
		if (css != null)
		{
			list.Add(new SvgUnknownElement
			{
				Content = css
			});
		}
		if (list.Any())
		{
			string content = string.Join(Environment.NewLine, list.Select((ISvgNode s) => s.Content).ToArray());
			foreach (IStyleRule styleRule in new StylesheetParser(includeUnknownRules: true, includeUnknownDeclarations: true, tolerateInvalidSelectors: false, tolerateInvalidValues: true).Parse(content).StyleRules)
			{
				try
				{
					foreach (SvgElement item in new NonSvgElement
					{
						Children = { (SvgElement)val }
					}.QuerySelectorAll(styleRule.Selector, elementFactory))
					{
						foreach (IProperty item2 in styleRule.Style)
						{
							item.AddStyle(item2.Name, item2.Original, styleRule.Selector.GetSpecificity());
						}
					}
				}
				catch (Exception ex)
				{
					Trace.TraceWarning(ex.Message);
				}
			}
		}
		val?.FlushStyles(children: true);
		return val;
	}

	internal static T Create<T>(XmlReader reader, SvgElementFactory elementFactory, List<ISvgNode> styles) where T : SvgDocument, new()
	{
		if (!SkipGdiPlusCapabilityCheck)
		{
			EnsureSystemIsGdiPlusCapable();
		}
		Stack<SvgElement> stack = new Stack<SvgElement>();
		SvgElement svgElement = null;
		T val = null;
		while (reader.Read())
		{
			try
			{
				switch (reader.NodeType)
				{
				case XmlNodeType.Element:
				{
					bool isEmptyElement = reader.IsEmptyElement;
					if (stack.Count > 0)
					{
						svgElement = elementFactory.CreateElement(reader, val);
					}
					else
					{
						val = elementFactory.CreateDocument<T>(reader);
						svgElement = val;
					}
					if (stack.Count > 0)
					{
						SvgElement svgElement2 = stack.Peek();
						if (svgElement2 != null && svgElement != null)
						{
							svgElement2.Children.Add(svgElement);
							svgElement2.Nodes.Add(svgElement);
						}
					}
					stack.Push(svgElement);
					if (!isEmptyElement)
					{
						break;
					}
					goto case XmlNodeType.EndElement;
				}
				case XmlNodeType.EndElement:
					svgElement = stack.Pop();
					if (svgElement.Nodes.OfType<SvgContentNode>().Any())
					{
						svgElement.Content = string.Concat(svgElement.Nodes.Select((ISvgNode n) => n.Content).ToArray());
					}
					else
					{
						svgElement.Nodes.Clear();
					}
					if (svgElement is SvgUnknownElement { ElementName: "style" } svgUnknownElement)
					{
						styles.Add(svgUnknownElement);
					}
					break;
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
					svgElement = stack.Peek();
					svgElement.Nodes.Add(new SvgContentNode
					{
						Content = reader.Value
					});
					break;
				case XmlNodeType.EntityReference:
					reader.ResolveEntity();
					svgElement = stack.Peek();
					svgElement.Nodes.Add(new SvgContentNode
					{
						Content = reader.Value
					});
					break;
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
		}
		return val;
	}

	public static SvgDocument Open(XmlDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		return Create<SvgDocument>(new SvgNodeReader(document.DocumentElement, null));
	}

	public virtual void RasterizeDimensions(ref SizeF size, int rasterWidth, int rasterHeight)
	{
		if (size.Width != 0f)
		{
			float num = size.Height / size.Width;
			size.Width = ((rasterWidth > 0) ? ((float)rasterWidth) : size.Width);
			size.Height = ((rasterHeight > 0) ? ((float)rasterHeight) : size.Height);
			if (rasterHeight == 0 && rasterWidth > 0)
			{
				size.Height = (int)((float)rasterWidth * num);
			}
			else if (rasterHeight > 0 && rasterWidth == 0)
			{
				size.Width = (int)((float)rasterHeight / num);
			}
		}
	}

	public override void Write(XmlWriter writer)
	{
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			base.Write(writer);
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = currentCulture;
		}
	}

	public void Write(Stream stream, bool useBom = true)
	{
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Encoding = (useBom ? Encoding.UTF8 : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)),
			Indent = true
		};
		using XmlWriter xmlWriter = XmlWriter.Create(stream, settings);
		xmlWriter.WriteStartDocument();
		xmlWriter.WriteDocType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null);
		if (!string.IsNullOrEmpty(ExternalCSSHref))
		{
			xmlWriter.WriteProcessingInstruction("xml-stylesheet", $"type=\"text/css\" href=\"{ExternalCSSHref}\"");
		}
		Write(xmlWriter);
		xmlWriter.Flush();
	}

	public void Write(string path, bool useBom = true)
	{
		using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
		Write(stream, useBom);
	}

	protected override void WriteAttributes(XmlWriter writer)
	{
		writer.WriteAttributeString("version", "1.1");
		base.WriteAttributes(writer);
	}

	public static bool SystemIsGdiPlusCapable()
	{
		try
		{
			EnsureSystemIsGdiPlusCapable();
		}
		catch (SvgGdiPlusCannotBeLoadedException)
		{
			return false;
		}
		catch (Exception)
		{
			throw;
		}
		return true;
	}

	public static void EnsureSystemIsGdiPlusCapable()
	{
		try
		{
			using (new Matrix(0f, 0f, 0f, 0f, 0f, 0f))
			{
			}
		}
		catch (Exception ex)
		{
			if (ExceptionCaughtIsGdiPlusRelated(ex))
			{
				throw new SvgGdiPlusCannotBeLoadedException(ex);
			}
			throw;
		}
	}

	private static bool ExceptionCaughtIsGdiPlusRelated(Exception e)
	{
		Exception ex = e;
		int num = 0;
		while (ex != null && num < 10)
		{
			DllNotFoundException obj = ex as DllNotFoundException;
			if (obj != null && obj.Message?.LastIndexOf("libgdiplus", StringComparison.OrdinalIgnoreCase) > -1)
			{
				return true;
			}
			ex = ex.InnerException;
			num++;
		}
		return false;
	}

	public static Bitmap OpenAsBitmap(string path)
	{
		return null;
	}

	public static Bitmap OpenAsBitmap(XmlDocument document)
	{
		return null;
	}

	private void Draw(ISvgRenderer renderer, ISvgBoundable boundable)
	{
		SvgFontManager svgFontManager = (FontManager = new SvgFontManager());
		using (svgFontManager)
		{
			renderer.SetBoundable(boundable);
			Render(renderer);
			FontManager = null;
		}
	}

	public void Draw(ISvgRenderer renderer)
	{
		if (renderer == null)
		{
			throw new ArgumentNullException("renderer");
		}
		Draw(renderer, this);
	}

	public void Draw(Graphics graphics)
	{
		Draw(graphics, null);
	}

	public void Draw(Graphics graphics, SizeF? size)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		using ISvgRenderer renderer = SvgRenderer.FromGraphics(graphics);
		SizeF sizeF = size ?? GetDimensions(renderer);
		GenericBoundable boundable = new GenericBoundable(0f, 0f, sizeF.Width, sizeF.Height);
		Draw(renderer, boundable);
	}

	public virtual Bitmap Draw()
	{
		Size size = Size.Round(GetDimensions());
		if (size.Width <= 0 || size.Height <= 0)
		{
			return null;
		}
		Bitmap bitmap = null;
		try
		{
			try
			{
				bitmap = new Bitmap(size.Width, size.Height);
			}
			catch (ArgumentException inner)
			{
				throw new SvgMemoryException("Cannot process SVG file, cannot allocate the required memory", inner);
			}
			Draw(bitmap);
			return bitmap;
		}
		catch
		{
			bitmap?.Dispose();
			throw;
		}
	}

	public virtual void Draw(Bitmap bitmap)
	{
		using ISvgRenderer renderer = SvgRenderer.FromImage(bitmap);
		GenericBoundable boundable = new GenericBoundable(0f, 0f, bitmap.Width, bitmap.Height);
		Draw(renderer, boundable);
	}

	public virtual Bitmap Draw(int rasterWidth, int rasterHeight)
	{
		SizeF dimensions = GetDimensions();
		SizeF size = dimensions;
		RasterizeDimensions(ref size, rasterWidth, rasterHeight);
		Size size2 = Size.Round(size);
		if (size2.Width <= 0 || size2.Height <= 0)
		{
			return null;
		}
		Bitmap bitmap = null;
		try
		{
			try
			{
				bitmap = new Bitmap(size2.Width, size2.Height);
			}
			catch (ArgumentException inner)
			{
				throw new SvgMemoryException("Cannot process SVG file, cannot allocate the required memory", inner);
			}
			using ISvgRenderer svgRenderer = SvgRenderer.FromImage(bitmap);
			svgRenderer.ScaleTransform(size.Width / dimensions.Width, size.Height / dimensions.Height);
			GenericBoundable boundable = new GenericBoundable(0f, 0f, dimensions.Width, dimensions.Height);
			Draw(svgRenderer, boundable);
			return bitmap;
		}
		catch
		{
			bitmap?.Dispose();
			throw;
		}
	}

	[DllImport("gdi32.dll")]
	private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

	[DllImport("user32.dll")]
	private static extern IntPtr GetDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	private static int GetWin32SystemDpi()
	{
		IntPtr dC = GetDC(IntPtr.Zero);
		int deviceCaps = GetDeviceCaps(dC, 90);
		ReleaseDC(IntPtr.Zero, dC);
		return deviceCaps;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgDocumentProperty in SvgDocumentProperties)
		{
			yield return svgDocumentProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgDocumentProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgDocumentProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgDocumentProperties.TryGetValue(attributeName, out var value2))
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
