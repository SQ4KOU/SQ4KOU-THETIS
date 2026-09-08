using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Thetis;

public class SortableBindingList<T> : BindingList<T>
{
	public class PropertyComparer : IComparer<T>
	{
		private PropertyInfo PropInfo { get; set; }

		private ListSortDirection Direction { get; set; }

		public PropertyComparer(string propName, ListSortDirection direction)
		{
			PropInfo = typeof(T).GetProperty(propName);
			Direction = direction;
		}

		public int Compare(T x, T y)
		{
			object value = PropInfo.GetValue(x, null);
			object value2 = PropInfo.GetValue(y, null);
			if (Direction == ListSortDirection.Ascending)
			{
				return Comparer.Default.Compare(value, value2);
			}
			return Comparer.Default.Compare(value2, value);
		}
	}

	private bool m_IsSorted;

	private ListSortDirection m_SortDirection;

	private PropertyDescriptor m_SortProperty;

	protected override ListSortDirection SortDirectionCore => m_SortDirection;

	protected override PropertyDescriptor SortPropertyCore => m_SortProperty;

	protected override bool IsSortedCore => m_IsSorted;

	protected override bool SupportsSortingCore => true;

	public SortableBindingList()
	{
	}

	public SortableBindingList(List<T> list)
		: base((IList<T>)list)
	{
	}

	protected override void RemoveSortCore()
	{
		m_IsSorted = false;
	}

	protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
	{
		if (!(prop.PropertyType.GetInterface("IComparable") == null))
		{
			if (!(base.Items is List<T> list))
			{
				m_IsSorted = false;
			}
			else
			{
				PropertyComparer comparer = new PropertyComparer(prop.Name, direction);
				list.Sort(comparer);
				m_IsSorted = true;
				m_SortDirection = direction;
				m_SortProperty = prop;
			}
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}
}
