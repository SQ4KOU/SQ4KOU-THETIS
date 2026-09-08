using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using SkiaSharp;
using Svg;

namespace Thetis;

public class ImageFetcher
{
	public enum State
	{
		OK = 0,
		ERROR_NO_SUITABLE_IMAGE = 1,
		ERROR_URL_ISSUE = 2,
		ERROR_IMAGE_CONVERSION_PROBLEM = 3,
		WAITING = 4,
		GATHERING_IMAGES = 5,
		IDLE = 99
	}

	public class StateEventArgs : EventArgs
	{
		public Guid Guid { get; }

		public State WebImageState { get; }

		public StateEventArgs(Guid guid, State state)
		{
			Guid = guid;
			WebImageState = state;
		}
	}

	private class ImageStore
	{
		private readonly int _image_limit;

		private readonly Queue<Image> _images;

		private readonly object _lock_object = new object();

		public ImageStore(int image_limit)
		{
			_image_limit = image_limit;
			_images = new Queue<Image>();
		}

		public bool AddImage(Image image)
		{
			bool result = false;
			lock (_lock_object)
			{
				if (_images.Count >= _image_limit)
				{
					_images.Dequeue().Dispose();
					result = true;
				}
				_images.Enqueue(image);
				return result;
			}
		}

		public List<Image> GetImages()
		{
			lock (_lock_object)
			{
				return new List<Image>(_images);
			}
		}

		public void ClearImages()
		{
			lock (_lock_object)
			{
				while (_images.Count > 0)
				{
					_images.Dequeue().Dispose();
				}
			}
		}
	}

	private readonly ConcurrentDictionary<Guid, ImageStore> _image_stores;

	private readonly ConcurrentDictionary<Guid, Thread> _threads;

	private readonly ConcurrentDictionary<Guid, ManualResetEvent> _reset_events;

	private readonly ConcurrentDictionary<Guid, int> _timeouts;

	private readonly ConcurrentDictionary<Guid, bool> _bypass_cache;

	private string _version;

	public string Version
	{
		get
		{
			return _version;
		}
		set
		{
			_version = value;
		}
	}

	public event EventHandler<Guid> ImagesObtained;

	public event EventHandler<StateEventArgs> StateChanged;

	public ImageFetcher()
	{
		_version = "";
		_image_stores = new ConcurrentDictionary<Guid, ImageStore>();
		_threads = new ConcurrentDictionary<Guid, Thread>();
		_reset_events = new ConcurrentDictionary<Guid, ManualResetEvent>();
		_timeouts = new ConcurrentDictionary<Guid, int>();
		_bypass_cache = new ConcurrentDictionary<Guid, bool>();
	}

	public Guid RegisterURL(string url, int timeout_secs, int image_limit, bool file, bool bypass_cache = false)
	{
		Guid id = Guid.NewGuid();
		ImageStore store = new ImageStore(image_limit);
		ManualResetEvent reset_event = new ManualResetEvent(initialState: false);
		Thread thread = new Thread((ThreadStart)delegate
		{
			fetch_images(url, store, reset_event, id, file);
		});
		if (_image_stores.TryAdd(id, store) && _threads.TryAdd(id, thread) && _reset_events.TryAdd(id, reset_event) && _timeouts.TryAdd(id, timeout_secs) && _bypass_cache.TryAdd(id, bypass_cache))
		{
			thread.Start();
			return id;
		}
		StopFetching(id);
		return Guid.Empty;
	}

	public List<Image> LatestImages(Guid id)
	{
		if (_image_stores.TryGetValue(id, out var value))
		{
			return value.GetImages();
		}
		return new List<Image>();
	}

	public void UpdateInterval(Guid id, int interval)
	{
		if (_timeouts.TryGetValue(id, out var value) && value != interval)
		{
			_timeouts[id] = interval;
			if (_reset_events.TryGetValue(id, out var value2))
			{
				value2.Set();
			}
		}
	}

	public void UpdateBypassCache(Guid id, bool bypass)
	{
		if (_bypass_cache.TryGetValue(id, out var value) && value != bypass)
		{
			_bypass_cache[id] = bypass;
			if (_reset_events.TryGetValue(id, out var value2))
			{
				value2.Set();
			}
		}
	}

	private void clearAllImages()
	{
		foreach (ImageStore value in _image_stores.Values)
		{
			value.ClearImages();
		}
	}

	private void cleanupResources(Guid id)
	{
		if (_image_stores.TryRemove(id, out var value))
		{
			value.ClearImages();
		}
		_reset_events.TryRemove(id, out var _);
		_threads.TryRemove(id, out var _);
		_timeouts.TryRemove(id, out var _);
		_bypass_cache.TryRemove(id, out var _);
	}

	public void StopFetching(Guid id)
	{
		if (_reset_events.TryGetValue(id, out var value))
		{
			value.Set();
			cleanupResources(id);
		}
	}

	public void Shutdown()
	{
		List<Guid> list = new List<Guid>();
		foreach (KeyValuePair<Guid, ManualResetEvent> reset_event in _reset_events)
		{
			list.Add(reset_event.Key);
		}
		foreach (Guid item in list)
		{
			StopFetching(item);
		}
	}

	private void fetch_images(string url, ImageStore store, ManualResetEvent reset_event, Guid id, bool file)
	{
		try
		{
			if (!_timeouts.TryGetValue(id, out var value) || !_bypass_cache.TryGetValue(id, out var value2))
			{
				return;
			}
			int num = _timeouts[id];
			bool flag = _bypass_cache[id];
			reset_event.Reset();
			while (true)
			{
				bool imagesAdded = false;
				try
				{
					StateChanged?.Invoke(this, new StateEventArgs(id, State.IDLE));
					if (!file)
					{
						string requestUriString = url;
						if (flag)
						{
							string text = Guid.NewGuid().ToString();
							requestUriString = ((!url.Contains("?")) ? (url + "?thetis_id=" + text) : (url + "&thetis_id=" + text));
						}
						HttpWebRequest obj = (HttpWebRequest)WebRequest.Create(requestUriString);
						obj.UserAgent = "Thetis v" + _version;
						obj.Timeout = 2000;
						obj.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
						using HttpWebResponse httpWebResponse = (HttpWebResponse)obj.GetResponse();
						string text2 = httpWebResponse.ContentType;
						if (string.IsNullOrEmpty(text2))
						{
							if (url.EndsWith(".html"))
							{
								text2 = "text/html";
							}
							else if (url.EndsWith(".jpg") || url.EndsWith(".gif") || url.EndsWith(".png") || url.EndsWith(".webp") || url.EndsWith(".jpeg") || url.EndsWith(".bmp") || url.EndsWith(".tif") || url.EndsWith(".tiff"))
							{
								text2 = "image";
							}
							else if (url.EndsWith(".svgz") || url.EndsWith(".svg"))
							{
								text2 = "image/svg+xml";
							}
						}
						if (text2.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
						{
							StateChanged?.Invoke(this, new StateEventArgs(id, State.GATHERING_IMAGES));
							using Stream stream = httpWebResponse.GetResponseStream();
							using StreamReader streamReader = new StreamReader(stream);
							string html = streamReader.ReadToEnd();
							foreach (string item in ExtractImageUrls(html, url))
							{
								try
								{
									using WebClient webClient = new WebClient();
									using MemoryStream stream2 = new MemoryStream(webClient.DownloadData(item));
									Image image = Image.FromStream(stream2);
									bool num2 = store.AddImage(image);
									imagesAdded = true;
									if (num2)
									{
										break;
									}
									StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
									continue;
								}
								catch
								{
									StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
									continue;
								}
							}
						}
						else if (text2.StartsWith("image/svg+xml", StringComparison.OrdinalIgnoreCase))
						{
							StateChanged?.Invoke(this, new StateEventArgs(id, State.GATHERING_IMAGES));
							using Stream stream3 = httpWebResponse.GetResponseStream();
							using MemoryStream memoryStream = new MemoryStream();
							stream3.CopyTo(memoryStream);
							byte[] svgData = memoryStream.ToArray();
							ProcessSvgImage(svgData, store, ref imagesAdded, id);
						}
						else if (text2.StartsWith("image", StringComparison.OrdinalIgnoreCase))
						{
							StateChanged?.Invoke(this, new StateEventArgs(id, State.GATHERING_IMAGES));
							using Stream stream4 = httpWebResponse.GetResponseStream();
							using MemoryStream memoryStream2 = new MemoryStream();
							stream4.CopyTo(memoryStream2);
							memoryStream2.Position = 0L;
							SKBitmap sKBitmap;
							try
							{
								sKBitmap = SKBitmap.Decode(memoryStream2);
							}
							catch (Exception)
							{
								sKBitmap = null;
							}
							if (sKBitmap != null)
							{
								using SKImage sKImage = SKImage.FromBitmap(sKBitmap);
								using SKData sKData = sKImage.Encode(SKEncodedImageFormat.Png, 100);
								using MemoryStream stream5 = new MemoryStream(sKData.ToArray());
								try
								{
									Image image2 = Image.FromStream(stream5);
									store.AddImage(image2);
									imagesAdded = true;
									StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
								}
								catch (Exception)
								{
									StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
								}
							}
							else
							{
								StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
							}
						}
						else if (text2.StartsWith("multipart/x-mixed-replace", StringComparison.OrdinalIgnoreCase))
						{
							string boundary = getBoundary(httpWebResponse.ContentType);
							if (boundary != null)
							{
								ProcessMultipartContent(httpWebResponse.GetResponseStream(), boundary, store, ref imagesAdded, reset_event, id);
							}
						}
					}
					else if (File.Exists(url))
					{
						SKBitmap sKBitmap2 = SKBitmap.Decode(url);
						if (sKBitmap2 != null)
						{
							using SKImage sKImage2 = SKImage.FromBitmap(sKBitmap2);
							using SKData sKData2 = sKImage2.Encode(SKEncodedImageFormat.Png, 100);
							using MemoryStream stream6 = new MemoryStream(sKData2.ToArray());
							try
							{
								Image image3 = Image.FromStream(stream6);
								store.AddImage(image3);
								imagesAdded = true;
								StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
							}
							catch (Exception)
							{
								StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
							}
						}
						else
						{
							StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
						}
					}
					else
					{
						StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_URL_ISSUE));
					}
				}
				catch
				{
					StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_URL_ISSUE));
				}
				if (imagesAdded)
				{
					OnImagesObtained(id);
				}
				StateChanged?.Invoke(this, new StateEventArgs(id, State.WAITING));
				if (reset_event.WaitOne(num * 1000))
				{
					bool num3 = _timeouts.TryGetValue(id, out value);
					bool flag2 = _bypass_cache.TryGetValue(id, out value2);
					if (!num3 || !flag2 || _timeouts[id] == num)
					{
						break;
					}
					num = _timeouts[id];
					flag = _bypass_cache[id];
					reset_event.Reset();
				}
			}
		}
		finally
		{
			StateChanged?.Invoke(this, new StateEventArgs(id, State.IDLE));
			cleanupResources(id);
		}
	}

	private void ProcessMultipartContent(Stream stream, string boundary, ImageStore store, ref bool imagesAdded, ManualResetEvent reset_event, Guid id)
	{
		byte[] bytes = Encoding.UTF8.GetBytes("--" + boundary);
		byte[] array = new byte[8192];
		int num = bytes.Length;
		MemoryStream memoryStream = new MemoryStream();
		bool flag = true;
		StateChanged?.Invoke(this, new StateEventArgs(id, State.GATHERING_IMAGES));
		int count;
		while (flag && (count = stream.Read(array, 0, array.Length)) > 0)
		{
			memoryStream.Write(array, 0, count);
			byte[] array2 = memoryStream.ToArray();
			int num2 = 0;
			while (num2 < array2.Length)
			{
				int num3 = findBoundary(array2, num2, bytes);
				if (num3 < 0)
				{
					memoryStream.Position = 0L;
					memoryStream.Write(array2, num2, array2.Length - num2);
					memoryStream.SetLength(array2.Length - num2);
					break;
				}
				num2 = num3 + num + 2;
				while (num2 < array2.Length && (array2[num2] != 13 || array2[num2 + 1] != 10))
				{
					int num4 = Array.IndexOf(array2, (byte)10, num2);
					if (num4 < 0)
					{
						break;
					}
					num2 = num4 + 1;
				}
				num2 += 2;
				int num5 = findBoundary(array2, num2, bytes);
				if (num5 < 0)
				{
					memoryStream.Position = 0L;
					memoryStream.Write(array2, num3, array2.Length - num3);
					memoryStream.SetLength(array2.Length - num3);
					break;
				}
				int num6 = num5 - num2 - 2;
				if (num6 > 0)
				{
					byte[] array3 = new byte[num6];
					Array.Copy(array2, num2, array3, 0, num6);
					try
					{
						using MemoryStream stream2 = new MemoryStream(array3);
						Image image = Image.FromStream(stream2, useEmbeddedColorManagement: true, validateImageData: true);
						bool num7 = store.AddImage(image);
						imagesAdded = true;
						StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
						if (num7)
						{
							flag = false;
							break;
						}
					}
					catch (Exception)
					{
						StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
						flag = false;
						break;
					}
				}
				num2 = num5;
			}
			if (reset_event.WaitOne(10))
			{
				flag = false;
				break;
			}
		}
	}

	private bool IsSvgImage(byte[] imageData)
	{
		string text = Encoding.UTF8.GetString(imageData);
		if (text.Contains("<svg"))
		{
			return text.Contains("</svg>");
		}
		return false;
	}

	private void ProcessSvgImage(byte[] svgData, ImageStore store, ref bool imagesAdded, Guid id)
	{
		try
		{
			using MemoryStream stream = new MemoryStream(svgData);
			using Bitmap bitmap = SvgDocument.Open<SvgDocument>(stream).Draw();
			using MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, ImageFormat.Png);
			memoryStream.Position = 0L;
			Image image = Image.FromStream(memoryStream);
			store.AddImage(image);
			imagesAdded = true;
			StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
		}
		catch (Exception)
		{
			StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
		}
	}

	private int findBoundary(byte[] content, int start, byte[] boundary)
	{
		for (int i = start; i <= content.Length - boundary.Length; i++)
		{
			bool flag = true;
			for (int j = 0; j < boundary.Length; j++)
			{
				if (content[i + j] != boundary[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	private bool CheckForBoundary(MemoryStream ms, string boundary)
	{
		long position = ms.Position;
		ms.Position -= Math.Min(ms.Length, boundary.Length + 4);
		string text = new StreamReader(ms).ReadToEnd();
		ms.Position = position;
		return text.Contains("--" + boundary);
	}

	private string getBoundary(string contentType)
	{
		string pattern = "boundary=(.*)";
		Match match = Regex.Match(contentType, pattern);
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		return null;
	}

	private List<string> ExtractImageUrls(string html, string baseUrl)
	{
		List<string> list = new List<string>();
		HtmlDocument htmlDocument = new HtmlDocument();
		htmlDocument.LoadHtml(html);
		HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//img[@src]");
		if (htmlNodeCollection != null && htmlNodeCollection.Count > 0)
		{
			foreach (HtmlNode item in htmlNodeCollection)
			{
				string attributeValue = item.GetAttributeValue("src", null);
				if (!string.IsNullOrEmpty(attributeValue))
				{
					Uri uri = new Uri(new Uri(baseUrl), attributeValue);
					list.Add(uri.ToString());
				}
			}
		}
		return list;
	}

	protected virtual void OnImagesObtained(Guid id)
	{
		ImagesObtained?.Invoke(this, id);
	}
}
