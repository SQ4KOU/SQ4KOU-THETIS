using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace Newtonsoft.Json.Utilities;

[RequiresDynamicCode("Newtonsoft.Json relies on dynamically creating types that may not be available with Ahead of Time compilation.")]
internal class NoThrowSetBinderMember : SetMemberBinder
{
	private readonly SetMemberBinder _innerBinder;

	public NoThrowSetBinderMember(SetMemberBinder innerBinder)
		: base(innerBinder.Name, innerBinder.IgnoreCase)
	{
		_innerBinder = innerBinder;
	}

	public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject? errorSuggestion)
	{
		DynamicMetaObject dynamicMetaObject = _innerBinder.Bind(target, new DynamicMetaObject[1] { value });
		return new DynamicMetaObject(new NoThrowExpressionVisitor().Visit(dynamicMetaObject.Expression), dynamicMetaObject.Restrictions);
	}
}
