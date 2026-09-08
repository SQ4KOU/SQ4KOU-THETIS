using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult> : Producer<TResult, Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>._>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		private sealed class LeftObserver : SafeObserver<TLeft>
		{
			private sealed class DurationObserver : SafeObserver<TLeftDuration>
			{
				private readonly LeftObserver _parent;

				private readonly int _id;

				public DurationObserver(LeftObserver parent, int id)
				{
					_parent = parent;
					_id = id;
				}

				public override void OnNext(TLeftDuration value)
				{
					_parent.Expire(_id, this);
				}

				public override void OnError(Exception error)
				{
					_parent.OnError(error);
				}

				public override void OnCompleted()
				{
					_parent.Expire(_id, this);
				}
			}

			private readonly @_ _parent;

			public LeftObserver(@_ parent)
			{
				_parent = parent;
			}

			private void Expire(int id, IDisposable resource)
			{
				lock (_parent._gate)
				{
					if (_parent._leftMap.Remove(id) && _parent._leftMap.Count == 0 && _parent._leftDone)
					{
						_parent.ForwardOnCompleted();
					}
				}
				_parent._group.Remove(resource);
			}

			public override void OnNext(TLeft value)
			{
				int num;
				int rightID;
				lock (_parent._gate)
				{
					num = _parent._leftID++;
					rightID = _parent._rightID;
					_parent._leftMap.Add(num, value);
				}
				IObservable<TLeftDuration> source;
				try
				{
					source = _parent._leftDurationSelector(value);
				}
				catch (Exception error)
				{
					_parent.ForwardOnError(error);
					return;
				}
				DurationObserver durationObserver = new DurationObserver(this, num);
				_parent._group.Add(durationObserver);
				durationObserver.SetResource(source.SubscribeSafe(durationObserver));
				lock (_parent._gate)
				{
					foreach (KeyValuePair<int, TRight> item in _parent._rightMap)
					{
						if (item.Key < rightID)
						{
							TResult value2;
							try
							{
								value2 = _parent._resultSelector(value, item.Value);
							}
							catch (Exception error2)
							{
								_parent.ForwardOnError(error2);
								break;
							}
							_parent.ForwardOnNext(value2);
						}
					}
				}
			}

			public override void OnError(Exception error)
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_parent._gate)
				{
					_parent._leftDone = true;
					if (_parent._rightDone || _parent._leftMap.Count == 0)
					{
						_parent.ForwardOnCompleted();
					}
					else
					{
						Dispose();
					}
				}
			}
		}

		private sealed class RightObserver : SafeObserver<TRight>
		{
			private sealed class DurationObserver : SafeObserver<TRightDuration>
			{
				private readonly RightObserver _parent;

				private readonly int _id;

				public DurationObserver(RightObserver parent, int id)
				{
					_parent = parent;
					_id = id;
				}

				public override void OnNext(TRightDuration value)
				{
					_parent.Expire(_id, this);
				}

				public override void OnError(Exception error)
				{
					_parent.OnError(error);
				}

				public override void OnCompleted()
				{
					_parent.Expire(_id, this);
				}
			}

			private readonly @_ _parent;

			public RightObserver(@_ parent)
			{
				_parent = parent;
			}

			private void Expire(int id, IDisposable resource)
			{
				lock (_parent._gate)
				{
					if (_parent._rightMap.Remove(id) && _parent._rightMap.Count == 0 && _parent._rightDone)
					{
						_parent.ForwardOnCompleted();
					}
				}
				_parent._group.Remove(resource);
			}

			public override void OnNext(TRight value)
			{
				int num;
				int leftID;
				lock (_parent._gate)
				{
					num = _parent._rightID++;
					leftID = _parent._leftID;
					_parent._rightMap.Add(num, value);
				}
				IObservable<TRightDuration> source;
				try
				{
					source = _parent._rightDurationSelector(value);
				}
				catch (Exception error)
				{
					_parent.ForwardOnError(error);
					return;
				}
				DurationObserver durationObserver = new DurationObserver(this, num);
				_parent._group.Add(durationObserver);
				durationObserver.SetResource(source.SubscribeSafe(durationObserver));
				lock (_parent._gate)
				{
					foreach (KeyValuePair<int, TLeft> item in _parent._leftMap)
					{
						if (item.Key < leftID)
						{
							TResult value2;
							try
							{
								value2 = _parent._resultSelector(item.Value, value);
							}
							catch (Exception error2)
							{
								_parent.ForwardOnError(error2);
								break;
							}
							_parent.ForwardOnNext(value2);
						}
					}
				}
			}

			public override void OnError(Exception error)
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_parent._gate)
				{
					_parent._rightDone = true;
					if (_parent._leftDone || _parent._rightMap.Count == 0)
					{
						_parent.ForwardOnCompleted();
					}
					else
					{
						Dispose();
					}
				}
			}
		}

		private readonly object _gate = new object();

		private readonly CompositeDisposable _group = new CompositeDisposable();

		private readonly SortedDictionary<int, TLeft> _leftMap = new SortedDictionary<int, TLeft>();

		private readonly SortedDictionary<int, TRight> _rightMap = new SortedDictionary<int, TRight>();

		private readonly Func<TLeft, IObservable<TLeftDuration>> _leftDurationSelector;

		private readonly Func<TRight, IObservable<TRightDuration>> _rightDurationSelector;

		private readonly Func<TLeft, TRight, TResult> _resultSelector;

		private bool _leftDone;

		private int _leftID;

		private bool _rightDone;

		private int _rightID;

		public _(Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult> parent, IObserver<TResult> observer)
			: base(observer)
		{
			_leftDurationSelector = parent._leftDurationSelector;
			_rightDurationSelector = parent._rightDurationSelector;
			_resultSelector = parent._resultSelector;
		}

		public void Run(Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult> parent)
		{
			LeftObserver leftObserver = new LeftObserver(this);
			RightObserver rightObserver = new RightObserver(this);
			_group.Add(leftObserver);
			_group.Add(rightObserver);
			leftObserver.SetResource(parent._left.SubscribeSafe(leftObserver));
			rightObserver.SetResource(parent._right.SubscribeSafe(rightObserver));
			SetUpstream(_group);
		}
	}

	private readonly IObservable<TLeft> _left;

	private readonly IObservable<TRight> _right;

	private readonly Func<TLeft, IObservable<TLeftDuration>> _leftDurationSelector;

	private readonly Func<TRight, IObservable<TRightDuration>> _rightDurationSelector;

	private readonly Func<TLeft, TRight, TResult> _resultSelector;

	public Join(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, TRight, TResult> resultSelector)
	{
		_left = left;
		_right = right;
		_leftDurationSelector = leftDurationSelector;
		_rightDurationSelector = rightDurationSelector;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(this);
	}
}
