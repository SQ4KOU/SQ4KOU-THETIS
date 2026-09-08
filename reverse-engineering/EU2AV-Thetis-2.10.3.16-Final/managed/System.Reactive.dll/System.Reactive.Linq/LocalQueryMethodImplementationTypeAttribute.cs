using System.ComponentModel;

namespace System.Reactive.Linq;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class LocalQueryMethodImplementationTypeAttribute : Attribute
{
	private readonly Type _targetType;

	public Type TargetType => _targetType;

	public LocalQueryMethodImplementationTypeAttribute(Type targetType)
	{
		_targetType = targetType;
	}
}
