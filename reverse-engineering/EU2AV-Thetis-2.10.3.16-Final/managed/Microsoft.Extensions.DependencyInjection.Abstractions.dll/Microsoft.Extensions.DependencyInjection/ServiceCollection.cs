using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection;

[DebuggerDisplay("{DebuggerToString(),nq}")]
[DebuggerTypeProxy(typeof(ServiceCollectionDebugView))]
public class ServiceCollection : IServiceCollection, IList<ServiceDescriptor>, ICollection<ServiceDescriptor>, IEnumerable<ServiceDescriptor>, IEnumerable
{
	private sealed class ServiceCollectionDebugView
	{
		private readonly ServiceCollection _services;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public ServiceDescriptor[] Items
		{
			get
			{
				ServiceDescriptor[] array = new ServiceDescriptor[_services.Count];
				_services.CopyTo(array, 0);
				return array;
			}
		}

		public ServiceCollectionDebugView(ServiceCollection services)
		{
			_services = services;
		}
	}

	private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();

	private bool _isReadOnly;

	public int Count => _descriptors.Count;

	public bool IsReadOnly => _isReadOnly;

	public ServiceDescriptor this[int index]
	{
		get
		{
			return _descriptors[index];
		}
		set
		{
			CheckReadOnly();
			_descriptors[index] = value;
		}
	}

	public void Clear()
	{
		CheckReadOnly();
		_descriptors.Clear();
	}

	public bool Contains(ServiceDescriptor item)
	{
		return _descriptors.Contains(item);
	}

	public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
	{
		_descriptors.CopyTo(array, arrayIndex);
	}

	public bool Remove(ServiceDescriptor item)
	{
		CheckReadOnly();
		return _descriptors.Remove(item);
	}

	public IEnumerator<ServiceDescriptor> GetEnumerator()
	{
		return _descriptors.GetEnumerator();
	}

	void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
	{
		CheckReadOnly();
		_descriptors.Add(item);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public int IndexOf(ServiceDescriptor item)
	{
		return _descriptors.IndexOf(item);
	}

	public void Insert(int index, ServiceDescriptor item)
	{
		CheckReadOnly();
		_descriptors.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		CheckReadOnly();
		_descriptors.RemoveAt(index);
	}

	public void MakeReadOnly()
	{
		_isReadOnly = true;
	}

	private void CheckReadOnly()
	{
		if (_isReadOnly)
		{
			ThrowReadOnlyException();
		}
	}

	private static void ThrowReadOnlyException()
	{
		throw new InvalidOperationException(System.SR.ServiceCollectionReadOnly);
	}

	private string DebuggerToString()
	{
		string text = $"Count = {_descriptors.Count}";
		if (_isReadOnly)
		{
			text += ", IsReadOnly = true";
		}
		return text;
	}
}
