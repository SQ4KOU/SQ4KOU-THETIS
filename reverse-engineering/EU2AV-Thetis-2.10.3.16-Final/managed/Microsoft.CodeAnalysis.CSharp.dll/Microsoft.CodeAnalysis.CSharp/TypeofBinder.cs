using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class TypeofBinder : Binder
{
	private readonly Dictionary<GenericNameSyntax, bool> _allowedMap;

	internal TypeofBinder(ExpressionSyntax typeExpression, Binder next)
		: base(next, next.Flags | BinderFlags.UnsafeRegion)
	{
		OpenTypeVisitor.Visit(typeExpression, out _allowedMap);
	}

	protected override bool IsUnboundTypeAllowed(GenericNameSyntax syntax)
	{
		bool value = default(bool);
		return (_allowedMap != null && _allowedMap.TryGetValue(syntax, out value)) & value;
	}
}
