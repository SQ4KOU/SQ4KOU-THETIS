using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WindowsFirewallHelper.InternalHelpers;

internal static class ThreadSafeSingleton
{
	private class ThreadInfo : IEquatable<ThreadInfo>
	{
		private readonly Thread _thread;

		private readonly Dictionary<Guid, object> _types;

		public ApartmentState ApartmentState { get; }

		public bool IsAlive
		{
			get
			{
				try
				{
					return _thread.IsAlive;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public int ThreadId { get; }

		public ThreadInfo(Thread thread)
		{
			ApartmentState = thread.GetApartmentState();
			ThreadId = thread.ManagedThreadId;
			_thread = thread;
			_types = new Dictionary<Guid, object>();
		}

		public bool Equals(ThreadInfo other)
		{
			if ((object)other == null)
			{
				return false;
			}
			if ((object)this == other)
			{
				return true;
			}
			return ThreadId == other.ThreadId;
		}

		public static bool operator ==(ThreadInfo left, ThreadInfo right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(ThreadInfo left, ThreadInfo right)
		{
			return !object.Equals(left, right);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			return Equals(obj as ThreadInfo);
		}

		public override int GetHashCode()
		{
			return ThreadId;
		}

		public T AddInstance<T>(T instance = null) where T : class, new()
		{
			Guid gUID = typeof(T).GUID;
			instance = instance ?? new T();
			_types[gUID] = instance;
			return instance;
		}

		public T GetInstance<T>() where T : class, new()
		{
			Guid type = typeof(T).GUID;
			return _types.FirstOrDefault((KeyValuePair<Guid, object> pair) => pair.Key.Equals(type)).Value as T;
		}
	}

	private static readonly IList<ThreadInfo> Threads = new List<ThreadInfo>();

	public static T GetInstance<T>() where T : class, new()
	{
		lock (Threads)
		{
			try
			{
				Thread currentThread = Thread.CurrentThread;
				ThreadInfo threadInfo = Threads.FirstOrDefault((ThreadInfo info) => info.ThreadId == currentThread.ManagedThreadId);
				if (threadInfo == null)
				{
					threadInfo = new ThreadInfo(currentThread);
					Threads.Add(threadInfo);
				}
				T instance = threadInfo.GetInstance<T>();
				if (instance != null)
				{
					return instance;
				}
				if (threadInfo.ApartmentState == ApartmentState.MTA)
				{
					instance = (from info in Threads
						where info.ApartmentState == ApartmentState.MTA
						select info.GetInstance<T>()).FirstOrDefault((T arg) => arg != null);
					if (instance != null)
					{
						return threadInfo.AddInstance(instance);
					}
				}
				return threadInfo.AddInstance<T>();
			}
			finally
			{
				ThreadInfo[] array = Threads.Where((ThreadInfo info) => !info.IsAlive).ToArray();
				foreach (ThreadInfo item in array)
				{
					Threads.Remove(item);
				}
			}
		}
	}
}
