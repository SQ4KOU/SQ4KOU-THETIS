using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CatAtonic;

namespace CATQueueBatching;

public sealed class MessageQueueSystem : IDisposable
{
	public sealed class MessageBatch
	{
		private readonly List<QueuedItem> items;

		private readonly object sync;

		public MessageBatch()
		{
			items = new List<QueuedItem>();
			sync = new object();
		}

		public Guid add(ScriptCommand cmd)
		{
			Guid guid = Guid.NewGuid();
			lock (sync)
			{
				items.Add(new QueuedItem(guid, cmd));
				return guid;
			}
		}

		internal bool isEmpty()
		{
			lock (sync)
			{
				return items.Count == 0;
			}
		}

		internal QueuedItem[] snapshot()
		{
			lock (sync)
			{
				return items.ToArray();
			}
		}
	}

	internal struct QueuedItem(Guid id, ScriptCommand command)
	{
		public readonly Guid id = id;

		public readonly ScriptCommand command = command;
	}

	private readonly BlockingCollection<QueuedItem>[] queues;

	private readonly CancellationTokenSource[] cts_array;

	private readonly Task[] workers;

	private readonly bool[] running;

	private readonly bool[] busy;

	private readonly object[] queue_locks;

	private readonly int queue_count;

	private int disposed;

	public event Action<int, Guid, ScriptCommand> message;

	public event Action<int, bool> queueState;

	public MessageQueueSystem(int n)
	{
		if (n <= 0)
		{
			throw new ArgumentOutOfRangeException("n");
		}
		queue_count = n;
		queues = new BlockingCollection<QueuedItem>[n];
		cts_array = new CancellationTokenSource[n];
		workers = new Task[n];
		running = new bool[n];
		busy = new bool[n];
		queue_locks = new object[n];
		for (int i = 0; i < n; i++)
		{
			queues[i] = new BlockingCollection<QueuedItem>(new ConcurrentQueue<QueuedItem>());
			cts_array[i] = new CancellationTokenSource();
			running[i] = false;
			busy[i] = false;
			queue_locks[i] = new object();
			startQueue(i);
		}
	}

	public MessageBatch createBatch()
	{
		return new MessageBatch();
	}

	public void sendBatch(int queue_index, MessageBatch batch)
	{
		throwIfDisposed();
		if (queue_index < 0 || queue_index >= queue_count)
		{
			throw new ArgumentOutOfRangeException("queue_index");
		}
		if (batch == null)
		{
			throw new ArgumentNullException("batch");
		}
		QueuedItem[] array = batch.snapshot();
		if (array.Length != 0)
		{
			ensureQueueRunning(queue_index);
			for (int i = 0; i < array.Length; i++)
			{
				queues[queue_index].Add(array[i]);
			}
		}
	}

	public bool isBusy(int queue_index)
	{
		throwIfDisposed();
		if (queue_index < 0 || queue_index >= queue_count)
		{
			throw new ArgumentOutOfRangeException("queue_index");
		}
		return busy[queue_index];
	}

	public int getPending(int queue_index)
	{
		throwIfDisposed();
		if (queue_index < 0 || queue_index >= queue_count)
		{
			throw new ArgumentOutOfRangeException("queue_index");
		}
		return queues[queue_index].Count;
	}

	public bool isEmpty(int queue_index)
	{
		throwIfDisposed();
		if (queue_index < 0 || queue_index >= queue_count)
		{
			throw new ArgumentOutOfRangeException("queue_index");
		}
		if (queues[queue_index].Count == 0)
		{
			return !busy[queue_index];
		}
		return false;
	}

	public void stopAndClearQueue(int queue_index)
	{
		throwIfDisposed();
		if (queue_index < 0 || queue_index >= queue_count)
		{
			throw new ArgumentOutOfRangeException("queue_index");
		}
		bool flag;
		lock (queue_locks[queue_index])
		{
			flag = running[queue_index];
			if (flag)
			{
				cts_array[queue_index].Cancel();
			}
		}
		if (flag)
		{
			try
			{
				workers[queue_index]?.Wait();
			}
			catch
			{
			}
			lock (queue_locks[queue_index])
			{
				drainQueue(queue_index);
				busy[queue_index] = false;
				running[queue_index] = false;
			}
			queueState?.Invoke(queue_index, arg2: false);
		}
	}

	public void stopAll()
	{
		throwIfDisposed();
		for (int i = 0; i < queue_count; i++)
		{
			stopAndClearQueue(i);
		}
	}

	private void startQueue(int index)
	{
		lock (queue_locks[index])
		{
			if (running[index])
			{
				return;
			}
			if (cts_array[index].IsCancellationRequested)
			{
				cts_array[index].Dispose();
				cts_array[index] = new CancellationTokenSource();
			}
			CancellationToken token = cts_array[index].Token;
			workers[index] = Task.Factory.StartNew(delegate
			{
				workerLoop(index, token);
			}, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
			running[index] = true;
		}
		queueState?.Invoke(index, arg2: true);
	}

	private void ensureQueueRunning(int index)
	{
		if (!running[index])
		{
			startQueue(index);
		}
	}

	private void workerLoop(int index, CancellationToken token)
	{
		BlockingCollection<QueuedItem> blockingCollection = queues[index];
		try
		{
			while (!token.IsCancellationRequested)
			{
				QueuedItem queuedItem;
				try
				{
					queuedItem = blockingCollection.Take(token);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				busy[index] = true;
				ScriptCommand command = queuedItem.command;
				if (command != null && command.type == ScriptCommandType.Wait)
				{
					int wait_ms = command.wait_ms;
					if (wait_ms > 0 && token.WaitHandle.WaitOne(wait_ms))
					{
						break;
					}
				}
				else
				{
					message?.Invoke(index, queuedItem.id, command);
				}
				if (blockingCollection.Count == 0)
				{
					busy[index] = false;
				}
			}
		}
		finally
		{
			busy[index] = false;
		}
	}

	private void drainQueue(int index)
	{
		BlockingCollection<QueuedItem> blockingCollection = queues[index];
		QueuedItem item;
		while (blockingCollection.TryTake(out item))
		{
		}
	}

	private void throwIfDisposed()
	{
		if (disposed != 0)
		{
			throw new ObjectDisposedException("MessageQueueSystem");
		}
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref disposed, 1) != 0)
		{
			return;
		}
		for (int i = 0; i < queue_count; i++)
		{
			try
			{
				cts_array[i].Cancel();
			}
			catch
			{
			}
		}
		for (int j = 0; j < queue_count; j++)
		{
			try
			{
				workers[j]?.Wait();
			}
			catch
			{
			}
		}
		for (int k = 0; k < queue_count; k++)
		{
			try
			{
				queues[k]?.Dispose();
			}
			catch
			{
			}
			try
			{
				cts_array[k]?.Dispose();
			}
			catch
			{
			}
		}
	}

	public void dispose()
	{
		Dispose();
	}
}
