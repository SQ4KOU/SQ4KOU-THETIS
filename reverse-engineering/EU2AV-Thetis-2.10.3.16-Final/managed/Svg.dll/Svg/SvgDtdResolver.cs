#define TRACE
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Svg;

internal class SvgDtdResolver : XmlUrlResolver
{
	private static readonly Regex _svgDtdRegex = new Regex("(?:SVG[0-9]+\\.DTD)|(?:DTD SVG [0-9\\.]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	internal ExternalType ResolveExternalXmlEntities => SvgDocument.ResolveExternalXmlEntites;

	public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
	{
		if (IsSvgDtdEntity(absoluteUri))
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("Svg.Resources.svg11.dtd");
		}
		if (ResolveExternalXmlEntities.AllowsResolving(absoluteUri))
		{
			return base.GetEntity(absoluteUri, role, ofObjectToReturn);
		}
		Trace.TraceWarning("Trying to resolve entity from '{0}', but resolving external entities of that type is disabled.", absoluteUri);
		return new MemoryStream();
	}

	private static bool IsSvgDtdEntity(Uri absoluteUri)
	{
		return _svgDtdRegex.IsMatch(absoluteUri.ToString());
	}
}
