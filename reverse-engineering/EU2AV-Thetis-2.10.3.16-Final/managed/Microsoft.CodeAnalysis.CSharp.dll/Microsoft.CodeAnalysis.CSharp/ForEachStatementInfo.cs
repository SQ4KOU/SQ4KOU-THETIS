using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public readonly struct ForEachStatementInfo : IEquatable<ForEachStatementInfo>
{
	public bool IsAsynchronous { get; }

	public IMethodSymbol? GetEnumeratorMethod { get; }

	public IMethodSymbol? MoveNextMethod { get; }

	public AwaitExpressionInfo MoveNextAwaitableInfo { get; }

	public IPropertySymbol? CurrentProperty { get; }

	public IMethodSymbol? DisposeMethod { get; }

	public AwaitExpressionInfo DisposeAwaitableInfo { get; }

	public ITypeSymbol? ElementType { get; }

	public Conversion ElementConversion { get; }

	public Conversion CurrentConversion { get; }

	internal ForEachStatementInfo(bool isAsync, IMethodSymbol getEnumeratorMethod, IMethodSymbol moveNextMethod, AwaitExpressionInfo moveNextAwaitableInfo, IPropertySymbol currentProperty, IMethodSymbol disposeMethod, AwaitExpressionInfo disposeAwaitableInfo, ITypeSymbol elementType, Conversion elementConversion, Conversion currentConversion)
	{
		IsAsynchronous = isAsync;
		GetEnumeratorMethod = getEnumeratorMethod;
		MoveNextMethod = moveNextMethod;
		MoveNextAwaitableInfo = moveNextAwaitableInfo;
		CurrentProperty = currentProperty;
		DisposeMethod = disposeMethod;
		DisposeAwaitableInfo = disposeAwaitableInfo;
		ElementType = elementType;
		ElementConversion = elementConversion;
		CurrentConversion = currentConversion;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ForEachStatementInfo)
		{
			return Equals((ForEachStatementInfo)obj);
		}
		return false;
	}

	public bool Equals(ForEachStatementInfo other)
	{
		if (IsAsynchronous == other.IsAsynchronous && object.Equals(GetEnumeratorMethod, other.GetEnumeratorMethod) && object.Equals(MoveNextMethod, other.MoveNextMethod) && MoveNextAwaitableInfo.Equals(other.MoveNextAwaitableInfo) && object.Equals(CurrentProperty, other.CurrentProperty) && object.Equals(DisposeMethod, other.DisposeMethod) && DisposeAwaitableInfo.Equals(other.DisposeAwaitableInfo) && object.Equals(ElementType, other.ElementType) && ElementConversion == other.ElementConversion)
		{
			return CurrentConversion == other.CurrentConversion;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(IsAsynchronous, Hash.Combine(GetEnumeratorMethod, Hash.Combine(MoveNextMethod, Hash.Combine(MoveNextAwaitableInfo.GetHashCode(), Hash.Combine(CurrentProperty, Hash.Combine(DisposeMethod, Hash.Combine(DisposeAwaitableInfo.GetHashCode(), Hash.Combine(ElementType, Hash.Combine(ElementConversion.GetHashCode(), CurrentConversion.GetHashCode())))))))));
	}
}
