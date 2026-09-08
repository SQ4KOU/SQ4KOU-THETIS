using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Markdig.Helpers;

namespace Markdig.Extensions.MediaLinks;

public class HostProviderBuilder
{
	private sealed class DelegateProvider(string hostPrefix, Func<Uri, string?> handler, bool allowFullscreen = true, string? className = null) : IHostProvider
	{
		public string HostPrefix { get; } = hostPrefix;

		public Func<Uri, string?> Delegate { get; } = handler;

		public bool AllowFullScreen { get; } = allowFullscreen;

		public string? Class { get; } = className;

		public bool TryHandle(Uri mediaUri, bool isSchemaRelative, [NotNullWhen(true)] out string? iframeUrl)
		{
			if (!mediaUri.Host.StartsWith(HostPrefix, StringComparison.OrdinalIgnoreCase))
			{
				iframeUrl = null;
				return false;
			}
			iframeUrl = Delegate(mediaUri);
			return !string.IsNullOrEmpty(iframeUrl);
		}
	}

	internal static readonly IHostProvider[] KnownHosts = new IHostProvider[6]
	{
		Create("www.youtube.com", YouTubeShort, allowFullScreen: true, "youtubeshort"),
		Create("www.youtube.com", YouTube, allowFullScreen: true, "youtube"),
		Create("youtu.be", YouTubeShortened, allowFullScreen: true, "youtube"),
		Create("vimeo.com", Vimeo, allowFullScreen: true, "vimeo"),
		Create("music.yandex.ru", Yandex, allowFullScreen: false, "yandex"),
		Create("ok.ru", Odnoklassniki, allowFullScreen: true, "odnoklassniki")
	};

	private static readonly string[] SplitAnd = new string[1] { "&" };

	public static IHostProvider Create(string hostPrefix, Func<Uri, string?> handler, bool allowFullScreen = true, string? iframeClass = null)
	{
		if (string.IsNullOrEmpty(hostPrefix))
		{
			ThrowHelper.ArgumentException("hostPrefix is null or empty.", "hostPrefix");
		}
		if (handler == null)
		{
			ThrowHelper.ArgumentNullException("handler");
		}
		return new DelegateProvider(hostPrefix, handler, allowFullScreen, iframeClass);
	}

	private static string[] SplitQuery(Uri uri)
	{
		return uri.Query.Substring(uri.Query.IndexOf('?') + 1).Split(SplitAnd, StringSplitOptions.RemoveEmptyEntries);
	}

	private static string? YouTube(Uri uri)
	{
		string absolutePath = uri.AbsolutePath;
		if (string.Equals(absolutePath, "/embed", StringComparison.OrdinalIgnoreCase) || absolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
		{
			return uri.ToString();
		}
		if (!string.Equals(absolutePath, "/watch", StringComparison.OrdinalIgnoreCase) && !absolutePath.StartsWith("/watch/", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		string[] source = SplitQuery(uri);
		return BuildYouTubeIframeUrl(source.FirstOrDefault((string p) => p.StartsWith("v=", StringComparison.Ordinal))?.Substring(2), source.FirstOrDefault((string p) => p.StartsWith("t=", StringComparison.Ordinal))?.Substring(2));
	}

	private static string? YouTubeShort(Uri uri)
	{
		string absolutePath = uri.AbsolutePath;
		if (!absolutePath.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		return BuildYouTubeIframeUrl(absolutePath.Substring("/shorts/".Length).Split('?').FirstOrDefault(), null);
	}

	private static string? YouTubeShortened(Uri uri)
	{
		return BuildYouTubeIframeUrl(uri.AbsolutePath.Substring(1), SplitQuery(uri).FirstOrDefault((string p) => p.StartsWith("t=", StringComparison.Ordinal))?.Substring(2));
	}

	private static string? BuildYouTubeIframeUrl(string? videoId, string? startTime)
	{
		if (string.IsNullOrEmpty(videoId))
		{
			return null;
		}
		string text = "https://www.youtube.com/embed/" + videoId;
		if (!string.IsNullOrEmpty(startTime))
		{
			return text + "?start=" + startTime;
		}
		return text;
	}

	private static string? Vimeo(Uri uri)
	{
		string[] array = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Split('/');
		if (array.Length == 0)
		{
			return null;
		}
		return "https://player.vimeo.com/video/" + array[array.Length - 1];
	}

	private static string? Odnoklassniki(Uri uri)
	{
		string[] array = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Split('/');
		if (array.Length == 0)
		{
			return null;
		}
		return "https://ok.ru/videoembed/" + array[array.Length - 1];
	}

	private static string? Yandex(Uri uri)
	{
		string[] source = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Split('/');
		string text = source.Skip(0).FirstOrDefault();
		string text2 = source.Skip(1).FirstOrDefault();
		string text3 = source.Skip(2).FirstOrDefault();
		string text4 = source.Skip(3).FirstOrDefault();
		if (text != "album" || text2 == null || text3 != "track" || text4 == null)
		{
			return null;
		}
		return "https://music.yandex.ru/iframe/#track/" + text4 + "/" + text2 + "/";
	}
}
