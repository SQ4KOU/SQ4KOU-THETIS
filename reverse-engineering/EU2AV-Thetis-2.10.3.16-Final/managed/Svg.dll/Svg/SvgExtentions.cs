using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace Svg;

public static class SvgExtentions
{
	public static void SetRectangle(this SvgRectangle r, RectangleF bounds)
	{
		r.X = bounds.X;
		r.Y = bounds.Y;
		r.Width = bounds.Width;
		r.Height = bounds.Height;
	}

	public static RectangleF GetRectangle(this SvgRectangle r)
	{
		return new RectangleF(r.X, r.Y, r.Width, r.Height);
	}

	public static string GetXML(this SvgDocument doc)
	{
		string empty = string.Empty;
		using MemoryStream memoryStream = new MemoryStream();
		doc.Write(memoryStream);
		memoryStream.Position = 0L;
		using StreamReader streamReader = new StreamReader(memoryStream);
		return streamReader.ReadToEnd();
	}

	public static string GetXML(this SvgElement elem)
	{
		string empty = string.Empty;
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8
			};
			using StringWriter stringWriter = new StringWriter();
			using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings);
			elem.Write(xmlWriter);
			xmlWriter.Flush();
			return stringWriter.ToString();
		}
		finally
		{
			Thread.CurrentThread.CurrentCulture = currentCulture;
		}
	}

	public static bool HasNonEmptyCustomAttribute(this SvgElement element, string name)
	{
		if (element.CustomAttributes.ContainsKey(name))
		{
			return !string.IsNullOrEmpty(element.CustomAttributes[name]);
		}
		return false;
	}

	public static void ApplyRecursive(this SvgElement elem, Action<SvgElement> action)
	{
		foreach (SvgElement item in elem.Traverse((SvgElement e) => e.Children))
		{
			action(item);
		}
	}

	public static void ApplyRecursiveDepthFirst(this SvgElement elem, Action<SvgElement> action)
	{
		foreach (SvgElement item in elem.TraverseDepthFirst((SvgElement e) => e.Children))
		{
			action(item);
		}
	}

	internal static IEnumerable<T> Traverse<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childrenSelector)
	{
		if (childrenSelector == null)
		{
			throw new ArgumentNullException("childrenSelector");
		}
		Queue<T> itemQueue = new Queue<T>(items);
		while (itemQueue.Count > 0)
		{
			T current = itemQueue.Dequeue();
			yield return current;
			foreach (T item in childrenSelector(current) ?? Enumerable.Empty<T>())
			{
				itemQueue.Enqueue(item);
			}
		}
	}

	internal static IEnumerable<T> Traverse<T>(this T root, Func<T, IEnumerable<T>> childrenSelector)
	{
		return Enumerable.Repeat(root, 1).Traverse(childrenSelector);
	}

	internal static IEnumerable<T> TraverseDepthFirst<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> childrenSelector)
	{
		if (childrenSelector == null)
		{
			throw new ArgumentNullException("childrenSelector");
		}
		Stack<T> itemStack = new Stack<T>(items ?? Enumerable.Empty<T>());
		while (itemStack.Count > 0)
		{
			T current = itemStack.Pop();
			yield return current;
			foreach (T item in childrenSelector(current) ?? Enumerable.Empty<T>())
			{
				itemStack.Push(item);
			}
		}
	}

	internal static IEnumerable<T> TraverseDepthFirst<T>(this T root, Func<T, IEnumerable<T>> childrenSelector)
	{
		return Enumerable.Repeat(root, 1).TraverseDepthFirst(childrenSelector);
	}
}
