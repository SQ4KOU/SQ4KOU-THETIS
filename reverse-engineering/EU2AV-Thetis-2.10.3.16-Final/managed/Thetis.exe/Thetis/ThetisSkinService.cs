using System;
using System.Drawing;
using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thetis;

public static class ThetisSkinService
{
	private static CancellationTokenSource _downloadCancellationTokenSource;

	private static string _version;

	public static string Version
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

	public static event EventHandler<SkinsData> ThetisSkinsData;

	public static event EventHandler<SkinServersData> ThetisSkinServerData;

	public static event EventHandler<SkinHttpImage> ImageLoaded;

	public static event EventHandler<SkinFileDownload> FileDownload;

	public static async void GetThetisSkinsData(string jsonUrl)
	{
		WebRequestHandler wrh = null;
		HttpClient client = null;
		try
		{
			HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
			wrh = new WebRequestHandler
			{
				CachePolicy = cachePolicy,
				UseCookies = false
			};
			client = new HttpClient(wrh);
			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
			{
				NoCache = true
			};
			SkinsData e = JsonConvert.DeserializeObject<SkinsData>(await client.GetStringAsync(jsonUrl + "?timestamp=" + DateTime.UtcNow.Ticks));
			ThetisSkinsData?.Invoke(null, e);
			client.Dispose();
			client = null;
			wrh.Dispose();
			wrh = null;
		}
		catch (Exception)
		{
			ThetisSkinsData?.Invoke(null, null);
		}
		finally
		{
			client?.Dispose();
			wrh?.Dispose();
		}
	}

	public static async void GetSkinServers(string jsonUrl)
	{
		WebRequestHandler wrh = null;
		HttpClient client = null;
		try
		{
			HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
			wrh = new WebRequestHandler
			{
				CachePolicy = cachePolicy,
				UseCookies = false
			};
			client = new HttpClient(wrh);
			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
			{
				NoCache = true
			};
			client.Timeout = TimeSpan.FromSeconds(10.0);
			SkinServersData e = JsonConvert.DeserializeObject<SkinServersData>(await client.GetStringAsync(jsonUrl + "?timestamp=" + DateTime.UtcNow.Ticks));
			ThetisSkinServerData?.Invoke(null, e);
		}
		catch (Exception)
		{
			ThetisSkinServerData?.Invoke(null, null);
		}
		finally
		{
			client?.Dispose();
			wrh?.Dispose();
		}
	}

	public static void SubscribeForSkinData(EventHandler<SkinsData> eventHandler)
	{
		ThetisSkinsData += eventHandler;
	}

	public static void UnsubscribeFromSkinData(EventHandler<SkinsData> eventHandler)
	{
		ThetisSkinsData -= eventHandler;
	}

	public static void SubscribeForSkinServerData(EventHandler<SkinServersData> eventHandler)
	{
		ThetisSkinServerData += eventHandler;
	}

	public static void UnsubscribeFromSkinServerData(EventHandler<SkinServersData> eventHandler)
	{
		ThetisSkinServerData -= eventHandler;
	}

	public static void SubscribeForImageLoaded(EventHandler<SkinHttpImage> eventHandler)
	{
		ImageLoaded += eventHandler;
	}

	public static void UnsubscribeFromImageLoaded(EventHandler<SkinHttpImage> eventHandler)
	{
		ImageLoaded -= eventHandler;
	}

	public static void SubscribeForDownload(EventHandler<SkinFileDownload> eventHandler)
	{
		FileDownload += eventHandler;
	}

	public static void UnsubscribeFromDownload(EventHandler<SkinFileDownload> eventHandler)
	{
		FileDownload -= eventHandler;
	}

	public static async void LoadImageFromUrl(string imageUrl, string sID)
	{
		WebRequestHandler wrh = null;
		HttpClient client = null;
		try
		{
			HttpRequestCachePolicy cachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
			wrh = new WebRequestHandler
			{
				CachePolicy = cachePolicy,
				UseCookies = false
			};
			client = new HttpClient(wrh);
			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
			{
				NoCache = true
			};
			using MemoryStream stream = new MemoryStream(await client.GetByteArrayAsync(imageUrl));
			Image image = Image.FromStream(stream);
			SkinHttpImage skinHttpImage = new SkinHttpImage();
			skinHttpImage.Image = image;
			skinHttpImage.ID = sID;
			ImageLoaded?.Invoke(null, skinHttpImage);
		}
		catch (Exception)
		{
			ImageLoaded?.Invoke(null, null);
		}
		finally
		{
			client?.Dispose();
			wrh?.Dispose();
		}
	}

	public static async void DownloadFile(string fileUrl, string savePath, bool bypassFolderCheck, bool isMeterSkin)
	{
		if (_downloadCancellationTokenSource != null)
		{
			return;
		}
		_downloadCancellationTokenSource = new CancellationTokenSource();
		try
		{
			using HttpClient httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Add("User-Agent", "Thetis v" + _version);
			using HttpResponseMessage response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, _downloadCancellationTokenSource.Token);
			using Stream contentStream = await response.Content.ReadAsStreamAsync();
			if (response.IsSuccessStatusCode)
			{
				SkinFileDownload sfd = new SkinFileDownload();
				long totalBytes = response.Content.Headers.ContentLength ?? (-1);
				long downloadedBytes = 0L;
				sfd.BypassRootFolderCheck = bypassFolderCheck;
				sfd.IsMeterSkin = isMeterSkin;
				sfd.Url = fileUrl;
				sfd.Path = savePath;
				sfd.TotalBytes = totalBytes;
				sfd.Complete = false;
				sfd.PercentageDownloaded = 0;
				if (response.RequestMessage != null)
				{
					Uri requestUri = response.RequestMessage.RequestUri;
					sfd.FinalUri = requestUri.ToString();
				}
				FileDownload?.Invoke(null, sfd);
				int num = 4096;
				byte[] buffer = new byte[num];
				using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, num, useAsync: true))
				{
					while (true)
					{
						int num2;
						int bytesRead = (num2 = await contentStream.ReadAsync(buffer, 0, buffer.Length, _downloadCancellationTokenSource.Token));
						if (num2 <= 0)
						{
							break;
						}
						await fileStream.WriteAsync(buffer, 0, bytesRead, _downloadCancellationTokenSource.Token);
						downloadedBytes = (sfd.BytesDownloaded = downloadedBytes + bytesRead);
						if (totalBytes != -1)
						{
							float num4 = (float)downloadedBytes / (float)totalBytes * 100f;
							if ((int)num4 != sfd.PercentageDownloaded)
							{
								sfd.PercentageDownloaded = (int)num4;
								FileDownload?.Invoke(null, sfd);
							}
						}
					}
				}
				sfd.BytesDownloaded = downloadedBytes;
				sfd.PercentageDownloaded = 100;
				sfd.Complete = true;
				sfd.Cancelled = false;
				FileDownload?.Invoke(null, sfd);
			}
			else
			{
				FileDownload?.Invoke(null, null);
			}
		}
		catch (TaskCanceledException)
		{
			SkinFileDownload skinFileDownload = new SkinFileDownload();
			skinFileDownload.Complete = false;
			skinFileDownload.Cancelled = true;
			FileDownload?.Invoke(null, skinFileDownload);
		}
		catch (Exception)
		{
			FileDownload?.Invoke(null, null);
		}
		_downloadCancellationTokenSource = null;
	}

	public static void CancelDownload()
	{
		if (_downloadCancellationTokenSource != null)
		{
			_downloadCancellationTokenSource.Cancel();
		}
	}
}
