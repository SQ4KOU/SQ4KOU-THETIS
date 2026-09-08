namespace System.Reactive.Concurrency;

public class SchedulerQueue<TAbsolute> where TAbsolute : IComparable<TAbsolute>
{
	private readonly PriorityQueue<ScheduledItem<TAbsolute>> _queue;

	public int Count => _queue.Count;

	public SchedulerQueue()
		: this(1024)
	{
	}

	public SchedulerQueue(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		_queue = new PriorityQueue<ScheduledItem<TAbsolute>>(capacity);
	}

	public void Enqueue(ScheduledItem<TAbsolute> scheduledItem)
	{
		_queue.Enqueue(scheduledItem);
	}

	public bool Remove(ScheduledItem<TAbsolute> scheduledItem)
	{
		return _queue.Remove(scheduledItem);
	}

	public ScheduledItem<TAbsolute> Dequeue()
	{
		return _queue.Dequeue();
	}

	public ScheduledItem<TAbsolute> Peek()
	{
		return _queue.Peek();
	}
}
