using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WindowsFirewallHelper.InternalHelpers.Collections;

internal abstract class ComCollectionBase<TCollection, TNative, TKey, TManaged> : IComCollection<TKey, TManaged>, ICollection<TManaged>, IEnumerable<TManaged>, IEnumerable, ICollection where TCollection : IEnumerable where TNative : class where TManaged : class
{
	protected readonly TCollection NativeEnumerable;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => NativeEnumerable;

	public virtual int Count
	{
		get
		{
			try
			{
				return InternalCount();
			}
			catch (COMException innerException)
			{
				throw new NotSupportedException("This property is not supported with the passed COM object.", innerException);
			}
		}
	}

	public abstract bool IsReadOnly { get; }

	public virtual TManaged this[TKey key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException();
			}
			try
			{
				TNative val = InternalItem(key);
				if (val == null)
				{
					return null;
				}
				return ConvertNativeToManaged(val);
			}
			catch (COMException innerException)
			{
				throw new NotSupportedException("This operation is not supported with the passed COM object.", innerException);
			}
		}
	}

	protected ComCollectionBase(TCollection nativeEnumerable)
	{
		if (nativeEnumerable == null)
		{
			throw new ArgumentNullException("nativeEnumerable");
		}
		if (!nativeEnumerable.GetType().IsCOMObject)
		{
			throw new ArgumentException("Passed argument is not a valid COM Enumerable object.", "nativeEnumerable");
		}
		NativeEnumerable = nativeEnumerable;
	}

	public virtual void CopyTo(Array array, int index)
	{
		using IEnumerator<TManaged> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			TManaged current = enumerator.Current;
			array.SetValue(current, index);
			index++;
		}
	}

	public virtual void Add(TManaged item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (IsReadOnly)
		{
			throw new InvalidOperationException("Collection is readonly.");
		}
		TNative val = ConvertManagedToNative(item);
		if (val == null)
		{
			return;
		}
		try
		{
			InternalAdd(val);
		}
		catch (COMException innerException)
		{
			throw new NotSupportedException("This operation is not supported with the passed COM object.", innerException);
		}
	}

	public virtual void Clear()
	{
		throw new NotSupportedException();
	}

	public virtual bool Contains(TManaged item)
	{
		return this.Any((TManaged target) => target == item);
	}

	public virtual void CopyTo(TManaged[] array, int arrayIndex)
	{
		CopyTo((Array)array, arrayIndex);
	}

	public virtual bool Remove(TManaged item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		TKey collectionKey = GetCollectionKey(item);
		if (collectionKey == null)
		{
			return false;
		}
		Remove(collectionKey);
		return true;
	}

	public virtual bool Contains(TKey key)
	{
		return this.Select(GetCollectionKey).Any((TKey val) => val.Equals(key));
	}

	public virtual bool Remove(TKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (IsReadOnly)
		{
			throw new InvalidOperationException("Collection is readonly.");
		}
		try
		{
			InternalRemove(key);
			return true;
		}
		catch (COMException innerException)
		{
			throw new NotSupportedException("This operation is not supported with the passed COM object.", innerException);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public virtual IEnumerator<TManaged> GetEnumerator()
	{
		return new ComEnumerator<TNative, TManaged>(GetEnumVariant() ?? throw new NotSupportedException("This operation is not supported with the passed COM object."), ConvertNativeToManaged);
	}

	protected abstract TNative ConvertManagedToNative(TManaged managed);

	protected abstract TManaged ConvertNativeToManaged(TNative native);

	protected abstract TKey GetCollectionKey(TManaged managed);

	protected abstract IEnumVARIANT GetEnumVariant();

	protected abstract void InternalAdd(TNative native);

	protected abstract int InternalCount();

	protected abstract TNative InternalItem(TKey key);

	protected abstract void InternalRemove(TKey key);
}
