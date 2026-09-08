#define TRACE
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Web;

namespace Svg.Web;

public class SvgHandler : IHttpAsyncHandler, IHttpHandler
{
	protected sealed class SvgAsyncRender
	{
		private SvgAsyncRenderState _state;

		public SvgAsyncRender(SvgAsyncRenderState state)
		{
			_state = state;
		}

		private void RenderRawSvg()
		{
			_state._context.Response.ContentType = "image/svg+xml";
			_state._context.Response.WriteFile(_state._context.Request.PhysicalPath);
			_state._context.Response.End();
			_state.CompleteRequest();
		}

		public void RenderSvg()
		{
			_state._context.Response.AddFileDependency(_state._context.Request.PhysicalPath);
			_state._context.Response.Cache.SetLastModifiedFromFileDependencies();
			_state._context.Response.Cache.SetETagFromFileDependencies();
			_state._context.Response.Buffer = false;
			if (_state._context.Request.Browser.Crawler || !string.IsNullOrEmpty(_state._context.Request.QueryString["raw"]))
			{
				RenderRawSvg();
				return;
			}
			try
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				NameValueCollection queryString = _state._context.Request.QueryString;
				for (int i = 0; i < queryString.Count; i++)
				{
					dictionary.Add(queryString.Keys[i], queryString[i]);
				}
				using Bitmap bitmap = SvgDocument.Open<SvgDocument>(_state._context.Request.PhysicalPath, dictionary).Draw();
				using MemoryStream memoryStream = new MemoryStream();
				bitmap?.Save(memoryStream, ImageFormat.Png);
				_state._context.Response.ContentType = "image/png";
				memoryStream.WriteTo(_state._context.Response.OutputStream);
			}
			catch (Exception ex)
			{
				Trace.TraceError("An error occured while attempting to render the SVG image '" + _state._context.Request.PhysicalPath + "': " + ex.Message);
			}
			finally
			{
				_state._context.Response.End();
				_state.CompleteRequest();
			}
		}
	}

	protected sealed class SvgAsyncRenderState : IAsyncResult
	{
		internal HttpContext _context;

		internal AsyncCallback _callback;

		internal object _extraData;

		private bool _isCompleted;

		private ManualResetEvent _callCompleteEvent;

		public object AsyncState => _extraData;

		public bool CompletedSynchronously => false;

		public bool IsCompleted => _isCompleted;

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				lock (this)
				{
					if (_callCompleteEvent == null)
					{
						_callCompleteEvent = new ManualResetEvent(initialState: false);
					}
					return _callCompleteEvent;
				}
			}
		}

		public SvgAsyncRenderState(HttpContext context, AsyncCallback callback, object extraData)
		{
			_context = context;
			_callback = callback;
			_extraData = extraData;
		}

		internal void CompleteRequest()
		{
			_isCompleted = true;
			lock (this)
			{
				if (AsyncWaitHandle != null)
				{
					_callCompleteEvent.Set();
				}
			}
			if (_callback != null)
			{
				_callback(this);
			}
		}
	}

	private Thread t;

	public bool IsReusable => false;

	public void ProcessRequest(HttpContext context)
	{
	}

	public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
	{
		if (!File.Exists(context.Request.PhysicalPath))
		{
			throw new HttpException(404, "The requested file cannot be found.");
		}
		SvgAsyncRenderState svgAsyncRenderState = new SvgAsyncRenderState(context, cb, extraData);
		ThreadStart start = new SvgAsyncRender(svgAsyncRenderState).RenderSvg;
		t = new Thread(start);
		t.Start();
		return svgAsyncRenderState;
	}

	public void EndProcessRequest(IAsyncResult result)
	{
	}
}
