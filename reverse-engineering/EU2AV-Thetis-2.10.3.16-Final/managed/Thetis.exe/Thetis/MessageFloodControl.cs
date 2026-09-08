using System;
using System.Collections.Generic;
using System.Threading;

namespace Thetis;

public static class MessageFloodControl
{
	private class State
	{
		public Timer timer;

		public string latest_message;

		public DateTime last_fire_time;
	}

	private const int _max_interval = 200;

	private static readonly object sync = new object();

	private static readonly Dictionary<string, State> states = new Dictionary<string, State>();

	private static readonly TimeSpan interval = TimeSpan.FromMilliseconds(200.0);

	private static volatile bool shutting_down = false;

	public static event Action<string, string> SendMessage;

	public static void FloodControl(string message, string uid, bool ignore_flood = false)
	{
		if (shutting_down)
		{
			return;
		}
		if (uid == null)
		{
			throw new ArgumentNullException("uid");
		}
		if (string.IsNullOrEmpty(message))
		{
			return;
		}
		try
		{
			if (ignore_flood)
			{
				raise_send_message(message, uid);
				return;
			}
			bool flag = false;
			string message2 = null;
			DateTime utcNow = DateTime.UtcNow;
			lock (sync)
			{
				if (shutting_down)
				{
					return;
				}
				if (!states.TryGetValue(uid, out var value))
				{
					value = new State();
					value.timer = new Timer(timer_callback, uid, -1, -1);
					value.latest_message = string.Empty;
					value.last_fire_time = DateTime.MinValue;
					states[uid] = value;
				}
				value.latest_message = message;
				TimeSpan timeSpan = value.last_fire_time.Add(interval) - utcNow;
				if (timeSpan <= TimeSpan.Zero)
				{
					value.last_fire_time = utcNow;
					message2 = value.latest_message;
					flag = true;
					value.timer.Change(-1, -1);
				}
				else
				{
					int num = (int)Math.Ceiling(timeSpan.TotalMilliseconds);
					if (num < 1)
					{
						num = 1;
					}
					value.timer.Change(num, -1);
				}
			}
			if (flag)
			{
				raise_send_message(message2, uid);
			}
		}
		catch
		{
		}
	}

	public static void Shutdown()
	{
		shutting_down = true;
		try
		{
			List<Timer> list = new List<Timer>();
			lock (sync)
			{
				foreach (KeyValuePair<string, State> state in states)
				{
					list.Add(state.Value.timer);
				}
				states.Clear();
			}
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					list[i]?.Dispose();
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private static void timer_callback(object state_obj)
	{
		if (shutting_down)
		{
			return;
		}
		try
		{
			string text = state_obj as string;
			string message = null;
			bool flag = false;
			lock (sync)
			{
				if (shutting_down)
				{
					return;
				}
				if (text != null && states.TryGetValue(text, out var value))
				{
					value.last_fire_time = DateTime.UtcNow;
					message = value.latest_message;
					flag = true;
				}
			}
			if (flag)
			{
				raise_send_message(message, text);
			}
		}
		catch
		{
		}
	}

	private static void raise_send_message(string message, string uid)
	{
		try
		{
			Action<string, string> sendMessage = SendMessage;
			if (sendMessage == null)
			{
				return;
			}
			Delegate[] invocationList = sendMessage.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				try
				{
					((Action<string, string>)invocationList[i])(message, uid);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}
}
