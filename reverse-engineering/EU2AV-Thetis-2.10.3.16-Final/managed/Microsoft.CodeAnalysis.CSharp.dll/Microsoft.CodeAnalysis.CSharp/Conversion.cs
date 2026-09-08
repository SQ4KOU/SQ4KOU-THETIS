using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public readonly struct Conversion : IEquatable<Conversion>, IConvertibleConversion
{
	private abstract class UncommonData
	{
	}

	private sealed class MethodUncommonData : UncommonData
	{
		public static readonly MethodUncommonData NoApplicableOperators = new MethodUncommonData(isExtensionMethod: false, isArrayIndex: false, UserDefinedConversionResult.NoApplicableOperators(ImmutableArray<UserDefinedConversionAnalysis>.Empty), null);

		internal readonly MethodSymbol? _conversionMethod;

		internal readonly UserDefinedConversionResult _conversionResult;

		private const byte IsExtensionMethodMask = 1;

		private const byte IsArrayIndexMask = 2;

		private readonly byte _flags;

		internal bool IsExtensionMethod => (_flags & 1) != 0;

		internal bool IsArrayIndex => (_flags & 2) != 0;

		public MethodUncommonData(bool isExtensionMethod, bool isArrayIndex, UserDefinedConversionResult conversionResult, MethodSymbol? conversionMethod)
		{
			_conversionMethod = conversionMethod;
			_conversionResult = conversionResult;
			_flags = (isExtensionMethod ? ((byte)1) : ((byte)0));
			if (isArrayIndex)
			{
				_flags |= 2;
			}
		}
	}

	private class NestedUncommonData : UncommonData
	{
		internal readonly ImmutableArray<Conversion> _nestedConversionsOpt;

		public NestedUncommonData(ImmutableArray<Conversion> nestedConversions)
		{
			_nestedConversionsOpt = nestedConversions;
		}
	}

	private sealed class DeconstructionUncommonData : UncommonData
	{
		internal readonly DeconstructMethodInfo DeconstructMethodInfo;

		internal readonly ImmutableArray<(BoundValuePlaceholder? placeholder, BoundExpression? conversion)> DeconstructConversionInfo;

		internal DeconstructionUncommonData(DeconstructMethodInfo deconstructMethodInfoOpt, ImmutableArray<(BoundValuePlaceholder? placeholder, BoundExpression? conversion)> deconstructConversionInfo)
		{
			DeconstructMethodInfo = deconstructMethodInfoOpt;
			DeconstructConversionInfo = deconstructConversionInfo;
		}
	}

	private sealed class CollectionExpressionUncommonData : NestedUncommonData
	{
		internal readonly CollectionExpressionTypeKind CollectionExpressionTypeKind;

		internal readonly TypeSymbol ElementType;

		internal readonly MethodSymbol? Constructor;

		internal readonly bool ConstructorUsedInExpandedForm;

		internal CollectionExpressionUncommonData(CollectionExpressionTypeKind collectionExpressionTypeKind, TypeSymbol elementType, MethodSymbol? constructor, bool constructorUsedInExpandedForm, ImmutableArray<Conversion> elementConversions)
			: base(elementConversions)
		{
			CollectionExpressionTypeKind = collectionExpressionTypeKind;
			ElementType = elementType;
			Constructor = constructor;
			ConstructorUsedInExpandedForm = constructorUsedInExpandedForm;
		}
	}

	private static class ConversionSingletons
	{
		internal static ImmutableArray<Conversion> IdentityUnderlying = ImmutableArray.Create(Identity);

		internal static ImmutableArray<Conversion> ImplicitConstantUnderlying = ImmutableArray.Create(ImplicitConstant);

		internal static ImmutableArray<Conversion> ImplicitNumericUnderlying = ImmutableArray.Create(ImplicitNumeric);

		internal static ImmutableArray<Conversion> ExplicitNumericUnderlying = ImmutableArray.Create(ExplicitNumeric);

		internal static ImmutableArray<Conversion> ExplicitEnumerationUnderlying = ImmutableArray.Create(ExplicitEnumeration);

		internal static ImmutableArray<Conversion> PointerToIntegerUnderlying = ImmutableArray.Create(PointerToInteger);
	}

	private readonly ConversionKind _kind;

	private readonly UncommonData? _uncommonData;

	internal static readonly Conversion ExplicitNullableWithExplicitEnumerationUnderlying = new Conversion(ConversionKind.ExplicitNullable, ExplicitEnumerationUnderlying);

	internal static readonly Conversion ExplicitNullableWithPointerToIntegerUnderlying = new Conversion(ConversionKind.ExplicitNullable, PointerToIntegerUnderlying);

	internal static readonly Conversion ExplicitNullableWithIdentityUnderlying = new Conversion(ConversionKind.ExplicitNullable, IdentityUnderlying);

	internal static readonly Conversion ExplicitNullableWithImplicitNumericUnderlying = new Conversion(ConversionKind.ExplicitNullable, ImplicitNumericUnderlying);

	internal static readonly Conversion ExplicitNullableWithExplicitNumericUnderlying = new Conversion(ConversionKind.ExplicitNullable, ExplicitNumericUnderlying);

	internal static readonly Conversion ExplicitNullableWithImplicitConstantUnderlying = new Conversion(ConversionKind.ExplicitNullable, ImplicitConstantUnderlying);

	internal static readonly Conversion ImplicitNullableWithExplicitEnumerationUnderlying = new Conversion(ConversionKind.ImplicitNullable, ExplicitEnumerationUnderlying);

	internal static readonly Conversion ImplicitNullableWithPointerToIntegerUnderlying = new Conversion(ConversionKind.ImplicitNullable, PointerToIntegerUnderlying);

	internal static readonly Conversion ImplicitNullableWithIdentityUnderlying = new Conversion(ConversionKind.ImplicitNullable, IdentityUnderlying);

	internal static readonly Conversion ImplicitNullableWithImplicitNumericUnderlying = new Conversion(ConversionKind.ImplicitNullable, ImplicitNumericUnderlying);

	internal static readonly Conversion ImplicitNullableWithExplicitNumericUnderlying = new Conversion(ConversionKind.ImplicitNullable, ExplicitNumericUnderlying);

	internal static readonly Conversion ImplicitNullableWithImplicitConstantUnderlying = new Conversion(ConversionKind.ImplicitNullable, ImplicitConstantUnderlying);

	internal static Conversion UnsetConversion => new Conversion(ConversionKind.UnsetConversionKind);

	internal static Conversion NoConversion => new Conversion(ConversionKind.NoConversion);

	internal static Conversion Identity => new Conversion(ConversionKind.Identity);

	internal static Conversion ImplicitConstant => new Conversion(ConversionKind.ImplicitConstant);

	internal static Conversion ImplicitNumeric => new Conversion(ConversionKind.ImplicitNumeric);

	internal static Conversion ImplicitReference => new Conversion(ConversionKind.ImplicitReference);

	internal static Conversion ImplicitEnumeration => new Conversion(ConversionKind.ImplicitEnumeration);

	internal static Conversion ImplicitThrow => new Conversion(ConversionKind.ImplicitThrow);

	internal static Conversion ObjectCreation => new Conversion(ConversionKind.ObjectCreation);

	internal static Conversion CollectionExpression => new Conversion(ConversionKind.CollectionExpression);

	internal static Conversion AnonymousFunction => new Conversion(ConversionKind.AnonymousFunction);

	internal static Conversion Boxing => new Conversion(ConversionKind.Boxing);

	internal static Conversion NullLiteral => new Conversion(ConversionKind.NullLiteral);

	internal static Conversion DefaultLiteral => new Conversion(ConversionKind.DefaultLiteral);

	internal static Conversion NullToPointer => new Conversion(ConversionKind.ImplicitNullToPointer);

	internal static Conversion PointerToVoid => new Conversion(ConversionKind.ImplicitPointerToVoid);

	internal static Conversion PointerToPointer => new Conversion(ConversionKind.ExplicitPointerToPointer);

	internal static Conversion PointerToInteger => new Conversion(ConversionKind.ExplicitPointerToInteger);

	internal static Conversion IntegerToPointer => new Conversion(ConversionKind.ExplicitIntegerToPointer);

	internal static Conversion Unboxing => new Conversion(ConversionKind.Unboxing);

	internal static Conversion ExplicitReference => new Conversion(ConversionKind.ExplicitReference);

	internal static Conversion IntPtr => new Conversion(ConversionKind.IntPtr);

	internal static Conversion ExplicitEnumeration => new Conversion(ConversionKind.ExplicitEnumeration);

	internal static Conversion ExplicitNumeric => new Conversion(ConversionKind.ExplicitNumeric);

	internal static Conversion ImplicitDynamic => new Conversion(ConversionKind.ImplicitDynamic);

	internal static Conversion ExplicitDynamic => new Conversion(ConversionKind.ExplicitDynamic);

	internal static Conversion InterpolatedString => new Conversion(ConversionKind.InterpolatedString);

	internal static Conversion InterpolatedStringHandler => new Conversion(ConversionKind.InterpolatedStringHandler);

	internal static Conversion Deconstruction => new Conversion(ConversionKind.Deconstruction);

	internal static Conversion PinnedObjectToPointer => new Conversion(ConversionKind.PinnedObjectToPointer);

	internal static Conversion ImplicitPointer => new Conversion(ConversionKind.ImplicitPointer);

	internal static Conversion FunctionType => new Conversion(ConversionKind.FunctionType);

	internal static Conversion InlineArray => new Conversion(ConversionKind.InlineArray);

	internal static Conversion ImplicitSpan => new Conversion(ConversionKind.ImplicitSpan);

	internal static Conversion ExplicitSpan => new Conversion(ConversionKind.ExplicitSpan);

	internal static ImmutableArray<Conversion> IdentityUnderlying => ConversionSingletons.IdentityUnderlying;

	internal static ImmutableArray<Conversion> ImplicitConstantUnderlying => ConversionSingletons.ImplicitConstantUnderlying;

	internal static ImmutableArray<Conversion> ImplicitNumericUnderlying => ConversionSingletons.ImplicitNumericUnderlying;

	internal static ImmutableArray<Conversion> ExplicitNumericUnderlying => ConversionSingletons.ExplicitNumericUnderlying;

	internal static ImmutableArray<Conversion> ExplicitEnumerationUnderlying => ConversionSingletons.ExplicitEnumerationUnderlying;

	internal static ImmutableArray<Conversion> PointerToIntegerUnderlying => ConversionSingletons.PointerToIntegerUnderlying;

	internal ConversionKind Kind => _kind;

	internal bool IsExtensionMethod
	{
		get
		{
			if (_uncommonData is MethodUncommonData methodUncommonData)
			{
				return methodUncommonData.IsExtensionMethod;
			}
			return false;
		}
	}

	internal bool IsArrayIndex
	{
		get
		{
			if (_uncommonData is MethodUncommonData methodUncommonData)
			{
				return methodUncommonData.IsArrayIndex;
			}
			return false;
		}
	}

	internal ImmutableArray<Conversion> UnderlyingConversions
	{
		get
		{
			if (_uncommonData is NestedUncommonData nestedUncommonData)
			{
				return nestedUncommonData._nestedConversionsOpt;
			}
			return default(ImmutableArray<Conversion>);
		}
	}

	internal MethodSymbol? Method
	{
		get
		{
			UncommonData uncommonData = _uncommonData;
			if (!(uncommonData is MethodUncommonData methodUncommonData))
			{
				if (uncommonData is DeconstructionUncommonData deconstructionUncommonData && deconstructionUncommonData.DeconstructMethodInfo.Invocation is BoundCall boundCall)
				{
					return boundCall.Method;
				}
			}
			else
			{
				if ((object)methodUncommonData._conversionMethod != null)
				{
					return methodUncommonData._conversionMethod;
				}
				UserDefinedConversionResult conversionResult = methodUncommonData._conversionResult;
				if (conversionResult.Kind == UserDefinedConversionResultKind.Valid)
				{
					return conversionResult.Results[conversionResult.Best].Operator;
				}
			}
			return null;
		}
	}

	internal TypeParameterSymbol? ConstrainedToTypeOpt
	{
		get
		{
			if (_uncommonData is MethodUncommonData { _conversionMethod: null, _conversionResult: { Kind: UserDefinedConversionResultKind.Valid } conversionResult })
			{
				return conversionResult.Results[conversionResult.Best].ConstrainedToTypeOpt;
			}
			return null;
		}
	}

	internal DeconstructMethodInfo DeconstructionInfo => ((DeconstructionUncommonData)_uncommonData)?.DeconstructMethodInfo ?? default(DeconstructMethodInfo);

	internal ImmutableArray<(BoundValuePlaceholder? placeholder, BoundExpression? conversion)> DeconstructConversionInfo => ((DeconstructionUncommonData)_uncommonData)?.DeconstructConversionInfo ?? default(ImmutableArray<(BoundValuePlaceholder, BoundExpression)>);

	internal bool IsValid
	{
		get
		{
			if (!Exists)
			{
				return false;
			}
			if (_uncommonData is NestedUncommonData { _nestedConversionsOpt: { IsDefault: false } nestedConversionsOpt })
			{
				foreach (Conversion item in nestedConversionsOpt)
				{
					if (!item.IsValid)
					{
						return false;
					}
				}
				return true;
			}
			if (IsUserDefined && (object)Method == null)
			{
				MethodUncommonData obj = _uncommonData as MethodUncommonData;
				if (obj == null)
				{
					return false;
				}
				return obj._conversionResult.Kind == UserDefinedConversionResultKind.Valid;
			}
			return true;
		}
	}

	public bool Exists => Kind != ConversionKind.NoConversion;

	public bool IsImplicit => Kind.IsImplicitConversion();

	public bool IsExplicit
	{
		get
		{
			if (Exists)
			{
				return !IsImplicit;
			}
			return false;
		}
	}

	public bool IsIdentity => Kind == ConversionKind.Identity;

	public bool IsStackAlloc
	{
		get
		{
			if (Kind != ConversionKind.StackAllocToPointerType)
			{
				return Kind == ConversionKind.StackAllocToSpanType;
			}
			return true;
		}
	}

	public bool IsNumeric
	{
		get
		{
			if (Kind != ConversionKind.ImplicitNumeric)
			{
				return Kind == ConversionKind.ExplicitNumeric;
			}
			return true;
		}
	}

	public bool IsEnumeration
	{
		get
		{
			if (Kind != ConversionKind.ImplicitEnumeration)
			{
				return Kind == ConversionKind.ExplicitEnumeration;
			}
			return true;
		}
	}

	public bool IsThrow => Kind == ConversionKind.ImplicitThrow;

	public bool IsObjectCreation => Kind == ConversionKind.ObjectCreation;

	public bool IsCollectionExpression => Kind == ConversionKind.CollectionExpression;

	public bool IsSwitchExpression => Kind == ConversionKind.SwitchExpression;

	public bool IsConditionalExpression => Kind == ConversionKind.ConditionalExpression;

	public bool IsInterpolatedString => Kind == ConversionKind.InterpolatedString;

	public bool IsInterpolatedStringHandler => Kind == ConversionKind.InterpolatedStringHandler;

	public bool IsInlineArray => Kind == ConversionKind.InlineArray;

	public bool IsNullable
	{
		get
		{
			if (Kind != ConversionKind.ImplicitNullable)
			{
				return Kind == ConversionKind.ExplicitNullable;
			}
			return true;
		}
	}

	public bool IsTupleLiteralConversion
	{
		get
		{
			if (Kind != ConversionKind.ImplicitTupleLiteral)
			{
				return Kind == ConversionKind.ExplicitTupleLiteral;
			}
			return true;
		}
	}

	public bool IsTupleConversion
	{
		get
		{
			if (Kind != ConversionKind.ImplicitTuple)
			{
				return Kind == ConversionKind.ExplicitTuple;
			}
			return true;
		}
	}

	public bool IsReference
	{
		get
		{
			if (Kind != ConversionKind.ImplicitReference)
			{
				return Kind == ConversionKind.ExplicitReference;
			}
			return true;
		}
	}

	public bool IsSpan
	{
		get
		{
			ConversionKind kind = Kind;
			if (kind - 46 <= ConversionKind.NoConversion)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsUserDefined => Kind.IsUserDefinedConversion();

	public bool IsBoxing => Kind == ConversionKind.Boxing;

	public bool IsUnboxing => Kind == ConversionKind.Unboxing;

	public bool IsNullLiteral => Kind == ConversionKind.NullLiteral;

	public bool IsDefaultLiteral => Kind == ConversionKind.DefaultLiteral;

	public bool IsDynamic => Kind.IsDynamic();

	public bool IsConstantExpression => Kind == ConversionKind.ImplicitConstant;

	public bool IsAnonymousFunction => Kind == ConversionKind.AnonymousFunction;

	public bool IsMethodGroup => Kind == ConversionKind.MethodGroup;

	public bool IsPointer => Kind.IsPointerConversion();

	public bool IsIntPtr => Kind == ConversionKind.IntPtr;

	public IMethodSymbol? MethodSymbol => Method.GetPublicSymbol();

	public ITypeSymbol? ConstrainedToType => ConstrainedToTypeOpt.GetPublicSymbol();

	internal LookupResultKind ResultKind
	{
		get
		{
			UserDefinedConversionResult userDefinedConversionResult = (_uncommonData as MethodUncommonData)?._conversionResult ?? default(UserDefinedConversionResult);
			switch (userDefinedConversionResult.Kind)
			{
			case UserDefinedConversionResultKind.Valid:
				return LookupResultKind.Viable;
			case UserDefinedConversionResultKind.NoBestSourceType:
			case UserDefinedConversionResultKind.NoBestTargetType:
			case UserDefinedConversionResultKind.Ambiguous:
				return LookupResultKind.OverloadResolutionFailure;
			case UserDefinedConversionResultKind.NoApplicableOperators:
				if (userDefinedConversionResult.Results.IsDefaultOrEmpty)
				{
					if (Kind != ConversionKind.NoConversion)
					{
						return LookupResultKind.Viable;
					}
					return LookupResultKind.Empty;
				}
				return LookupResultKind.OverloadResolutionFailure;
			default:
				throw ExceptionUtilities.UnexpectedValue(userDefinedConversionResult.Kind);
			}
		}
	}

	internal Conversion UserDefinedFromConversion => BestUserDefinedConversionAnalysis?.SourceConversion ?? NoConversion;

	internal Conversion UserDefinedToConversion => BestUserDefinedConversionAnalysis?.TargetConversion ?? NoConversion;

	internal ImmutableArray<MethodSymbol> OriginalUserDefinedConversions
	{
		get
		{
			if (_uncommonData is MethodUncommonData { _conversionResult: { Kind: not UserDefinedConversionResultKind.NoApplicableOperators } conversionResult })
			{
				ArrayBuilder<MethodSymbol> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.GetInstance();
				foreach (UserDefinedConversionAnalysis result in conversionResult.Results)
				{
					instance.Add(result.Operator);
				}
				return instance.ToImmutableAndFree();
			}
			return ImmutableArray<Microsoft.CodeAnalysis.CSharp.Symbols.MethodSymbol>.Empty;
		}
	}

	internal UserDefinedConversionAnalysis? BestUserDefinedConversionAnalysis
	{
		get
		{
			if (_uncommonData is MethodUncommonData { _conversionResult: { Kind: UserDefinedConversionResultKind.Valid } conversionResult })
			{
				return conversionResult.Results[conversionResult.Best];
			}
			return null;
		}
	}

	internal static Conversion CreateCollectionExpressionConversion(CollectionExpressionTypeKind collectionExpressionTypeKind, TypeSymbol elementType, MethodSymbol? constructor, bool constructorUsedInExpandedForm, ImmutableArray<Conversion> elementConversions)
	{
		return new Conversion(ConversionKind.CollectionExpression, new CollectionExpressionUncommonData(collectionExpressionTypeKind, elementType, constructor, constructorUsedInExpandedForm, elementConversions));
	}

	private Conversion(ConversionKind kind, UncommonData? uncommonData = null)
	{
		_kind = kind;
		_uncommonData = uncommonData;
	}

	internal Conversion(UserDefinedConversionResult conversionResult, bool isImplicit)
	{
		_kind = ((conversionResult.Kind == UserDefinedConversionResultKind.NoApplicableOperators) ? ConversionKind.NoConversion : (isImplicit ? ConversionKind.ImplicitUserDefined : ConversionKind.ExplicitUserDefined));
		_uncommonData = ((conversionResult.Kind == UserDefinedConversionResultKind.NoApplicableOperators && conversionResult.Results.IsEmpty) ? MethodUncommonData.NoApplicableOperators : new MethodUncommonData(isExtensionMethod: false, isArrayIndex: false, conversionResult, null));
	}

	internal Conversion(ConversionKind kind, MethodSymbol conversionMethod, bool isExtensionMethod)
	{
		_kind = kind;
		_uncommonData = new MethodUncommonData(isExtensionMethod, isArrayIndex: false, default(UserDefinedConversionResult), conversionMethod);
	}

	internal Conversion(ConversionKind kind, ImmutableArray<Conversion> nestedConversions)
	{
		_kind = kind;
		_uncommonData = new NestedUncommonData(nestedConversions);
	}

	internal Conversion(ConversionKind kind, DeconstructMethodInfo deconstructMethodInfo, ImmutableArray<(BoundValuePlaceholder? placeholder, BoundExpression? conversion)> deconstructConversionInfo)
	{
		_kind = kind;
		_uncommonData = new DeconstructionUncommonData(deconstructMethodInfo, deconstructConversionInfo);
	}

	internal Conversion SetConversionMethod(MethodSymbol conversionMethod)
	{
		return new Conversion(Kind, conversionMethod, IsExtensionMethod);
	}

	internal Conversion SetArrayIndexConversionForDynamic()
	{
		return new Conversion(_kind, new MethodUncommonData(isExtensionMethod: false, isArrayIndex: true, default(UserDefinedConversionResult), null));
	}

	[Conditional("DEBUG")]
	private static void AssertTrivialConversion(ConversionKind kind)
	{
		switch (kind)
		{
		}
	}

	internal static Conversion GetTrivialConversion(ConversionKind kind)
	{
		return new Conversion(kind);
	}

	internal static Conversion MakeStackAllocToPointerType(Conversion underlyingConversion)
	{
		return new Conversion(ConversionKind.StackAllocToPointerType, ImmutableArray.Create(underlyingConversion));
	}

	internal static Conversion MakeStackAllocToSpanType(Conversion underlyingConversion)
	{
		return new Conversion(ConversionKind.StackAllocToSpanType, ImmutableArray.Create(underlyingConversion));
	}

	internal static Conversion MakeNullableConversion(ConversionKind kind, Conversion nestedConversion)
	{
		return nestedConversion.Kind switch
		{
			ConversionKind.Identity => (kind == ConversionKind.ImplicitNullable) ? ImplicitNullableWithIdentityUnderlying : ExplicitNullableWithIdentityUnderlying, 
			ConversionKind.ImplicitConstant => (kind == ConversionKind.ImplicitNullable) ? ImplicitNullableWithImplicitConstantUnderlying : ExplicitNullableWithImplicitConstantUnderlying, 
			ConversionKind.ImplicitNumeric => (kind == ConversionKind.ImplicitNullable) ? ImplicitNullableWithImplicitNumericUnderlying : ExplicitNullableWithImplicitNumericUnderlying, 
			ConversionKind.ExplicitNumeric => (kind == ConversionKind.ImplicitNullable) ? ImplicitNullableWithExplicitNumericUnderlying : ExplicitNullableWithExplicitNumericUnderlying, 
			ConversionKind.ExplicitEnumeration => (kind == ConversionKind.ImplicitNullable) ? ImplicitNullableWithExplicitEnumerationUnderlying : ExplicitNullableWithExplicitEnumerationUnderlying, 
			ConversionKind.ExplicitPointerToInteger => (kind == ConversionKind.ImplicitNullable) ? ImplicitNullableWithPointerToIntegerUnderlying : ExplicitNullableWithPointerToIntegerUnderlying, 
			_ => new Conversion(kind, ImmutableArray.Create(nestedConversion)), 
		};
	}

	internal static Conversion MakeSwitchExpression(ImmutableArray<Conversion> innerConversions)
	{
		return new Conversion(ConversionKind.SwitchExpression, innerConversions);
	}

	internal static Conversion MakeConditionalExpression(ImmutableArray<Conversion> innerConversions)
	{
		return new Conversion(ConversionKind.ConditionalExpression, innerConversions);
	}

	[Conditional("DEBUG")]
	internal void AssertUnderlyingConversionsChecked()
	{
	}

	[Conditional("DEBUG")]
	internal void AssertUnderlyingConversionsCheckedRecursive()
	{
		ImmutableArray<Conversion> underlyingConversions = UnderlyingConversions;
		if (!underlyingConversions.IsDefaultOrEmpty)
		{
			foreach (Conversion item in underlyingConversions)
			{
				_ = item;
			}
		}
		_ = IsUserDefined;
	}

	[Conditional("DEBUG")]
	internal void MarkUnderlyingConversionsChecked()
	{
	}

	[Conditional("DEBUG")]
	internal void MarkUnderlyingConversionsCheckedRecursive()
	{
	}

	internal CollectionExpressionTypeKind GetCollectionExpressionTypeKind(out TypeSymbol? elementType, out MethodSymbol? constructor, out bool isExpanded)
	{
		if (_uncommonData is CollectionExpressionUncommonData collectionExpressionUncommonData)
		{
			elementType = collectionExpressionUncommonData.ElementType;
			constructor = collectionExpressionUncommonData.Constructor;
			isExpanded = collectionExpressionUncommonData.ConstructorUsedInExpandedForm;
			return collectionExpressionUncommonData.CollectionExpressionTypeKind;
		}
		elementType = null;
		constructor = null;
		isExpanded = false;
		return CollectionExpressionTypeKind.None;
	}

	public CommonConversion ToCommonConversion()
	{
		IMethodSymbol methodSymbol;
		ITypeSymbol constrainedToType;
		if (!IsUserDefined)
		{
			methodSymbol = null;
			constrainedToType = null;
		}
		else
		{
			IMethodSymbol? methodSymbol2 = MethodSymbol;
			ITypeSymbol constrainedToType2 = ConstrainedToType;
			constrainedToType = constrainedToType2;
			methodSymbol = methodSymbol2;
		}
		return new CommonConversion(Exists, IsIdentity, IsNumeric, IsReference, IsImplicit, IsNullable, methodSymbol, constrainedToType);
	}

	public override string ToString()
	{
		return Kind.ToString();
	}

	public override bool Equals(object? obj)
	{
		if (obj is Conversion)
		{
			return Equals((Conversion)obj);
		}
		return false;
	}

	public bool Equals(Conversion other)
	{
		if (Kind == other.Kind)
		{
			return Method == other.Method;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Method, (int)Kind);
	}

	public static bool operator ==(Conversion left, Conversion right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Conversion left, Conversion right)
	{
		return !(left == right);
	}
}
