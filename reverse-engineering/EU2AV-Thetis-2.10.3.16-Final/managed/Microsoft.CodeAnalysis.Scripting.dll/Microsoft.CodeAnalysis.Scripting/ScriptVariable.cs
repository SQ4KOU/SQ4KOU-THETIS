using System;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Scripting;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public sealed class ScriptVariable
{
	private readonly object _instance;

	private readonly FieldInfo _field;

	public string Name => _field.Name;

	public Type Type => _field.FieldType;

	public bool IsReadOnly
	{
		get
		{
			if (!_field.IsInitOnly)
			{
				return _field.IsLiteral;
			}
			return true;
		}
	}

	public object Value
	{
		get
		{
			return _field.GetValue(_instance);
		}
		set
		{
			if (_field.IsInitOnly)
			{
				throw new InvalidOperationException(ScriptingResources.CannotSetReadOnlyVariable);
			}
			if (_field.IsLiteral)
			{
				throw new InvalidOperationException(ScriptingResources.CannotSetConstantVariable);
			}
			_field.SetValue(_instance, value);
		}
	}

	internal ScriptVariable(object instance, FieldInfo field)
	{
		_instance = instance;
		_field = field;
	}

	private string GetDebuggerDisplay()
	{
		return string.Format("{0}: {1}", Name, Value ?? "<null>");
	}
}
