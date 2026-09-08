#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace Svg;

public class SvgElementIdManager
{
	private static readonly HttpClient _httpClient = new HttpClient();

	private SvgDocument _document;

	private Dictionary<string, SvgElement> _idValueMap;

	private static readonly Regex regex = new Regex("#\\d+$");

	internal ExternalType ResolveExternalElements => SvgDocument.ResolveExternalElements;

	public event EventHandler<SvgElementEventArgs> ElementAdded;

	public event EventHandler<SvgElementEventArgs> ElementRemoved;

	public virtual SvgElement GetElementById(string id)
	{
		id = GetUrlString(id);
		if (id.StartsWith("#"))
		{
			id = id.Substring(1);
		}
		SvgElement value = null;
		_idValueMap.TryGetValue(id, out value);
		return value;
	}

	public virtual SvgElement GetElementById(Uri uri)
	{
		string urlString = GetUrlString(uri.ToString());
		if (!urlString.StartsWith("#"))
		{
			int startIndex = urlString.LastIndexOf('#');
			string text = urlString.Substring(startIndex);
			uri = new Uri(urlString.Remove(startIndex, text.Length), UriKind.RelativeOrAbsolute);
			if (!uri.IsAbsoluteUri && _document.BaseUri != null)
			{
				uri = new Uri(_document.BaseUri, uri);
			}
			if (!ResolveExternalElements.AllowsResolving(uri))
			{
				Trace.TraceWarning("Trying to resolve element by ID from '{0}', but resolving external resources of that type is disabled.", uri);
				return null;
			}
			if (uri.IsAbsoluteUri)
			{
				if (uri.IsFile)
				{
					return SvgDocument.Open<SvgDocument>(uri.LocalPath).IdManager.GetElementById(text);
				}
				if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
				{
					using (HttpResponseMessage httpResponseMessage = _httpClient.GetAsync(uri).Result)
					{
						using Stream stream = httpResponseMessage.Content.ReadAsStreamAsync().Result;
						return SvgDocument.Open<SvgDocument>(stream).IdManager.GetElementById(text);
					}
				}
				throw new NotSupportedException();
			}
		}
		return GetElementById(urlString);
	}

	private static string GetUrlString(string url)
	{
		url = url.Trim();
		if (url.StartsWith("url(", StringComparison.OrdinalIgnoreCase) && url.EndsWith(")"))
		{
			url = new StringBuilder(url).Remove(url.Length - 1, 1).Remove(0, 4).ToString()
				.Trim();
			if ((url.StartsWith("\"") && url.EndsWith("\"")) || (url.StartsWith("'") && url.EndsWith("'")))
			{
				url = new StringBuilder(url).Remove(url.Length - 1, 1).Remove(0, 1).ToString()
					.Trim();
			}
		}
		return url;
	}

	public virtual void Add(SvgElement element)
	{
		AddAndForceUniqueID(element, null, autoForceUniqueID: false);
	}

	public virtual bool AddAndForceUniqueID(SvgElement element, SvgElement sibling, bool autoForceUniqueID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
	{
		bool result = false;
		if (!string.IsNullOrEmpty(element.ID))
		{
			string text = EnsureValidId(element.ID, autoForceUniqueID);
			if (autoForceUniqueID && text != element.ID)
			{
				logElementOldIDNewID?.Invoke(element, element.ID, text);
				element.ForceUniqueID(text);
				result = true;
			}
			_idValueMap.Add(element.ID, element);
		}
		OnAdded(element);
		return result;
	}

	public virtual void Remove(SvgElement element)
	{
		if (!string.IsNullOrEmpty(element.ID))
		{
			_idValueMap.Remove(element.ID);
		}
		OnRemoved(element);
	}

	public string EnsureValidId(string id, bool autoForceUniqueID = false)
	{
		if (string.IsNullOrEmpty(id))
		{
			return id;
		}
		if (_idValueMap.ContainsKey(id))
		{
			if (autoForceUniqueID)
			{
				Match match = regex.Match(id);
				id = ((!match.Success || !int.TryParse(match.Value.Substring(1), out var result)) ? (id + "#1") : regex.Replace(id, "#" + (result + 1)));
				return EnsureValidId(id, autoForceUniqueID: true);
			}
			throw new SvgIDExistsException("An element with the same ID already exists: '" + id + "'.");
		}
		return id;
	}

	public SvgElementIdManager(SvgDocument document)
	{
		_document = document;
		_idValueMap = new Dictionary<string, SvgElement>();
	}

	protected void OnAdded(SvgElement element)
	{
		ElementAdded?.Invoke(_document, new SvgElementEventArgs
		{
			Element = element
		});
	}

	protected void OnRemoved(SvgElement element)
	{
		ElementRemoved?.Invoke(_document, new SvgElementEventArgs
		{
			Element = element
		});
	}
}
