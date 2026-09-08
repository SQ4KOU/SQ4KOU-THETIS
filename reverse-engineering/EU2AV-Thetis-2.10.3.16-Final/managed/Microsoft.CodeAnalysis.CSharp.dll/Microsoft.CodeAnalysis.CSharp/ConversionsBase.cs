using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class ConversionsBase
{
	private static class ConversionEasyOut
	{
		private static readonly byte[,] s_convkind;

		static ConversionEasyOut()
		{
			s_convkind = new byte[32, 32]
			{
				{
					2, 27, 28, 28, 28, 28, 28, 28, 28, 28,
					28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
					28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
					28, 28
				},
				{
					12, 2, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1
				},
				{
					13, 1, 2, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 10, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1
				},
				{
					13, 1, 1, 2, 24, 24, 3, 3, 24, 3,
					3, 3, 3, 3, 3, 3, 3, 1, 10, 26,
					26, 10, 10, 26, 10, 10, 10, 10, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 2, 3, 3, 3, 24, 24,
					24, 24, 3, 24, 3, 3, 3, 1, 26, 10,
					10, 10, 10, 26, 26, 26, 26, 10, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 2, 3, 3, 24, 24,
					24, 24, 3, 24, 3, 3, 3, 1, 26, 26,
					10, 10, 10, 26, 26, 26, 26, 10, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 2, 3, 24, 24,
					24, 24, 3, 24, 3, 3, 3, 1, 26, 26,
					26, 10, 10, 26, 26, 26, 26, 10, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 24, 2, 24, 24,
					24, 24, 24, 24, 3, 3, 3, 1, 26, 26,
					26, 26, 10, 26, 26, 26, 26, 26, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 3, 3, 3, 2, 3,
					3, 3, 3, 3, 3, 3, 3, 1, 26, 26,
					10, 10, 10, 10, 10, 10, 10, 10, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 3, 3, 24, 2,
					3, 3, 3, 3, 3, 3, 3, 1, 26, 26,
					26, 10, 10, 26, 10, 10, 10, 10, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 24, 3, 24, 24,
					2, 3, 24, 3, 3, 3, 3, 1, 26, 26,
					26, 26, 10, 26, 26, 10, 10, 26, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 24, 24, 24, 24,
					24, 2, 24, 24, 3, 3, 3, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 10, 26, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 24, 3, 24, 24,
					24, 24, 2, 24, 3, 3, 3, 1, 26, 26,
					26, 26, 10, 26, 26, 26, 26, 10, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 24, 24, 24, 24,
					24, 3, 24, 2, 3, 3, 3, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 10, 26, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 24, 24, 24, 24, 24, 24, 24,
					24, 24, 24, 24, 2, 3, 24, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 26, 26, 10,
					10, 26
				},
				{
					13, 1, 1, 24, 24, 24, 24, 24, 24, 24,
					24, 24, 24, 24, 24, 2, 24, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
					10, 26
				},
				{
					13, 1, 1, 24, 24, 24, 24, 24, 24, 24,
					24, 24, 24, 24, 24, 24, 2, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
					26, 10
				},
				{
					13, 1, 26, 1, 1, 1, 1, 1, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 2, 1, 1,
					1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
					1, 1
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 2, 26,
					26, 10, 10, 26, 10, 10, 10, 10, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 2,
					10, 10, 10, 26, 26, 26, 26, 10, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					2, 10, 10, 26, 26, 26, 26, 10, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 2, 10, 26, 26, 26, 26, 10, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 2, 26, 26, 26, 26, 26, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					10, 10, 10, 2, 10, 10, 10, 10, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 10, 10, 26, 2, 10, 10, 10, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 10, 26, 26, 2, 10, 26, 10, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 2, 26, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 10, 26, 26, 26, 26, 2, 26, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 10, 26, 2, 10,
					10, 10
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 26, 26, 2,
					10, 26
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
					2, 26
				},
				{
					13, 1, 1, 26, 26, 26, 26, 26, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 1, 26, 26,
					26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
					26, 2
				}
			};
		}

		public static ConversionKind ClassifyConversion(TypeSymbol source, TypeSymbol target)
		{
			int num = source.TypeToIndex();
			if (num < 0)
			{
				return ConversionKind.NoConversion;
			}
			int num2 = target.TypeToIndex();
			if (num2 < 0)
			{
				return ConversionKind.NoConversion;
			}
			return (ConversionKind)s_convkind[num, num2];
		}
	}

	private delegate Conversion ClassifyConversionFromExpressionDelegate(ConversionsBase conversions, BoundExpression sourceExpression, TypeWithAnnotations destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast);

	private delegate Conversion ClassifyConversionFromTypeDelegate(ConversionsBase conversions, TypeWithAnnotations source, TypeWithAnnotations destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast);

	private const int MaximumRecursionDepth = 50;

	protected readonly AssemblySymbol corLibrary;

	protected readonly int currentRecursionDepth;

	internal readonly bool IncludeNullability;

	private ConversionsBase _lazyOtherNullability;

	protected abstract bool IsAttributeArgumentBinding { get; }

	protected abstract bool IsParameterDefaultValueBinding { get; }

	internal AssemblySymbol CorLibrary => corLibrary;

	protected abstract CSharpCompilation? Compilation { get; }

	private bool IsFeatureFirstClassSpanEnabled => Compilation?.IsFeatureEnabled(MessageID.IDS_FeatureFirstClassSpan) ?? true;

	protected ConversionsBase(AssemblySymbol corLibrary, int currentRecursionDepth, bool includeNullability, ConversionsBase otherNullabilityOpt)
	{
		this.corLibrary = corLibrary;
		this.currentRecursionDepth = currentRecursionDepth;
		IncludeNullability = includeNullability;
		_lazyOtherNullability = otherNullabilityOpt;
	}

	internal ConversionsBase WithNullability(bool includeNullability)
	{
		if (IncludeNullability == includeNullability)
		{
			return this;
		}
		if (_lazyOtherNullability == null)
		{
			Interlocked.CompareExchange(ref _lazyOtherNullability, WithNullabilityCore(includeNullability), null);
		}
		return _lazyOtherNullability;
	}

	protected abstract ConversionsBase WithNullabilityCore(bool includeNullability);

	public abstract Conversion GetMethodGroupDelegateConversion(BoundMethodGroup source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo);

	public abstract Conversion GetMethodGroupFunctionPointerConversion(BoundMethodGroup source, FunctionPointerTypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo);

	public abstract Conversion GetStackAllocConversion(BoundStackAllocArrayCreation sourceExpression, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo);

	protected abstract ConversionsBase CreateInstance(int currentRecursionDepth);

	protected abstract Conversion GetInterpolatedStringConversion(BoundExpression source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo);

	protected abstract Conversion GetCollectionExpressionConversion(BoundUnconvertedCollectionExpression source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo);

	public Conversion ClassifyImplicitConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeSymbol type = sourceExpression.Type;
		if ((object)type != null && HasIdentityConversionInternal(type, destination))
		{
			return Conversion.Identity;
		}
		Conversion result = ClassifyImplicitBuiltInConversionFromExpression(sourceExpression, type, destination, ref useSiteInfo);
		if (result.Exists)
		{
			return result;
		}
		if ((object)type != null)
		{
			Conversion result2 = FastClassifyConversion(type, destination);
			if (result2.Exists)
			{
				if (result2.IsImplicit)
				{
					return result2;
				}
			}
			else
			{
				result = ClassifyImplicitBuiltInConversionSlow(type, destination, ref useSiteInfo);
				if (result.Exists)
				{
					return result;
				}
			}
		}
		else
		{
			FunctionTypeSymbol functionType = sourceExpression.GetFunctionType();
			if ((object)functionType != null && HasImplicitFunctionTypeConversion(functionType, destination, ref useSiteInfo))
			{
				return Conversion.FunctionType;
			}
		}
		result = GetImplicitUserDefinedConversion(sourceExpression, type, destination, ref useSiteInfo);
		if (result.Exists)
		{
			return result;
		}
		result = GetSwitchExpressionConversion(sourceExpression, destination, ref useSiteInfo);
		if (result.Exists)
		{
			return result;
		}
		return GetConditionalExpressionConversion(sourceExpression, destination, ref useSiteInfo);
	}

	public Conversion ClassifyImplicitConversionFromType(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (HasIdentityConversionInternal(source, destination))
		{
			return Conversion.Identity;
		}
		Conversion result = FastClassifyConversion(source, destination);
		if (result.Exists)
		{
			if (!result.IsImplicit)
			{
				return Conversion.NoConversion;
			}
			return result;
		}
		Conversion result2 = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteInfo);
		if (result2.Exists)
		{
			return result2;
		}
		return GetImplicitUserDefinedConversion(source, destination, ref useSiteInfo);
	}

	public Conversion ClassifyImplicitConversionFromTypeWhenNeitherOrBothFunctionTypes(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		FunctionTypeSymbol functionTypeSymbol = source as FunctionTypeSymbol;
		FunctionTypeSymbol functionTypeSymbol2 = destination as FunctionTypeSymbol;
		if ((object)functionTypeSymbol == null && (object)functionTypeSymbol2 == null)
		{
			return ClassifyImplicitConversionFromType(source, destination, ref useSiteInfo);
		}
		if ((object)functionTypeSymbol != null && (object)functionTypeSymbol2 != null)
		{
			if (!HasImplicitFunctionTypeToFunctionTypeConversion(functionTypeSymbol, functionTypeSymbol2, ref useSiteInfo))
			{
				return Conversion.NoConversion;
			}
			return Conversion.FunctionType;
		}
		return Conversion.NoConversion;
	}

	public Conversion ClassifyConversionFromExpressionType(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (HasImplicitDynamicConversionFromExpression(source, destination))
		{
			return Conversion.ImplicitDynamic;
		}
		return ClassifyConversionFromType(source, destination, isChecked, ref useSiteInfo);
	}

	private static bool TryGetVoidConversion(TypeSymbol source, TypeSymbol destination, out Conversion conversion)
	{
		bool flag = (object)source != null && source.SpecialType == SpecialType.System_Void;
		bool flag2 = destination.SpecialType == SpecialType.System_Void;
		if (flag & flag2)
		{
			conversion = Conversion.Identity;
			return true;
		}
		if (flag | flag2)
		{
			conversion = Conversion.NoConversion;
			return true;
		}
		conversion = default(Conversion);
		return false;
	}

	public Conversion ClassifyConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast = false)
	{
		if (TryGetVoidConversion(sourceExpression.Type, destination, out var conversion))
		{
			return conversion;
		}
		if (forCast)
		{
			return ClassifyConversionFromExpressionForCast(sourceExpression, destination, isChecked, ref useSiteInfo);
		}
		Conversion result = ClassifyImplicitConversionFromExpression(sourceExpression, destination, ref useSiteInfo);
		if (result.Exists)
		{
			return result;
		}
		return ClassifyExplicitOnlyConversionFromExpression(sourceExpression, destination, isChecked, ref useSiteInfo, forCast: false);
	}

	public Conversion ClassifyConversionFromType(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast = false)
	{
		if (TryGetVoidConversion(source, destination, out var conversion))
		{
			return conversion;
		}
		if (forCast)
		{
			return ClassifyConversionFromTypeForCast(source, destination, isChecked, ref useSiteInfo);
		}
		Conversion result = FastClassifyConversion(source, destination);
		if (result.Exists)
		{
			return result;
		}
		Conversion result2 = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteInfo);
		if (result2.Exists)
		{
			return result2;
		}
		Conversion implicitUserDefinedConversion = GetImplicitUserDefinedConversion(source, destination, ref useSiteInfo);
		if (implicitUserDefinedConversion.Exists)
		{
			return implicitUserDefinedConversion;
		}
		implicitUserDefinedConversion = ClassifyExplicitBuiltInOnlyConversion(source, destination, isChecked, ref useSiteInfo, forCast: false);
		if (implicitUserDefinedConversion.Exists)
		{
			return implicitUserDefinedConversion;
		}
		return GetExplicitUserDefinedConversion(source, destination, isChecked, ref useSiteInfo);
	}

	private Conversion ClassifyConversionFromExpressionForCast(BoundExpression source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion conversion = ClassifyImplicitConversionFromExpression(source, destination, ref useSiteInfo);
		if (conversion.Exists && !ExplicitConversionMayDifferFromImplicit(conversion))
		{
			return conversion;
		}
		Conversion result = ClassifyExplicitOnlyConversionFromExpression(source, destination, isChecked, ref useSiteInfo, forCast: true);
		if (result.Exists)
		{
			return result;
		}
		return conversion;
	}

	private Conversion ClassifyConversionFromTypeForCast(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion result = FastClassifyConversion(source, destination);
		if (result.Exists)
		{
			return result;
		}
		Conversion conversion = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteInfo);
		if (conversion.Exists && !ExplicitConversionMayDifferFromImplicit(conversion))
		{
			return conversion;
		}
		Conversion result2 = ClassifyExplicitBuiltInOnlyConversion(source, destination, isChecked, ref useSiteInfo, forCast: true);
		if (result2.Exists)
		{
			return result2;
		}
		if (conversion.Exists)
		{
			return conversion;
		}
		Conversion explicitUserDefinedConversion = GetExplicitUserDefinedConversion(source, destination, isChecked, ref useSiteInfo);
		if (explicitUserDefinedConversion.Exists)
		{
			return explicitUserDefinedConversion;
		}
		return GetImplicitUserDefinedConversion(source, destination, ref useSiteInfo);
	}

	public static Conversion FastClassifyConversion(TypeSymbol source, TypeSymbol target)
	{
		ConversionKind conversionKind = ConversionEasyOut.ClassifyConversion(source, target);
		if (conversionKind != ConversionKind.ImplicitNullable && conversionKind != ConversionKind.ExplicitNullable)
		{
			return Conversion.GetTrivialConversion(conversionKind);
		}
		return Conversion.MakeNullableConversion(conversionKind, FastClassifyConversion(source.StrippedType(), target.StrippedType()));
	}

	public Conversion ClassifyBuiltInConversion(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion result = FastClassifyConversion(source, destination);
		if (result.Exists)
		{
			return result;
		}
		Conversion result2 = ClassifyImplicitBuiltInConversionSlow(source, destination, ref useSiteInfo);
		if (result2.Exists)
		{
			return result2;
		}
		return ClassifyExplicitBuiltInOnlyConversion(source, destination, isChecked, ref useSiteInfo, forCast: false);
	}

	public Conversion ClassifyStandardConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return ClassifyStandardConversion(null, source, destination, ref useSiteInfo);
	}

	public Conversion ClassifyStandardConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion result = ClassifyStandardImplicitConversion(sourceExpression, source, destination, ref useSiteInfo);
		if (result.Exists)
		{
			return result;
		}
		if ((object)source != null)
		{
			return DeriveStandardExplicitFromOppositeStandardImplicitConversion(source, destination, ref useSiteInfo);
		}
		return Conversion.NoConversion;
	}

	private static bool IsStandardImplicitConversionFromType(ConversionKind kind)
	{
		switch (kind)
		{
		case ConversionKind.Identity:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ImplicitNullable:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
		case ConversionKind.ImplicitPointerToVoid:
		case ConversionKind.ImplicitPointer:
		case ConversionKind.ImplicitConstant:
		case ConversionKind.ImplicitSpan:
			return true;
		default:
			return false;
		}
	}

	private Conversion ClassifyStandardImplicitConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion conversion = ClassifyImplicitBuiltInConversionFromExpression(sourceExpression, source, destination, ref useSiteInfo);
		if (conversion.Exists && !conversion.IsInterpolatedStringHandler && !isImplicitCollectionExpressionConversion(conversion))
		{
			return conversion;
		}
		if ((object)source != null)
		{
			return ClassifyStandardImplicitConversion(source, destination, ref useSiteInfo);
		}
		return Conversion.NoConversion;
		static bool isImplicitCollectionExpressionConversion(Conversion conversion2)
		{
			switch (conversion2.Kind)
			{
			case ConversionKind.ImplicitNullable:
			{
				ImmutableArray<Conversion> underlyingConversions = conversion2.UnderlyingConversions;
				if (underlyingConversions.Length == 1 && underlyingConversions[0].Kind == ConversionKind.CollectionExpression)
				{
					return true;
				}
				break;
			}
			case ConversionKind.CollectionExpression:
				return true;
			}
			return false;
		}
	}

	private Conversion ClassifyStandardImplicitConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return classifyConversion(source, destination, ref useSiteInfo);
		Conversion classifyConversion(TypeSymbol typeSymbol, TypeSymbol typeSymbol2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (HasIdentityConversionInternal(typeSymbol, typeSymbol2))
			{
				return Conversion.Identity;
			}
			if (HasImplicitNumericConversion(typeSymbol, typeSymbol2))
			{
				return Conversion.ImplicitNumeric;
			}
			Conversion result = ClassifyImplicitNullableConversion(typeSymbol, typeSymbol2, ref useSiteInfo2);
			if (result.Exists)
			{
				return result;
			}
			if (typeSymbol is FunctionTypeSymbol)
			{
				return Conversion.NoConversion;
			}
			if (HasImplicitReferenceConversion(typeSymbol, typeSymbol2, ref useSiteInfo2))
			{
				return Conversion.ImplicitReference;
			}
			if (HasBoxingConversion(typeSymbol, typeSymbol2, ref useSiteInfo2))
			{
				return Conversion.Boxing;
			}
			if (HasImplicitPointerToVoidConversion(typeSymbol, typeSymbol2))
			{
				return Conversion.PointerToVoid;
			}
			if (HasImplicitPointerConversion(typeSymbol, typeSymbol2, ref useSiteInfo2))
			{
				return Conversion.ImplicitPointer;
			}
			Conversion result2 = ClassifyImplicitTupleConversion(typeSymbol, typeSymbol2, ref useSiteInfo2);
			if (result2.Exists)
			{
				return result2;
			}
			if (HasImplicitSpanConversion(typeSymbol, typeSymbol2, ref useSiteInfo2))
			{
				return Conversion.ImplicitSpan;
			}
			return Conversion.NoConversion;
		}
	}

	private Conversion ClassifyImplicitBuiltInConversionSlow(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (source.IsVoidType() || destination.IsVoidType())
		{
			return Conversion.NoConversion;
		}
		Conversion result = ClassifyStandardImplicitConversion(source, destination, ref useSiteInfo);
		if (result.Exists)
		{
			return result;
		}
		return Conversion.NoConversion;
	}

	private Conversion GetImplicitUserDefinedConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return new Conversion(AnalyzeImplicitUserDefinedConversions(sourceExpression, source, destination, ref useSiteInfo), isImplicit: true);
	}

	private Conversion GetImplicitUserDefinedConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return GetImplicitUserDefinedConversion(null, source, destination, ref useSiteInfo);
	}

	private Conversion ClassifyExplicitBuiltInOnlyConversion(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast)
	{
		if (source.IsVoidType() || destination.IsVoidType())
		{
			return Conversion.NoConversion;
		}
		if (HasSpecialIntPtrConversion(source, destination))
		{
			return Conversion.IntPtr;
		}
		if (HasExplicitEnumerationConversion(source, destination))
		{
			return Conversion.ExplicitEnumeration;
		}
		Conversion result = ClassifyExplicitNullableConversion(source, destination, isChecked, ref useSiteInfo, forCast);
		if (result.Exists)
		{
			return result;
		}
		if (HasExplicitReferenceConversion(source, destination, ref useSiteInfo))
		{
			if (source.Kind != SymbolKind.DynamicType)
			{
				return Conversion.ExplicitReference;
			}
			return Conversion.ExplicitDynamic;
		}
		if (HasUnboxingConversion(source, destination, ref useSiteInfo))
		{
			return Conversion.Unboxing;
		}
		Conversion result2 = ClassifyExplicitTupleConversion(source, destination, isChecked, ref useSiteInfo, forCast);
		if (result2.Exists)
		{
			return result2;
		}
		if (HasPointerToPointerConversion(source, destination))
		{
			return Conversion.PointerToPointer;
		}
		if (HasPointerToIntegerConversion(source, destination))
		{
			return Conversion.PointerToInteger;
		}
		if (HasIntegerToPointerConversion(source, destination))
		{
			return Conversion.IntegerToPointer;
		}
		if (HasExplicitDynamicConversion(source, destination))
		{
			return Conversion.ExplicitDynamic;
		}
		if (HasExplicitSpanConversion(source, destination, ref useSiteInfo))
		{
			return Conversion.ExplicitSpan;
		}
		return Conversion.NoConversion;
	}

	private Conversion GetExplicitUserDefinedConversion(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return new Conversion(AnalyzeExplicitUserDefinedConversions(sourceExpression, source, destination, isChecked, ref useSiteInfo), isImplicit: false);
	}

	private Conversion GetExplicitUserDefinedConversion(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return GetExplicitUserDefinedConversion(null, source, destination, isChecked, ref useSiteInfo);
	}

	private Conversion DeriveStandardExplicitFromOppositeStandardImplicitConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion conversion = ClassifyStandardImplicitConversion(destination, source, ref useSiteInfo);
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
			return Conversion.Identity;
		case ConversionKind.ImplicitNumeric:
			return Conversion.ExplicitNumeric;
		case ConversionKind.ImplicitReference:
			return Conversion.ExplicitReference;
		case ConversionKind.Boxing:
			return Conversion.Unboxing;
		case ConversionKind.NoConversion:
			return Conversion.NoConversion;
		case ConversionKind.ImplicitPointerToVoid:
			return Conversion.PointerToPointer;
		case ConversionKind.ImplicitTuple:
			return Conversion.NoConversion;
		case ConversionKind.ImplicitNullable:
		{
			TypeSymbol source2 = source.StrippedType();
			TypeSymbol destination2 = destination.StrippedType();
			Conversion nestedConversion = DeriveStandardExplicitFromOppositeStandardImplicitConversion(source2, destination2, ref useSiteInfo);
			return nestedConversion.Exists ? Conversion.MakeNullableConversion(ConversionKind.ExplicitNullable, nestedConversion) : Conversion.NoConversion;
		}
		case ConversionKind.ImplicitSpan:
			return Conversion.NoConversion;
		default:
			throw ExceptionUtilities.UnexpectedValue(conversion.Kind);
		}
	}

	public bool IsBaseInterface(TypeSymbol baseType, TypeSymbol derivedType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!baseType.IsInterfaceType())
		{
			return false;
		}
		if (!(derivedType is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		foreach (NamedTypeSymbol item in namedTypeSymbol.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
		{
			if (HasIdentityConversionInternal(item, baseType))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBaseClass(TypeSymbol derivedType, TypeSymbol baseType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!baseType.IsClassType())
		{
			return false;
		}
		TypeSymbol typeSymbol = derivedType.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		while ((object)typeSymbol != null)
		{
			if (HasIdentityConversionInternal(typeSymbol, baseType))
			{
				return true;
			}
			typeSymbol = typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		}
		return false;
	}

	private static bool ExplicitConversionMayDifferFromImplicit(Conversion implicitConversion)
	{
		switch (implicitConversion.Kind)
		{
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ImplicitNullable:
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ConditionalExpression:
		case ConversionKind.ImplicitSpan:
			return true;
		default:
			return false;
		}
	}

	private Conversion ClassifyImplicitBuiltInConversionFromExpression(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (HasImplicitDynamicConversionFromExpression(source, destination))
		{
			return Conversion.ImplicitDynamic;
		}
		if (sourceExpression == null)
		{
			return Conversion.NoConversion;
		}
		if (HasImplicitEnumerationConversion(sourceExpression, destination))
		{
			return Conversion.ImplicitEnumeration;
		}
		Conversion result = ClassifyImplicitConstantExpressionConversion(sourceExpression, destination);
		if (result.Exists)
		{
			return result;
		}
		BoundKind kind = sourceExpression.Kind;
		if (kind <= BoundKind.UnconvertedObjectCreationExpression)
		{
			if (kind <= BoundKind.DefaultLiteral)
			{
				if (kind != BoundKind.UnconvertedAddressOfOperator)
				{
					if (kind != BoundKind.BinaryOperator)
					{
						if (kind == BoundKind.DefaultLiteral)
						{
							return Conversion.DefaultLiteral;
						}
					}
					else if (((BoundBinaryOperator)sourceExpression).IsUnconvertedInterpolatedStringAddition)
					{
						goto IL_01bb;
					}
				}
				else if (destination is FunctionPointerTypeSymbol destination2)
				{
					Conversion methodGroupFunctionPointerConversion = GetMethodGroupFunctionPointerConversion(((BoundUnconvertedAddressOfOperator)sourceExpression).Operand, destination2, ref useSiteInfo);
					if (methodGroupFunctionPointerConversion.Exists)
					{
						return methodGroupFunctionPointerConversion;
					}
				}
			}
			else
			{
				switch (kind)
				{
				case BoundKind.Literal:
				{
					Conversion result2 = ClassifyNullLiteralConversion(sourceExpression, destination);
					if (result2.Exists)
					{
						return result2;
					}
					break;
				}
				case BoundKind.MethodGroup:
				{
					Conversion methodGroupDelegateConversion = GetMethodGroupDelegateConversion((BoundMethodGroup)sourceExpression, destination, ref useSiteInfo);
					if (methodGroupDelegateConversion.Exists)
					{
						return methodGroupDelegateConversion;
					}
					break;
				}
				case BoundKind.UnconvertedObjectCreationExpression:
					return Conversion.ObjectCreation;
				}
			}
		}
		else if (kind <= BoundKind.StackAllocArrayCreation)
		{
			switch (kind)
			{
			case BoundKind.TupleLiteral:
			{
				Conversion result3 = ClassifyImplicitTupleLiteralConversion((BoundTupleLiteral)sourceExpression, destination, ref useSiteInfo);
				if (result3.Exists)
				{
					return result3;
				}
				break;
			}
			case BoundKind.StackAllocArrayCreation:
			{
				Conversion stackAllocConversion = GetStackAllocConversion((BoundStackAllocArrayCreation)sourceExpression, destination, ref useSiteInfo);
				if (stackAllocConversion.Exists)
				{
					return stackAllocConversion;
				}
				break;
			}
			case BoundKind.UnconvertedCollectionExpression:
			{
				Conversion implicitCollectionExpressionConversion = GetImplicitCollectionExpressionConversion((BoundUnconvertedCollectionExpression)sourceExpression, destination, ref useSiteInfo);
				if (implicitCollectionExpressionConversion.Exists)
				{
					return implicitCollectionExpressionConversion;
				}
				break;
			}
			}
		}
		else if (kind <= BoundKind.UnconvertedInterpolatedString)
		{
			if (kind != BoundKind.UnboundLambda)
			{
				if (kind == BoundKind.UnconvertedInterpolatedString)
				{
					goto IL_01bb;
				}
			}
			else if (HasAnonymousFunctionConversion(sourceExpression, destination, Compilation))
			{
				return Conversion.AnonymousFunction;
			}
		}
		else
		{
			switch (kind)
			{
			case BoundKind.ExpressionWithNullability:
			{
				BoundExpression expression = ((BoundExpressionWithNullability)sourceExpression).Expression;
				Conversion result4 = ClassifyImplicitBuiltInConversionFromExpression(expression, expression.Type, destination, ref useSiteInfo);
				if (result4.Exists)
				{
					return result4;
				}
				break;
			}
			case BoundKind.ThrowExpression:
				return Conversion.ImplicitThrow;
			}
		}
		goto IL_0248;
		IL_0248:
		if (!IsAttributeArgumentBinding && !IsParameterDefaultValueBinding && (object)source != null && source.HasInlineArrayAttribute(out var _))
		{
			FieldSymbol fieldSymbol = source.TryGetInlineArrayElementField();
			if ((object)fieldSymbol != null)
			{
				TypeWithAnnotations typeWithAnnotations = fieldSymbol.TypeWithAnnotations;
				if ((destination.OriginalDefinition.Equals(Compilation.GetWellKnownType(WellKnownType.System_Span_T), TypeCompareKind.AllIgnoreOptions) || destination.OriginalDefinition.Equals(Compilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T), TypeCompareKind.AllIgnoreOptions)) && HasIdentityConversionInternal(((NamedTypeSymbol)destination.OriginalDefinition).Construct(ImmutableArray.Create(typeWithAnnotations)), destination))
				{
					return Conversion.InlineArray;
				}
			}
		}
		return Conversion.NoConversion;
		IL_01bb:
		Conversion interpolatedStringConversion = GetInterpolatedStringConversion(sourceExpression, destination, ref useSiteInfo);
		if (interpolatedStringConversion.Exists)
		{
			return interpolatedStringConversion;
		}
		goto IL_0248;
	}

	private Conversion GetImplicitCollectionExpressionConversion(BoundUnconvertedCollectionExpression collectionExpression, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion collectionExpressionConversion = GetCollectionExpressionConversion(collectionExpression, destination, ref useSiteInfo);
		if (collectionExpressionConversion.Exists)
		{
			return collectionExpressionConversion;
		}
		if (destination.IsNullableType(out TypeSymbol underlyingType))
		{
			Conversion collectionExpressionConversion2 = GetCollectionExpressionConversion(collectionExpression, underlyingType, ref useSiteInfo);
			if (collectionExpressionConversion2.Exists)
			{
				return new Conversion(ConversionKind.ImplicitNullable, ImmutableArray.Create(collectionExpressionConversion2));
			}
		}
		return Conversion.NoConversion;
	}

	private Conversion GetSwitchExpressionConversion(BoundExpression source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!(source is BoundConvertedSwitchExpression))
		{
			if (source is BoundUnconvertedSwitchExpression boundUnconvertedSwitchExpression)
			{
				ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(boundUnconvertedSwitchExpression.SwitchArms.Length);
				foreach (BoundSwitchExpressionArm switchArm in boundUnconvertedSwitchExpression.SwitchArms)
				{
					Conversion item = ClassifyImplicitConversionFromExpression(switchArm.Value, destination, ref useSiteInfo);
					if (!item.Exists)
					{
						instance.Free();
						return Conversion.NoConversion;
					}
					instance.Add(item);
				}
				return Conversion.MakeSwitchExpression(instance.ToImmutableAndFree());
			}
			return Conversion.NoConversion;
		}
		return Conversion.NoConversion;
	}

	private Conversion GetConditionalExpressionConversion(BoundExpression source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!(source is BoundUnconvertedConditionalOperator boundUnconvertedConditionalOperator))
		{
			return Conversion.NoConversion;
		}
		Conversion item = ClassifyImplicitConversionFromExpression(boundUnconvertedConditionalOperator.Consequence, destination, ref useSiteInfo);
		if (!item.Exists)
		{
			return Conversion.NoConversion;
		}
		Conversion item2 = ClassifyImplicitConversionFromExpression(boundUnconvertedConditionalOperator.Alternative, destination, ref useSiteInfo);
		if (!item2.Exists)
		{
			return Conversion.NoConversion;
		}
		return Conversion.MakeConditionalExpression(ImmutableArray.Create(item, item2));
	}

	private static Conversion ClassifyNullLiteralConversion(BoundExpression source, TypeSymbol destination)
	{
		if (!source.IsLiteralNull())
		{
			return Conversion.NoConversion;
		}
		if (destination.IsNullableType())
		{
			return Conversion.NullLiteral;
		}
		if (destination.IsReferenceType)
		{
			return Conversion.ImplicitReference;
		}
		if (destination.IsPointerOrFunctionPointer())
		{
			return Conversion.NullToPointer;
		}
		return Conversion.NoConversion;
	}

	private static Conversion ClassifyImplicitConstantExpressionConversion(BoundExpression source, TypeSymbol destination)
	{
		if (HasImplicitConstantExpressionConversion(source, destination))
		{
			return Conversion.ImplicitConstant;
		}
		if (destination.Kind == SymbolKind.NamedType && destination.IsNullableType(out TypeSymbol underlyingType) && HasImplicitConstantExpressionConversion(source, underlyingType))
		{
			return Conversion.ImplicitNullableWithImplicitConstantUnderlying;
		}
		return Conversion.NoConversion;
	}

	private Conversion ClassifyImplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion implicitTupleLiteralConversion = GetImplicitTupleLiteralConversion(source, destination, ref useSiteInfo);
		if (implicitTupleLiteralConversion.Exists)
		{
			return implicitTupleLiteralConversion;
		}
		if (destination.IsNullableType(out TypeSymbol underlyingType))
		{
			Conversion implicitTupleLiteralConversion2 = GetImplicitTupleLiteralConversion(source, underlyingType, ref useSiteInfo);
			if (implicitTupleLiteralConversion2.Exists)
			{
				return new Conversion(ConversionKind.ImplicitNullable, ImmutableArray.Create(implicitTupleLiteralConversion2));
			}
		}
		return Conversion.NoConversion;
	}

	private Conversion ClassifyExplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast)
	{
		Conversion explicitTupleLiteralConversion = GetExplicitTupleLiteralConversion(source, destination, isChecked, ref useSiteInfo, forCast);
		if (explicitTupleLiteralConversion.Exists)
		{
			return explicitTupleLiteralConversion;
		}
		if (destination.Kind == SymbolKind.NamedType && destination.IsNullableType(out TypeSymbol underlyingType))
		{
			Conversion explicitTupleLiteralConversion2 = GetExplicitTupleLiteralConversion(source, underlyingType, isChecked, ref useSiteInfo, forCast);
			if (explicitTupleLiteralConversion2.Exists)
			{
				return new Conversion(ConversionKind.ExplicitNullable, ImmutableArray.Create(explicitTupleLiteralConversion2));
			}
		}
		return Conversion.NoConversion;
	}

	internal static bool HasImplicitConstantExpressionConversion(BoundExpression source, TypeSymbol destination)
	{
		ConstantValue constantValueOpt = source.ConstantValueOpt;
		if (constantValueOpt == null || (object)source.Type == null)
		{
			return false;
		}
		switch (source.Type.GetSpecialTypeSafe())
		{
		case SpecialType.System_Int32:
		{
			int num = ((!constantValueOpt.IsBad) ? constantValueOpt.Int32Value : 0);
			switch (destination.GetSpecialTypeSafe())
			{
			case SpecialType.System_Byte:
				if (0 <= num)
				{
					return num <= 255;
				}
				return false;
			case SpecialType.System_SByte:
				if (-128 <= num)
				{
					return num <= 127;
				}
				return false;
			case SpecialType.System_Int16:
				if (-32768 <= num)
				{
					return num <= 32767;
				}
				return false;
			case SpecialType.System_IntPtr:
				if (destination.IsNativeIntegerType)
				{
					return true;
				}
				break;
			case SpecialType.System_UIntPtr:
				if (!destination.IsNativeIntegerType)
				{
					break;
				}
				goto case SpecialType.System_UInt32;
			case SpecialType.System_UInt32:
				return 0L <= (long)num;
			case SpecialType.System_UInt64:
				return 0 <= num;
			case SpecialType.System_UInt16:
				if (0 <= num)
				{
					return num <= 65535;
				}
				return false;
			}
			return false;
		}
		case SpecialType.System_Int64:
			if (destination.GetSpecialTypeSafe() == SpecialType.System_UInt64 && (constantValueOpt.IsBad || 0 <= constantValueOpt.Int64Value))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private Conversion ClassifyExplicitOnlyConversionFromExpression(BoundExpression sourceExpression, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast)
	{
		if (sourceExpression.Kind == BoundKind.TupleLiteral)
		{
			Conversion result = ClassifyExplicitTupleLiteralConversion((BoundTupleLiteral)sourceExpression, destination, isChecked, ref useSiteInfo, forCast);
			if (result.Exists)
			{
				return result;
			}
		}
		TypeSymbol type = sourceExpression.Type;
		if ((object)type != null)
		{
			Conversion result2 = FastClassifyConversion(type, destination);
			if (result2.Exists)
			{
				return result2;
			}
			Conversion result3 = ClassifyExplicitBuiltInOnlyConversion(type, destination, isChecked, ref useSiteInfo, forCast);
			if (result3.Exists)
			{
				return result3;
			}
		}
		return GetExplicitUserDefinedConversion(sourceExpression, type, destination, isChecked, ref useSiteInfo);
	}

	private static bool HasImplicitEnumerationConversion(BoundExpression source, TypeSymbol destination)
	{
		if (!destination.IsEnumType() && (!destination.IsNullableType() || !destination.GetNullableUnderlyingType().IsEnumType()))
		{
			return false;
		}
		ConstantValue constantValueOpt = source.ConstantValueOpt;
		if (constantValueOpt != null && (object)source.Type != null && IsNumericType(source.Type))
		{
			return IsConstantNumericZero(constantValueOpt);
		}
		return false;
	}

	private static LambdaConversionResult IsAnonymousFunctionCompatibleWithDelegate(UnboundLambda anonymousFunction, TypeSymbol type, CSharpCompilation compilation, bool isTargetExpressionTree)
	{
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
		MethodSymbol delegateInvokeMethod = namedTypeSymbol.DelegateInvokeMethod;
		if ((object)delegateInvokeMethod == null || delegateInvokeMethod.HasUseSiteError)
		{
			return LambdaConversionResult.BadTargetType;
		}
		if (anonymousFunction.HasExplicitReturnType(out RefKind refKind, out ImmutableArray<CustomModifier> _, out TypeWithAnnotations returnType) && (delegateInvokeMethod.RefKind != refKind || !delegateInvokeMethod.ReturnType.Equals(returnType.Type, TypeCompareKind.AllIgnoreOptions)))
		{
			return LambdaConversionResult.MismatchedReturnType;
		}
		ImmutableArray<ParameterSymbol> parameters = delegateInvokeMethod.Parameters;
		if (anonymousFunction.HasSignature)
		{
			if (anonymousFunction.ParameterCount != delegateInvokeMethod.ParameterCount)
			{
				return LambdaConversionResult.BadParameterCount;
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				if (!OverloadResolution.AreRefsCompatibleForMethodConversion(anonymousFunction.RefKind(i), parameters[i].RefKind, compilation))
				{
					return LambdaConversionResult.MismatchedParameterRefKind;
				}
			}
			if (anonymousFunction.HasExplicitlyTypedParameterList)
			{
				for (int j = 0; j < parameters.Length; j++)
				{
					if (!parameters[j].Type.Equals(anonymousFunction.ParameterType(j), TypeCompareKind.AllIgnoreOptions))
					{
						return LambdaConversionResult.MismatchedParameterType;
					}
				}
			}
			else
			{
				for (int k = 0; k < parameters.Length; k++)
				{
					if (parameters[k].TypeWithAnnotations.IsStatic)
					{
						return LambdaConversionResult.StaticTypeInImplicitlyTypedLambda;
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < parameters.Length; l++)
			{
				if (parameters[l].RefKind == RefKind.Out)
				{
					return LambdaConversionResult.MissingSignatureWithOutParameter;
				}
			}
		}
		if (ErrorFacts.PreventsSuccessfulDelegateConversion(anonymousFunction.Bind(namedTypeSymbol, isTargetExpressionTree).Diagnostics.Diagnostics))
		{
			return LambdaConversionResult.BindingFailed;
		}
		return LambdaConversionResult.Success;
	}

	private static LambdaConversionResult IsAnonymousFunctionCompatibleWithExpressionTree(UnboundLambda anonymousFunction, NamedTypeSymbol type, CSharpCompilation compilation)
	{
		TypeSymbol type2 = type.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0].Type;
		if (!type2.IsDelegateType())
		{
			return LambdaConversionResult.ExpressionTreeMustHaveDelegateTypeArgument;
		}
		if (anonymousFunction.Syntax.Kind() == SyntaxKind.AnonymousMethodExpression)
		{
			return LambdaConversionResult.ExpressionTreeFromAnonymousMethod;
		}
		return IsAnonymousFunctionCompatibleWithDelegate(anonymousFunction, type2, compilation, isTargetExpressionTree: true);
	}

	internal bool IsAssignableFromMulticastDelegate(TypeSymbol type, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol specialType = corLibrary.GetSpecialType(SpecialType.System_MulticastDelegate);
		specialType.AddUseSiteInfo(ref useSiteInfo);
		return ClassifyImplicitConversionFromType(specialType, type, ref useSiteInfo).Exists;
	}

	public static LambdaConversionResult IsAnonymousFunctionCompatibleWithType(UnboundLambda anonymousFunction, TypeSymbol type, CSharpCompilation compilation)
	{
		if (type.IsDelegateType())
		{
			return IsAnonymousFunctionCompatibleWithDelegate(anonymousFunction, type, compilation, isTargetExpressionTree: false);
		}
		if (type.IsExpressionTree())
		{
			return IsAnonymousFunctionCompatibleWithExpressionTree(anonymousFunction, (NamedTypeSymbol)type, compilation);
		}
		return LambdaConversionResult.BadTargetType;
	}

	private static bool HasAnonymousFunctionConversion(BoundExpression source, TypeSymbol destination, CSharpCompilation compilation)
	{
		if (source.Kind != BoundKind.UnboundLambda)
		{
			return false;
		}
		return IsAnonymousFunctionCompatibleWithType((UnboundLambda)source, destination, compilation) == LambdaConversionResult.Success;
	}

	internal static CollectionExpressionTypeKind GetCollectionExpressionTypeKind(CSharpCompilation compilation, TypeSymbol destination, out TypeWithAnnotations elementType)
	{
		if (destination is ArrayTypeSymbol arrayTypeSymbol)
		{
			if (arrayTypeSymbol.IsSZArray)
			{
				elementType = arrayTypeSymbol.ElementTypeWithAnnotations;
				return CollectionExpressionTypeKind.Array;
			}
		}
		else
		{
			if (IsSpanOrListType(compilation, destination, WellKnownType.System_Span_T, out elementType))
			{
				return CollectionExpressionTypeKind.Span;
			}
			if (IsSpanOrListType(compilation, destination, WellKnownType.System_ReadOnlySpan_T, out elementType))
			{
				return CollectionExpressionTypeKind.ReadOnlySpan;
			}
			NamedTypeSymbol obj = destination as NamedTypeSymbol;
			if ((object)obj != null && obj.HasCollectionBuilderAttribute(out TypeSymbol _, out string _))
			{
				elementType = default(TypeWithAnnotations);
				return CollectionExpressionTypeKind.CollectionBuilder;
			}
			if (implementsSpecialInterface(compilation, destination, SpecialType.System_Collections_IEnumerable))
			{
				elementType = default(TypeWithAnnotations);
				return CollectionExpressionTypeKind.ImplementsIEnumerable;
			}
			if (destination.IsArrayInterface(out elementType))
			{
				return CollectionExpressionTypeKind.ArrayInterface;
			}
		}
		elementType = default(TypeWithAnnotations);
		return CollectionExpressionTypeKind.None;
		static bool implementsSpecialInterface(CSharpCompilation cSharpCompilation, TypeSymbol targetType, SpecialType specialInterface)
		{
			return targetType.GetAllInterfacesOrEffectiveInterfaces().Any(arg: cSharpCompilation.GetSpecialType(specialInterface), predicate: (NamedTypeSymbol a, NamedTypeSymbol b) => (object)a.OriginalDefinition == b);
		}
	}

	internal static bool IsSpanOrListType(CSharpCompilation compilation, TypeSymbol targetType, WellKnownType spanType, [NotNullWhen(true)] out TypeWithAnnotations elementType)
	{
		if (targetType is NamedTypeSymbol { Arity: 1 } namedTypeSymbol && (object)namedTypeSymbol.OriginalDefinition == compilation.GetWellKnownType(spanType))
		{
			elementType = namedTypeSymbol.TypeArgumentsWithAnnotationsNoUseSiteDiagnostics[0];
			return true;
		}
		elementType = default(TypeWithAnnotations);
		return false;
	}

	internal Conversion ClassifyImplicitUserDefinedConversionForV6SwitchGoverningType(TypeSymbol sourceType, out TypeSymbol switchGoverningType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		UserDefinedConversionResult conversionResult = AnalyzeImplicitUserDefinedConversionForV6SwitchGoverningType(sourceType, ref useSiteInfo);
		if (conversionResult.Kind == UserDefinedConversionResultKind.Valid)
		{
			UserDefinedConversionAnalysis userDefinedConversionAnalysis = conversionResult.Results[conversionResult.Best];
			switchGoverningType = userDefinedConversionAnalysis.ToType;
		}
		else
		{
			switchGoverningType = null;
		}
		return new Conversion(conversionResult, isImplicit: true);
	}

	internal Conversion GetCallerLineNumberConversion(TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax syntax = new Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax(new Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LiteralExpressionSyntax(SyntaxKind.NumericLiteralExpression, new Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken(SyntaxKind.NumericLiteralToken)), null, 0);
		TypeSymbol specialType = corLibrary.GetSpecialType(SpecialType.System_Int32);
		BoundLiteral source = new BoundLiteral(syntax, ConstantValue.Create(int.MaxValue), specialType);
		if (HasImplicitEnumerationConversion(source, destination))
		{
			return Conversion.ImplicitEnumeration;
		}
		Conversion result = ClassifyImplicitConstantExpressionConversion(source, destination);
		if (result.Exists)
		{
			return result;
		}
		return ClassifyStandardImplicitConversion(specialType, destination, ref useSiteInfo);
	}

	internal bool HasCallerLineNumberConversion(TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return GetCallerLineNumberConversion(destination, ref useSiteInfo).Exists;
	}

	internal bool HasCallerInfoStringConversion(TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeSymbol specialType = corLibrary.GetSpecialType(SpecialType.System_String);
		return ClassifyStandardImplicitConversion(specialType, destination, ref useSiteInfo).Exists;
	}

	public static bool HasIdentityConversion(TypeSymbol type1, TypeSymbol type2)
	{
		return HasIdentityConversionInternal(type1, type2, includeNullability: false);
	}

	private static bool HasIdentityConversionInternal(TypeSymbol type1, TypeSymbol type2, bool includeNullability)
	{
		TypeCompareKind compareKind = (includeNullability ? TypeCompareKind.AllIgnoreOptionsPlusNullableWithObliviousMatchesAny : TypeCompareKind.AllIgnoreOptions);
		return type1.Equals(type2, compareKind);
	}

	private bool HasIdentityConversionInternal(TypeSymbol type1, TypeSymbol type2)
	{
		return HasIdentityConversionInternal(type1, type2, IncludeNullability);
	}

	internal bool HasTopLevelNullabilityIdentityConversion(TypeWithAnnotations source, TypeWithAnnotations destination)
	{
		if (!IncludeNullability)
		{
			return true;
		}
		if (source.NullableAnnotation.IsOblivious() || destination.NullableAnnotation.IsOblivious())
		{
			return true;
		}
		bool flag = IsPossiblyNullableTypeTypeParameter(in source);
		bool flag2 = IsPossiblyNullableTypeTypeParameter(in destination);
		if (flag && !flag2)
		{
			return destination.NullableAnnotation.IsAnnotated();
		}
		if (flag2 && !flag)
		{
			return source.NullableAnnotation.IsAnnotated();
		}
		return source.NullableAnnotation.IsAnnotated() == destination.NullableAnnotation.IsAnnotated();
	}

	internal bool HasTopLevelNullabilityImplicitConversion(TypeWithAnnotations source, TypeWithAnnotations destination)
	{
		if (!IncludeNullability)
		{
			return true;
		}
		if (source.NullableAnnotation.IsOblivious() || destination.NullableAnnotation.IsOblivious() || destination.NullableAnnotation.IsAnnotated())
		{
			return true;
		}
		if (IsPossiblyNullableTypeTypeParameter(in source) && !IsPossiblyNullableTypeTypeParameter(in destination))
		{
			return false;
		}
		return !source.NullableAnnotation.IsAnnotated();
	}

	private static bool IsPossiblyNullableTypeTypeParameter(in TypeWithAnnotations typeWithAnnotations)
	{
		TypeSymbol type = typeWithAnnotations.Type;
		if ((object)type != null)
		{
			if (!type.IsPossiblyNullableReferenceTypeTypeParameter())
			{
				return type.IsNullableTypeOrTypeParameter();
			}
			return true;
		}
		return false;
	}

	public bool HasAnyNullabilityImplicitConversion(TypeWithAnnotations source, TypeWithAnnotations destination)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		if (HasTopLevelNullabilityImplicitConversion(source, destination))
		{
			return ClassifyImplicitConversionFromType(source.Type, destination.Type, ref useSiteInfo).Kind != ConversionKind.NoConversion;
		}
		return false;
	}

	private static bool HasIdentityConversionToAny(NamedTypeSymbol type, ArrayBuilder<(NamedTypeSymbol ParticipatingType, TypeParameterSymbol ConstrainedToTypeOpt)> targetTypes)
	{
		foreach (var targetType in targetTypes)
		{
			if (HasIdentityConversionInternal(type, targetType.ParticipatingType, includeNullability: false))
			{
				return true;
			}
		}
		return false;
	}

	public Conversion ConvertExtensionMethodThisArg(TypeSymbol parameterType, TypeSymbol thisType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool isMethodGroupConversion)
	{
		Conversion conversion = ClassifyImplicitExtensionMethodThisArgConversion(null, thisType, parameterType, ref useSiteInfo, isMethodGroupConversion);
		if (!IsValidExtensionMethodThisArgConversion(conversion))
		{
			return Conversion.NoConversion;
		}
		return conversion;
	}

	public Conversion ClassifyImplicitExtensionMethodThisArgConversion(BoundExpression sourceExpressionOpt, TypeSymbol sourceType, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool isMethodGroupConversion)
	{
		if ((object)sourceType != null)
		{
			if (HasIdentityConversionInternal(sourceType, destination))
			{
				return Conversion.Identity;
			}
			if (HasBoxingConversion(sourceType, destination, ref useSiteInfo))
			{
				return Conversion.Boxing;
			}
			if (HasImplicitReferenceConversion(sourceType, destination, ref useSiteInfo))
			{
				return Conversion.ImplicitReference;
			}
			if (!isMethodGroupConversion && HasImplicitSpanConversion(sourceType, destination, ref useSiteInfo))
			{
				return Conversion.ImplicitSpan;
			}
		}
		if (sourceExpressionOpt != null && sourceExpressionOpt.Kind == BoundKind.TupleLiteral)
		{
			Conversion tupleLiteralConversion = GetTupleLiteralConversion((BoundTupleLiteral)sourceExpressionOpt, destination, ref useSiteInfo, ConversionKind.ImplicitTupleLiteral, delegate(ConversionsBase conversions, BoundExpression s, TypeWithAnnotations d, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> u, bool forCast)
			{
				return conversions.ClassifyImplicitExtensionMethodThisArgConversion(s, s.Type, d.Type, ref u, isMethodGroupConversion: false);
			}, isChecked: false, forCast: false);
			if (tupleLiteralConversion.Exists)
			{
				return tupleLiteralConversion;
			}
		}
		if ((object)sourceType != null)
		{
			Conversion result = ClassifyTupleConversion(sourceType, destination, ref useSiteInfo, ConversionKind.ImplicitTuple, delegate(ConversionsBase conversions, TypeWithAnnotations s, TypeWithAnnotations d, bool _, ref CompoundUseSiteInfo<AssemblySymbol> u, bool _)
			{
				return (!conversions.HasTopLevelNullabilityImplicitConversion(s, d)) ? Conversion.NoConversion : conversions.ClassifyImplicitExtensionMethodThisArgConversion(null, s.Type, d.Type, ref u, isMethodGroupConversion: false);
			}, isChecked: false, forCast: false);
			if (result.Exists)
			{
				return result;
			}
		}
		return Conversion.NoConversion;
	}

	public static bool IsValidExtensionMethodThisArgConversion(Conversion conversion)
	{
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
		case ConversionKind.ImplicitSpan:
			return true;
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
			foreach (Conversion underlyingConversion in conversion.UnderlyingConversions)
			{
				if (!IsValidExtensionMethodThisArgConversion(underlyingConversion))
				{
					return false;
				}
			}
			return true;
		default:
			return false;
		}
	}

	private static ConversionKind GetNumericConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (!IsNumericType(source) || !IsNumericType(destination))
		{
			return ConversionKind.UnsetConversionKind;
		}
		if (source.SpecialType == destination.SpecialType)
		{
			return ConversionKind.UnsetConversionKind;
		}
		return ConversionEasyOut.ClassifyConversion(source, destination);
	}

	private static bool HasImplicitNumericConversion(TypeSymbol source, TypeSymbol destination)
	{
		return GetNumericConversion(source, destination) == ConversionKind.ImplicitNumeric;
	}

	private static bool HasExplicitNumericConversion(TypeSymbol source, TypeSymbol destination)
	{
		return GetNumericConversion(source, destination) == ConversionKind.ExplicitNumeric;
	}

	private static bool IsConstantNumericZero(ConstantValue value)
	{
		switch (value.Discriminator)
		{
		case ConstantValueTypeDiscriminator.SByte:
			return value.SByteValue == 0;
		case ConstantValueTypeDiscriminator.Byte:
			return value.ByteValue == 0;
		case ConstantValueTypeDiscriminator.Int16:
			return value.Int16Value == 0;
		case ConstantValueTypeDiscriminator.Int32:
		case ConstantValueTypeDiscriminator.NInt:
			return value.Int32Value == 0;
		case ConstantValueTypeDiscriminator.Int64:
			return value.Int64Value == 0;
		case ConstantValueTypeDiscriminator.UInt16:
			return value.UInt16Value == 0;
		case ConstantValueTypeDiscriminator.UInt32:
		case ConstantValueTypeDiscriminator.NUInt:
			return value.UInt32Value == 0;
		case ConstantValueTypeDiscriminator.UInt64:
			return value.UInt64Value == 0;
		case ConstantValueTypeDiscriminator.Single:
		case ConstantValueTypeDiscriminator.Double:
			return value.DoubleValue == 0.0;
		case ConstantValueTypeDiscriminator.Decimal:
			return value.DecimalValue == 0m;
		default:
			return false;
		}
	}

	private static bool IsNumericType(TypeSymbol type)
	{
		switch (type.SpecialType)
		{
		case SpecialType.System_IntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_Char;
		case SpecialType.System_UIntPtr:
			if (!type.IsNativeIntegerType)
			{
				break;
			}
			goto case SpecialType.System_Char;
		case SpecialType.System_Char:
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
		case SpecialType.System_Decimal:
		case SpecialType.System_Single:
		case SpecialType.System_Double:
			return true;
		}
		return false;
	}

	private static bool HasSpecialIntPtrConversion(TypeSymbol source, TypeSymbol target)
	{
		TypeSymbol typeSymbol = source.StrippedType();
		TypeSymbol typeSymbol2 = target.StrippedType();
		TypeSymbol typeSymbol3;
		if (isIntPtrOrUIntPtr(typeSymbol))
		{
			typeSymbol3 = typeSymbol2;
		}
		else
		{
			if (!isIntPtrOrUIntPtr(typeSymbol2))
			{
				return false;
			}
			typeSymbol3 = typeSymbol;
		}
		if (typeSymbol3.IsPointerOrFunctionPointer())
		{
			return true;
		}
		if (typeSymbol3.TypeKind == TypeKind.Enum)
		{
			return true;
		}
		SpecialType specialType = typeSymbol3.SpecialType;
		if ((uint)(specialType - 8) <= 11u)
		{
			return true;
		}
		return false;
		static bool isIntPtrOrUIntPtr(TypeSymbol type)
		{
			if (type.SpecialType == SpecialType.System_IntPtr || type.SpecialType == SpecialType.System_UIntPtr)
			{
				return !type.IsNativeIntegerType;
			}
			return false;
		}
	}

	private static bool HasExplicitEnumerationConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (IsNumericType(source) && destination.IsEnumType())
		{
			return true;
		}
		if (IsNumericType(destination) && source.IsEnumType())
		{
			return true;
		}
		if (source.IsEnumType() && destination.IsEnumType())
		{
			return true;
		}
		return false;
	}

	private Conversion ClassifyImplicitNullableConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!destination.IsNullableType())
		{
			return Conversion.NoConversion;
		}
		TypeSymbol nullableUnderlyingType = destination.GetNullableUnderlyingType();
		TypeSymbol typeSymbol = source.StrippedType();
		if (!typeSymbol.IsValueType)
		{
			return Conversion.NoConversion;
		}
		if (HasIdentityConversionInternal(typeSymbol, nullableUnderlyingType))
		{
			return Conversion.ImplicitNullableWithIdentityUnderlying;
		}
		if (HasImplicitNumericConversion(typeSymbol, nullableUnderlyingType))
		{
			return Conversion.ImplicitNullableWithImplicitNumericUnderlying;
		}
		Conversion item = ClassifyImplicitTupleConversion(typeSymbol, nullableUnderlyingType, ref useSiteInfo);
		if (item.Exists)
		{
			return new Conversion(ConversionKind.ImplicitNullable, ImmutableArray.Create(item));
		}
		return Conversion.NoConversion;
	}

	private Conversion GetImplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return GetTupleLiteralConversion(source, destination, ref useSiteInfo, ConversionKind.ImplicitTupleLiteral, delegate(ConversionsBase conversions, BoundExpression s, TypeWithAnnotations d, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> u, bool forCast)
		{
			return conversions.ClassifyImplicitConversionFromExpression(s, d.Type, ref u);
		}, isChecked: false, forCast: false);
	}

	private Conversion GetExplicitTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast)
	{
		return GetTupleLiteralConversion(source, destination, ref useSiteInfo, ConversionKind.ExplicitTupleLiteral, delegate(ConversionsBase conversions, BoundExpression s, TypeWithAnnotations d, bool isChecked2, ref CompoundUseSiteInfo<AssemblySymbol> u, bool forCast2)
		{
			return conversions.ClassifyConversionFromExpression(s, d.Type, isChecked2, ref u, forCast2);
		}, isChecked, forCast);
	}

	private Conversion GetTupleLiteralConversion(BoundTupleLiteral source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, ConversionKind kind, ClassifyConversionFromExpressionDelegate classifyConversion, bool isChecked, bool forCast)
	{
		ImmutableArray<BoundExpression> arguments = source.Arguments;
		if (!destination.IsTupleTypeOfCardinality(arguments.Length))
		{
			return Conversion.NoConversion;
		}
		ImmutableArray<TypeWithAnnotations> tupleElementTypesWithAnnotations = destination.TupleElementTypesWithAnnotations;
		ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(arguments.Length);
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression sourceExpression = arguments[i];
			Conversion item = classifyConversion(this, sourceExpression, tupleElementTypesWithAnnotations[i], isChecked, ref useSiteInfo, forCast);
			if (!item.Exists)
			{
				instance.Free();
				return Conversion.NoConversion;
			}
			instance.Add(item);
		}
		return new Conversion(kind, instance.ToImmutableAndFree());
	}

	private Conversion ClassifyImplicitTupleConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return ClassifyTupleConversion(source, destination, ref useSiteInfo, ConversionKind.ImplicitTuple, delegate(ConversionsBase conversions, TypeWithAnnotations s, TypeWithAnnotations d, bool _, ref CompoundUseSiteInfo<AssemblySymbol> u, bool _)
		{
			return (!conversions.HasTopLevelNullabilityImplicitConversion(s, d)) ? Conversion.NoConversion : conversions.ClassifyImplicitConversionFromType(s.Type, d.Type, ref u);
		}, isChecked: false, forCast: false);
	}

	private Conversion ClassifyExplicitTupleConversion(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast)
	{
		return ClassifyTupleConversion(source, destination, ref useSiteInfo, ConversionKind.ExplicitTuple, delegate(ConversionsBase conversions, TypeWithAnnotations s, TypeWithAnnotations d, bool isChecked2, ref CompoundUseSiteInfo<AssemblySymbol> u, bool forCast2)
		{
			return (!conversions.HasTopLevelNullabilityImplicitConversion(s, d)) ? Conversion.NoConversion : conversions.ClassifyConversionFromType(s.Type, d.Type, isChecked2, ref u, forCast2);
		}, isChecked, forCast);
	}

	private Conversion ClassifyTupleConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, ConversionKind kind, ClassifyConversionFromTypeDelegate classifyConversion, bool isChecked, bool forCast)
	{
		if (!source.TryGetElementTypesWithAnnotationsIfTupleType(out var elementTypes) || !destination.TryGetElementTypesWithAnnotationsIfTupleType(out var elementTypes2) || elementTypes.Length != elementTypes2.Length)
		{
			return Conversion.NoConversion;
		}
		ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(elementTypes.Length);
		for (int i = 0; i < elementTypes.Length; i++)
		{
			Conversion item = classifyConversion(this, elementTypes[i], elementTypes2[i], isChecked, ref useSiteInfo, forCast);
			if (!item.Exists)
			{
				instance.Free();
				return Conversion.NoConversion;
			}
			instance.Add(item);
		}
		return new Conversion(kind, instance.ToImmutableAndFree());
	}

	private Conversion ClassifyExplicitNullableConversion(TypeSymbol source, TypeSymbol destination, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool forCast)
	{
		if (!source.IsNullableType() && !destination.IsNullableType())
		{
			return Conversion.NoConversion;
		}
		TypeSymbol typeSymbol = source.StrippedType();
		TypeSymbol typeSymbol2 = destination.StrippedType();
		if (HasIdentityConversionInternal(typeSymbol, typeSymbol2))
		{
			return Conversion.ExplicitNullableWithIdentityUnderlying;
		}
		if (HasImplicitNumericConversion(typeSymbol, typeSymbol2))
		{
			return Conversion.ExplicitNullableWithImplicitNumericUnderlying;
		}
		if (HasExplicitNumericConversion(typeSymbol, typeSymbol2))
		{
			return Conversion.ExplicitNullableWithExplicitNumericUnderlying;
		}
		Conversion item = ClassifyExplicitTupleConversion(typeSymbol, typeSymbol2, isChecked, ref useSiteInfo, forCast);
		if (item.Exists)
		{
			return new Conversion(ConversionKind.ExplicitNullable, ImmutableArray.Create(item));
		}
		if (HasExplicitEnumerationConversion(typeSymbol, typeSymbol2))
		{
			return Conversion.ExplicitNullableWithExplicitEnumerationUnderlying;
		}
		if (HasPointerToIntegerConversion(typeSymbol, typeSymbol2))
		{
			return Conversion.ExplicitNullableWithPointerToIntegerUnderlying;
		}
		return Conversion.NoConversion;
	}

	private bool HasCovariantArrayConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayTypeSymbol arrayTypeSymbol = source as ArrayTypeSymbol;
		ArrayTypeSymbol arrayTypeSymbol2 = destination as ArrayTypeSymbol;
		if ((object)arrayTypeSymbol == null || (object)arrayTypeSymbol2 == null)
		{
			return false;
		}
		if (!arrayTypeSymbol.HasSameShapeAs(arrayTypeSymbol2))
		{
			return false;
		}
		return HasImplicitReferenceConversion(arrayTypeSymbol.ElementTypeWithAnnotations, arrayTypeSymbol2.ElementTypeWithAnnotations, ref useSiteInfo);
	}

	public bool HasIdentityOrImplicitReferenceConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (HasIdentityConversionInternal(source, destination))
		{
			return true;
		}
		return HasImplicitReferenceConversion(source, destination, ref useSiteInfo);
	}

	private static bool HasImplicitDynamicConversionFromExpression(TypeSymbol expressionType, TypeSymbol destination)
	{
		if ((object)expressionType != null && expressionType.Kind == SymbolKind.DynamicType)
		{
			return !destination.IsPointerOrFunctionPointer();
		}
		return false;
	}

	private static bool HasExplicitDynamicConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.Kind == SymbolKind.DynamicType)
		{
			return !destination.IsPointerOrFunctionPointer();
		}
		return false;
	}

	private bool HasArrayConversionToInterface(ArrayTypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!source.IsSZArray)
		{
			return false;
		}
		if (!destination.IsInterfaceType())
		{
			return false;
		}
		if (destination.SpecialType == SpecialType.System_Collections_IEnumerable)
		{
			return true;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)destination;
		if (namedTypeSymbol.AllTypeArgumentCount() != 1)
		{
			return false;
		}
		if (!namedTypeSymbol.IsPossibleArrayGenericInterface())
		{
			return false;
		}
		TypeWithAnnotations elementTypeWithAnnotations = source.ElementTypeWithAnnotations;
		TypeWithAnnotations destination2 = namedTypeSymbol.TypeArgumentWithDefinitionUseSiteDiagnostics(0, ref useSiteInfo);
		if (IncludeNullability && !HasTopLevelNullabilityImplicitConversion(elementTypeWithAnnotations, destination2))
		{
			return false;
		}
		return HasIdentityOrImplicitReferenceConversion(elementTypeWithAnnotations.Type, destination2.Type, ref useSiteInfo);
	}

	private bool HasImplicitReferenceConversion(TypeWithAnnotations source, TypeWithAnnotations destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (IncludeNullability)
		{
			if (!HasTopLevelNullabilityImplicitConversion(source, destination))
			{
				return false;
			}
			if (source.NullableAnnotation != destination.NullableAnnotation && HasIdentityConversionInternal(source.Type, destination.Type, includeNullability: true))
			{
				return true;
			}
		}
		return HasImplicitReferenceConversion(source.Type, destination.Type, ref useSiteInfo);
	}

	internal bool HasImplicitReferenceConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (source.IsErrorType())
		{
			return false;
		}
		if (!source.IsReferenceType)
		{
			return false;
		}
		if (destination.SpecialType == SpecialType.System_Object || destination.Kind == SymbolKind.DynamicType)
		{
			return true;
		}
		switch (source.TypeKind)
		{
		case TypeKind.Class:
			if (destination.IsClassType() && IsBaseClass(source, destination, ref useSiteInfo))
			{
				return true;
			}
			return HasImplicitConversionToInterface(source, destination, ref useSiteInfo);
		case TypeKind.Interface:
			return HasImplicitConversionToInterface(source, destination, ref useSiteInfo);
		case TypeKind.Delegate:
			return HasImplicitConversionFromDelegate(source, destination, ref useSiteInfo);
		case TypeKind.TypeParameter:
			return HasImplicitReferenceTypeParameterConversion((TypeParameterSymbol)source, destination, ref useSiteInfo);
		case TypeKind.Array:
			return HasImplicitConversionFromArray(source, destination, ref useSiteInfo);
		default:
			return false;
		}
	}

	private bool HasImplicitConversionToInterface(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!destination.IsInterfaceType())
		{
			return false;
		}
		if (source.IsClassType())
		{
			return HasAnyBaseInterfaceConversion(source, destination, ref useSiteInfo);
		}
		if (source.IsInterfaceType())
		{
			if (HasAnyBaseInterfaceConversion(source, destination, ref useSiteInfo))
			{
				return true;
			}
			if (!HasIdentityConversionInternal(source, destination) && HasInterfaceVarianceConversion(source, destination, ref useSiteInfo))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasImplicitConversionFromArray(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!(source is ArrayTypeSymbol source2))
		{
			return false;
		}
		if (HasCovariantArrayConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (destination.GetSpecialTypeSafe() == SpecialType.System_Array)
		{
			return true;
		}
		if (IsBaseInterface(destination, corLibrary.GetDeclaredSpecialType(SpecialType.System_Array), ref useSiteInfo))
		{
			return true;
		}
		if (HasArrayConversionToInterface(source2, destination, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitConversionFromDelegate(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!source.IsDelegateType())
		{
			return false;
		}
		SpecialType specialTypeSafe = destination.GetSpecialTypeSafe();
		if (specialTypeSafe == SpecialType.System_MulticastDelegate || specialTypeSafe == SpecialType.System_Delegate || IsBaseInterface(destination, corLibrary.GetDeclaredSpecialType(SpecialType.System_MulticastDelegate), ref useSiteInfo))
		{
			return true;
		}
		if (HasDelegateVarianceConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitFunctionTypeConversion(FunctionTypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (destination is FunctionTypeSymbol destinationType)
		{
			return HasImplicitFunctionTypeToFunctionTypeConversion(source, destinationType, ref useSiteInfo);
		}
		if (IsValidFunctionTypeConversionTarget(destination, ref useSiteInfo))
		{
			return (object)source.GetInternalDelegateType() != null;
		}
		return false;
	}

	internal bool IsValidFunctionTypeConversionTarget(TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (destination.SpecialType == SpecialType.System_MulticastDelegate)
		{
			return true;
		}
		if (destination.IsNonGenericExpressionType())
		{
			return true;
		}
		NamedTypeSymbol declaredSpecialType = corLibrary.GetDeclaredSpecialType(SpecialType.System_MulticastDelegate);
		if (IsBaseClass(declaredSpecialType, destination, ref useSiteInfo) || IsBaseInterface(destination, declaredSpecialType, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitFunctionTypeToFunctionTypeConversion(FunctionTypeSymbol sourceType, FunctionTypeSymbol destinationType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol internalDelegateType = sourceType.GetInternalDelegateType();
		if ((object)internalDelegateType == null)
		{
			return false;
		}
		NamedTypeSymbol internalDelegateType2 = destinationType.GetInternalDelegateType();
		if ((object)internalDelegateType2 == null)
		{
			return false;
		}
		return HasDelegateVarianceConversion(internalDelegateType, internalDelegateType2, ref useSiteInfo);
	}

	public bool HasImplicitTypeParameterConversion(TypeParameterSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (HasImplicitReferenceTypeParameterConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasImplicitBoxingTypeParameterConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (destination is TypeParameterSymbol { AllowsRefLikeType: false } && !source.AllowsRefLikeType && source.DependsOn((TypeParameterSymbol)destination))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitReferenceTypeParameterConversion(TypeParameterSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (source.IsValueType)
		{
			return false;
		}
		if (source.AllowsRefLikeType)
		{
			return false;
		}
		if (HasImplicitEffectiveBaseConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasImplicitEffectiveInterfaceSetConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (destination is TypeParameterSymbol { AllowsRefLikeType: false } && source.DependsOn((TypeParameterSymbol)destination))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitEffectiveBaseConversion(TypeParameterSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol namedTypeSymbol = source.EffectiveBaseClass(ref useSiteInfo);
		if (HasIdentityConversionInternal(namedTypeSymbol, destination))
		{
			return true;
		}
		if (IsBaseClass(namedTypeSymbol, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasAnyBaseInterfaceConversion(namedTypeSymbol, destination, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	private bool HasImplicitEffectiveInterfaceSetConversion(TypeParameterSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return HasVarianceCompatibleInterfaceInEffectiveInterfaceSet(source, destination, ref useSiteInfo);
	}

	private bool HasVarianceCompatibleInterfaceInEffectiveInterfaceSet(TypeParameterSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!destination.IsInterfaceType())
		{
			return false;
		}
		foreach (NamedTypeSymbol item in source.AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
		{
			if (HasInterfaceVarianceConversion(item, destination, ref useSiteInfo))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasAnyBaseInterfaceConversion(TypeSymbol derivedType, TypeSymbol baseType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return ImplementsVarianceCompatibleInterface(derivedType, baseType, ref useSiteInfo);
	}

	private bool ImplementsVarianceCompatibleInterface(TypeSymbol derivedType, TypeSymbol baseType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!baseType.IsInterfaceType())
		{
			return false;
		}
		if (!(derivedType is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		foreach (NamedTypeSymbol item in namedTypeSymbol.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
		{
			if (HasInterfaceVarianceConversion(item, baseType, ref useSiteInfo))
			{
				return true;
			}
		}
		return false;
	}

	internal bool ImplementsVarianceCompatibleInterface(NamedTypeSymbol derivedType, TypeSymbol baseType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return ImplementsVarianceCompatibleInterface((TypeSymbol)derivedType, baseType, ref useSiteInfo);
	}

	internal bool HasImplicitConversionToOrImplementsVarianceCompatibleInterface(TypeSymbol typeToCheck, NamedTypeSymbol targetInterfaceType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool needSupportForRefStructInterfaces)
	{
		if (ClassifyImplicitConversionFromType(typeToCheck, targetInterfaceType, ref useSiteInfo).IsImplicit)
		{
			needSupportForRefStructInterfaces = false;
			return true;
		}
		if (IsRefLikeOrAllowsRefLikeTypeImplementingVarianceCompatibleInterface(typeToCheck, targetInterfaceType, ref useSiteInfo))
		{
			needSupportForRefStructInterfaces = true;
			return true;
		}
		needSupportForRefStructInterfaces = false;
		return false;
	}

	private bool IsRefLikeOrAllowsRefLikeTypeImplementingVarianceCompatibleInterface(TypeSymbol typeToCheck, NamedTypeSymbol targetInterfaceType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (typeToCheck is TypeParameterSymbol typeParameterSymbol)
		{
			if (typeParameterSymbol.AllowsRefLikeType)
			{
				return HasVarianceCompatibleInterfaceInEffectiveInterfaceSet(typeParameterSymbol, targetInterfaceType, ref useSiteInfo);
			}
			return false;
		}
		if (typeToCheck.IsRefLikeType)
		{
			return ImplementsVarianceCompatibleInterface(typeToCheck, targetInterfaceType, ref useSiteInfo);
		}
		return false;
	}

	internal bool HasImplicitConversionToOrImplementsVarianceCompatibleInterface(BoundExpression expressionToCheck, NamedTypeSymbol targetInterfaceType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, out bool needSupportForRefStructInterfaces)
	{
		if (ClassifyImplicitConversionFromExpression(expressionToCheck, targetInterfaceType, ref useSiteInfo).IsImplicit)
		{
			needSupportForRefStructInterfaces = false;
			return true;
		}
		TypeSymbol type = expressionToCheck.Type;
		if ((object)type != null && IsRefLikeOrAllowsRefLikeTypeImplementingVarianceCompatibleInterface(type, targetInterfaceType, ref useSiteInfo))
		{
			needSupportForRefStructInterfaces = true;
			return true;
		}
		needSupportForRefStructInterfaces = false;
		return false;
	}

	private bool HasInterfaceVarianceConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol namedTypeSymbol = source as NamedTypeSymbol;
		NamedTypeSymbol namedTypeSymbol2 = destination as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null || (object)namedTypeSymbol2 == null)
		{
			return false;
		}
		if (!namedTypeSymbol.IsInterfaceType() || !namedTypeSymbol2.IsInterfaceType())
		{
			return false;
		}
		return HasVariantConversion(namedTypeSymbol, namedTypeSymbol2, ref useSiteInfo);
	}

	private bool HasDelegateVarianceConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol namedTypeSymbol = source as NamedTypeSymbol;
		NamedTypeSymbol namedTypeSymbol2 = destination as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null || (object)namedTypeSymbol2 == null)
		{
			return false;
		}
		if (!namedTypeSymbol.IsDelegateType() || !namedTypeSymbol2.IsDelegateType())
		{
			return false;
		}
		return HasVariantConversion(namedTypeSymbol, namedTypeSymbol2, ref useSiteInfo);
	}

	private bool HasVariantConversion(NamedTypeSymbol source, NamedTypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (currentRecursionDepth >= 50)
		{
			return false;
		}
		ThreeState value = HasVariantConversionQuick(source, destination);
		if (value.HasValue())
		{
			return value.Value();
		}
		return CreateInstance(currentRecursionDepth + 1).HasVariantConversionNoCycleCheck(source, destination, ref useSiteInfo);
	}

	private ThreeState HasVariantConversionQuick(NamedTypeSymbol source, NamedTypeSymbol destination)
	{
		if (HasIdentityConversionInternal(source, destination))
		{
			return ThreeState.True;
		}
		if (!TypeSymbol.Equals(source.OriginalDefinition, destination.OriginalDefinition, TypeCompareKind.ConsiderEverything))
		{
			return ThreeState.False;
		}
		return ThreeState.Unknown;
	}

	private bool HasVariantConversionNoCycleCheck(NamedTypeSymbol source, NamedTypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<TypeWithAnnotations> instance = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		ArrayBuilder<TypeWithAnnotations> instance2 = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		ArrayBuilder<TypeWithAnnotations> instance3 = ArrayBuilder<TypeWithAnnotations>.GetInstance();
		try
		{
			source.OriginalDefinition.GetAllTypeArguments(instance, ref useSiteInfo);
			source.GetAllTypeArguments(instance2, ref useSiteInfo);
			destination.GetAllTypeArguments(instance3, ref useSiteInfo);
			for (int i = 0; i < instance.Count; i++)
			{
				TypeWithAnnotations typeWithAnnotations = instance2[i];
				TypeWithAnnotations typeWithAnnotations2 = instance3[i];
				if (HasIdentityConversionInternal(typeWithAnnotations.Type, typeWithAnnotations2.Type) && HasTopLevelNullabilityIdentityConversion(typeWithAnnotations, typeWithAnnotations2))
				{
					continue;
				}
				TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)instance[i].Type;
				switch (typeParameterSymbol.Variance)
				{
				case VarianceKind.None:
					if (isTypeIEquatable(destination.OriginalDefinition) && TypeSymbol.Equals(typeWithAnnotations2.Type, typeWithAnnotations.Type, TypeCompareKind.AllNullableIgnoreOptions) && HasAnyNullabilityImplicitConversion(typeWithAnnotations2, typeWithAnnotations))
					{
						return true;
					}
					return false;
				case VarianceKind.Out:
					if (!HasImplicitReferenceConversion(typeWithAnnotations, typeWithAnnotations2, ref useSiteInfo))
					{
						return false;
					}
					break;
				case VarianceKind.In:
					if (!HasImplicitReferenceConversion(typeWithAnnotations2, typeWithAnnotations, ref useSiteInfo))
					{
						return false;
					}
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(typeParameterSymbol.Variance);
				}
			}
		}
		finally
		{
			instance.Free();
			instance2.Free();
			instance3.Free();
		}
		return true;
		static bool isTypeIEquatable(NamedTypeSymbol type)
		{
			if ((object)type != null && type.IsInterface && type.Name == "IEquatable")
			{
				NamespaceSymbol containingNamespace = type.ContainingNamespace;
				if ((object)containingNamespace != null && containingNamespace.Name == "System")
				{
					NamespaceSymbol containingNamespace2 = containingNamespace.ContainingNamespace;
					if ((object)containingNamespace2 != null && containingNamespace2.IsGlobalNamespace)
					{
						Symbol containingSymbol = type.ContainingSymbol;
						if ((object)containingSymbol != null && containingSymbol.Kind == SymbolKind.Namespace)
						{
							return type.TypeParameters.Length == 1;
						}
					}
				}
			}
			return false;
		}
	}

	private bool HasImplicitBoxingTypeParameterConversion(TypeParameterSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (source.IsReferenceType)
		{
			return false;
		}
		if (source.AllowsRefLikeType)
		{
			return false;
		}
		if (HasImplicitEffectiveBaseConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasImplicitEffectiveInterfaceSetConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (destination is TypeParameterSymbol { AllowsRefLikeType: false } typeParameterSymbol && source.DependsOn(typeParameterSymbol))
		{
			return true;
		}
		if (destination.Kind == SymbolKind.DynamicType)
		{
			return true;
		}
		return false;
	}

	public bool HasBoxingConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (source.TypeKind == TypeKind.TypeParameter && HasImplicitBoxingTypeParameterConversion((TypeParameterSymbol)source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (!source.IsValueType || !destination.IsReferenceType)
		{
			return false;
		}
		if (source.IsNullableType())
		{
			return HasBoxingConversion(source.GetNullableUnderlyingType(), destination, ref useSiteInfo);
		}
		if (source.IsRestrictedType())
		{
			return false;
		}
		if (destination.Kind == SymbolKind.DynamicType)
		{
			return !source.IsPointerOrFunctionPointer();
		}
		if (IsBaseClass(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasAnyBaseInterfaceConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	internal static bool HasImplicitPointerToVoidConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.IsPointerOrFunctionPointer())
		{
			if (destination is PointerTypeSymbol pointerTypeSymbol)
			{
				TypeSymbol pointedAtType = pointerTypeSymbol.PointedAtType;
				if ((object)pointedAtType != null)
				{
					return pointedAtType.SpecialType == SpecialType.System_Void;
				}
			}
			return false;
		}
		return false;
	}

	internal bool HasImplicitPointerConversion(TypeSymbol? source, TypeSymbol? destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (source is FunctionPointerTypeSymbol functionPointerTypeSymbol)
		{
			FunctionPointerMethodSymbol signature = functionPointerTypeSymbol.Signature;
			if ((object)signature != null && destination is FunctionPointerTypeSymbol functionPointerTypeSymbol2)
			{
				FunctionPointerMethodSymbol signature2 = functionPointerTypeSymbol2.Signature;
				if ((object)signature2 != null)
				{
					if (signature.ParameterCount != signature2.ParameterCount || signature.CallingConvention != signature2.CallingConvention)
					{
						return false;
					}
					if (signature.CallingConvention == CallingConvention.Unmanaged && !signature.GetCallingConventionModifiers().SetEqualsWithoutIntermediateHashSet(signature2.GetCallingConventionModifiers()))
					{
						return false;
					}
					for (int i = 0; i < signature.ParameterCount; i++)
					{
						ParameterSymbol parameterSymbol = signature.Parameters[i];
						ParameterSymbol parameterSymbol2 = signature2.Parameters[i];
						if (parameterSymbol.RefKind != parameterSymbol2.RefKind)
						{
							return false;
						}
						if (!hasConversion(parameterSymbol.RefKind, signature2.Parameters[i].TypeWithAnnotations, signature.Parameters[i].TypeWithAnnotations, ref useSiteInfo))
						{
							return false;
						}
					}
					if (signature.RefKind == signature2.RefKind)
					{
						return hasConversion(signature.RefKind, signature.ReturnTypeWithAnnotations, signature2.ReturnTypeWithAnnotations, ref useSiteInfo);
					}
					return false;
				}
			}
		}
		return false;
		bool hasConversion(RefKind refKind, TypeWithAnnotations sourceType, TypeWithAnnotations destinationType, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (refKind == RefKind.None)
			{
				if (!IncludeNullability || HasTopLevelNullabilityImplicitConversion(sourceType, destinationType))
				{
					if (!HasIdentityOrImplicitReferenceConversion(sourceType.Type, destinationType.Type, ref useSiteInfo2) && !HasImplicitPointerToVoidConversion(sourceType.Type, destinationType.Type))
					{
						return HasImplicitPointerConversion(sourceType.Type, destinationType.Type, ref useSiteInfo2);
					}
					return true;
				}
				return false;
			}
			if (!IncludeNullability || HasTopLevelNullabilityIdentityConversion(sourceType, destinationType))
			{
				return HasIdentityConversion(sourceType.Type, destinationType.Type);
			}
			return false;
		}
	}

	private bool HasIdentityOrReferenceConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (HasIdentityConversionInternal(source, destination))
		{
			return true;
		}
		if (HasImplicitReferenceConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasExplicitReferenceConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	private bool HasExplicitReferenceConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (source.SpecialType == SpecialType.System_Object)
		{
			if (destination.IsReferenceType)
			{
				return true;
			}
		}
		else if (source.Kind == SymbolKind.DynamicType && destination.IsReferenceType)
		{
			return true;
		}
		if (destination.IsClassType() && IsBaseClass(destination, source, ref useSiteInfo))
		{
			return true;
		}
		if (source.IsClassType() && destination.IsInterfaceType() && !source.IsSealed && !HasAnyBaseInterfaceConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (source.IsInterfaceType() && destination.IsClassType() && (!destination.IsSealed || HasAnyBaseInterfaceConversion(destination, source, ref useSiteInfo)))
		{
			return true;
		}
		if (source.IsInterfaceType() && destination.IsInterfaceType() && !HasImplicitConversionToInterface(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasExplicitArrayConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasExplicitDelegateConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		if (HasExplicitReferenceTypeParameterConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	private bool HasExplicitReferenceTypeParameterConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeParameterSymbol typeParameterSymbol = source as TypeParameterSymbol;
		TypeParameterSymbol typeParameterSymbol2 = destination as TypeParameterSymbol;
		if (((object)typeParameterSymbol != null && typeParameterSymbol.AllowsRefLikeType) || ((object)typeParameterSymbol2 != null && typeParameterSymbol2.AllowsRefLikeType))
		{
			return false;
		}
		if ((object)typeParameterSymbol2 != null && typeParameterSymbol2.IsReferenceType)
		{
			NamedTypeSymbol namedTypeSymbol = typeParameterSymbol2.EffectiveBaseClass(ref useSiteInfo);
			while ((object)namedTypeSymbol != null)
			{
				if (HasIdentityConversionInternal(namedTypeSymbol, source))
				{
					return true;
				}
				namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
			}
		}
		if ((object)typeParameterSymbol2 != null && source.IsInterfaceType() && typeParameterSymbol2.IsReferenceType)
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && typeParameterSymbol.IsReferenceType && destination.IsInterfaceType() && !HasImplicitReferenceTypeParameterConversion(typeParameterSymbol, destination, ref useSiteInfo))
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && (object)typeParameterSymbol2 != null && typeParameterSymbol2.IsReferenceType && typeParameterSymbol2.DependsOn(typeParameterSymbol))
		{
			return true;
		}
		return false;
	}

	private bool HasUnboxingTypeParameterConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		TypeParameterSymbol typeParameterSymbol = source as TypeParameterSymbol;
		TypeParameterSymbol typeParameterSymbol2 = destination as TypeParameterSymbol;
		if (((object)typeParameterSymbol != null && typeParameterSymbol.AllowsRefLikeType) || ((object)typeParameterSymbol2 != null && typeParameterSymbol2.AllowsRefLikeType))
		{
			return false;
		}
		if ((object)typeParameterSymbol2 != null && !typeParameterSymbol2.IsReferenceType)
		{
			NamedTypeSymbol namedTypeSymbol = typeParameterSymbol2.EffectiveBaseClass(ref useSiteInfo);
			while ((object)namedTypeSymbol != null)
			{
				if (TypeSymbol.Equals(namedTypeSymbol, source, TypeCompareKind.ConsiderEverything))
				{
					return true;
				}
				namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
			}
		}
		if (source.IsInterfaceType() && (object)typeParameterSymbol2 != null && !typeParameterSymbol2.IsReferenceType)
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && !typeParameterSymbol.IsReferenceType && destination.IsInterfaceType() && !HasImplicitReferenceTypeParameterConversion(typeParameterSymbol, destination, ref useSiteInfo))
		{
			return true;
		}
		if ((object)typeParameterSymbol != null && (object)typeParameterSymbol2 != null && !typeParameterSymbol2.IsReferenceType && typeParameterSymbol2.DependsOn(typeParameterSymbol))
		{
			return true;
		}
		return false;
	}

	private bool HasExplicitDelegateConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (destination.IsDelegateType())
		{
			if (source.SpecialType == SpecialType.System_Delegate || source.SpecialType == SpecialType.System_MulticastDelegate)
			{
				return true;
			}
			if (HasImplicitConversionToInterface(corLibrary.GetDeclaredSpecialType(SpecialType.System_Delegate), source, ref useSiteInfo))
			{
				return true;
			}
		}
		if (!source.IsDelegateType() || !destination.IsDelegateType())
		{
			return false;
		}
		if (!TypeSymbol.Equals(source.OriginalDefinition, destination.OriginalDefinition, TypeCompareKind.ConsiderEverything))
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)source;
		NamedTypeSymbol namedTypeSymbol2 = (NamedTypeSymbol)destination;
		NamedTypeSymbol originalDefinition = namedTypeSymbol.OriginalDefinition;
		if (HasIdentityConversionInternal(source, destination))
		{
			return false;
		}
		if (HasDelegateVarianceConversion(source, destination, ref useSiteInfo))
		{
			return false;
		}
		ImmutableArray<TypeWithAnnotations> immutableArray = namedTypeSymbol.TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		ImmutableArray<TypeWithAnnotations> immutableArray2 = namedTypeSymbol2.TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo);
		for (int i = 0; i < immutableArray.Length; i++)
		{
			TypeSymbol type = immutableArray[i].Type;
			TypeSymbol type2 = immutableArray2[i].Type;
			switch (originalDefinition.TypeParameters[i].Variance)
			{
			case VarianceKind.None:
				if (!HasIdentityConversionInternal(type, type2))
				{
					return false;
				}
				break;
			case VarianceKind.Out:
				if (!HasIdentityOrReferenceConversion(type, type2, ref useSiteInfo))
				{
					return false;
				}
				break;
			case VarianceKind.In:
			{
				bool num = HasIdentityConversionInternal(type, type2);
				bool flag = type.IsReferenceType && type2.IsReferenceType;
				if (!(num | flag))
				{
					return false;
				}
				break;
			}
			}
		}
		return true;
	}

	private bool HasExplicitArrayConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayTypeSymbol arrayTypeSymbol = source as ArrayTypeSymbol;
		ArrayTypeSymbol arrayTypeSymbol2 = destination as ArrayTypeSymbol;
		if ((object)arrayTypeSymbol != null && (object)arrayTypeSymbol2 != null)
		{
			if (arrayTypeSymbol.HasSameShapeAs(arrayTypeSymbol2))
			{
				return HasExplicitReferenceConversion(arrayTypeSymbol.ElementType, arrayTypeSymbol2.ElementType, ref useSiteInfo);
			}
			return false;
		}
		if ((object)arrayTypeSymbol2 != null)
		{
			if (source.SpecialType == SpecialType.System_Array)
			{
				return true;
			}
			foreach (NamedTypeSymbol item in corLibrary.GetDeclaredSpecialType(SpecialType.System_Array).AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
			{
				if (HasIdentityConversionInternal(item, source))
				{
					return true;
				}
			}
		}
		if ((object)arrayTypeSymbol != null && arrayTypeSymbol.IsSZArray && destination.IsPossibleArrayGenericInterface() && HasExplicitReferenceConversion(arrayTypeSymbol.ElementType, ((NamedTypeSymbol)destination).TypeArgumentWithDefinitionUseSiteDiagnostics(0, ref useSiteInfo).Type, ref useSiteInfo))
		{
			return true;
		}
		if ((object)arrayTypeSymbol2 != null && arrayTypeSymbol2.IsSZArray)
		{
			SpecialType specialType = source.OriginalDefinition.SpecialType;
			if (specialType == SpecialType.System_Collections_Generic_IList_T || specialType == SpecialType.System_Collections_Generic_ICollection_T || specialType == SpecialType.System_Collections_Generic_IEnumerable_T || specialType == SpecialType.System_Collections_Generic_IReadOnlyList_T || specialType == SpecialType.System_Collections_Generic_IReadOnlyCollection_T)
			{
				TypeSymbol type = ((NamedTypeSymbol)source).TypeArgumentWithDefinitionUseSiteDiagnostics(0, ref useSiteInfo).Type;
				TypeSymbol elementType = arrayTypeSymbol2.ElementType;
				if (HasIdentityConversionInternal(type, elementType))
				{
					return true;
				}
				if (HasImplicitReferenceConversion(type, elementType, ref useSiteInfo))
				{
					return true;
				}
				if (HasExplicitReferenceConversion(type, elementType, ref useSiteInfo))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasUnboxingConversion(TypeSymbol source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (destination.IsPointerOrFunctionPointer())
		{
			return false;
		}
		if (destination.IsRestrictedType())
		{
			return false;
		}
		SpecialType specialType = source.SpecialType;
		if ((specialType == SpecialType.System_Object || specialType == SpecialType.System_ValueType) && destination.IsValueType && !destination.IsNullableType())
		{
			return true;
		}
		if (source.IsInterfaceType() && destination.IsValueType && !destination.IsNullableType() && HasBoxingConversion(destination, source, ref useSiteInfo))
		{
			return true;
		}
		if (source.SpecialType == SpecialType.System_Enum && destination.IsEnumType())
		{
			return true;
		}
		if (source.IsReferenceType && destination.IsNullableType() && HasUnboxingConversion(source, destination.GetNullableUnderlyingType(), ref useSiteInfo))
		{
			return true;
		}
		if (HasUnboxingTypeParameterConversion(source, destination, ref useSiteInfo))
		{
			return true;
		}
		return false;
	}

	private static bool HasPointerToPointerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (source.IsPointerOrFunctionPointer())
		{
			return destination.IsPointerOrFunctionPointer();
		}
		return false;
	}

	private static bool HasPointerToIntegerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (!source.IsPointerOrFunctionPointer())
		{
			return false;
		}
		return IsIntegerTypeSupportingPointerConversions(destination.StrippedType());
	}

	private static bool HasIntegerToPointerConversion(TypeSymbol source, TypeSymbol destination)
	{
		if (!destination.IsPointerOrFunctionPointer())
		{
			return false;
		}
		return IsIntegerTypeSupportingPointerConversions(source);
	}

	private static bool IsIntegerTypeSupportingPointerConversions(TypeSymbol type)
	{
		switch (type.SpecialType)
		{
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
			return true;
		case SpecialType.System_IntPtr:
		case SpecialType.System_UIntPtr:
			return type.IsNativeIntegerType;
		default:
			return false;
		}
	}

	private bool HasImplicitSpanConversion(TypeSymbol? source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)source == null || !IsFeatureFirstClassSpanEnabled)
		{
			return false;
		}
		if (source is ArrayTypeSymbol { IsSZArray: not false, ElementTypeWithAnnotations: var elementTypeWithAnnotations })
		{
			if (destination.IsSpan())
			{
				TypeWithAnnotations destination2 = ((NamedTypeSymbol)destination).TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo)[0];
				return hasIdentityConversion(elementTypeWithAnnotations, destination2);
			}
			if (destination.IsReadOnlySpan())
			{
				TypeWithAnnotations destination3 = ((NamedTypeSymbol)destination).TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo)[0];
				return hasCovariantConversion(elementTypeWithAnnotations, destination3, ref useSiteInfo);
			}
		}
		else if (source.IsSpan() || source.IsReadOnlySpan())
		{
			if (destination.IsReadOnlySpan())
			{
				TypeWithAnnotations source2 = ((NamedTypeSymbol)source).TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo)[0];
				TypeWithAnnotations destination4 = ((NamedTypeSymbol)destination).TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo)[0];
				return hasCovariantConversion(source2, destination4, ref useSiteInfo);
			}
		}
		else if (source.IsStringType() && destination.IsReadOnlySpan())
		{
			return ((NamedTypeSymbol)destination).TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo)[0].SpecialType == SpecialType.System_Char;
		}
		return false;
		bool hasCovariantConversion(TypeWithAnnotations source3, TypeWithAnnotations destination5, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (!hasIdentityConversion(source3, destination5))
			{
				return HasImplicitReferenceConversion(source3, destination5, ref useSiteInfo2);
			}
			return true;
		}
		bool hasIdentityConversion(TypeWithAnnotations source3, TypeWithAnnotations destination5)
		{
			if (HasIdentityConversionInternal(source3.Type, destination5.Type))
			{
				return HasTopLevelNullabilityIdentityConversion(source3, destination5);
			}
			return false;
		}
	}

	private bool HasExplicitSpanConversion(TypeSymbol? source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (!IsFeatureFirstClassSpanEnabled)
		{
			return false;
		}
		if (source is ArrayTypeSymbol { IsSZArray: not false, ElementTypeWithAnnotations: var elementTypeWithAnnotations } && (destination.IsSpan() || destination.IsReadOnlySpan()))
		{
			TypeWithAnnotations destination2 = ((NamedTypeSymbol)destination).TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteInfo)[0];
			if (HasIdentityOrReferenceConversion(elementTypeWithAnnotations.Type, destination2.Type, ref useSiteInfo))
			{
				return HasTopLevelNullabilityIdentityConversion(elementTypeWithAnnotations, destination2);
			}
			return false;
		}
		return false;
	}

	private bool IgnoreUserDefinedSpanConversions(TypeSymbol? source, TypeSymbol? target)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		if ((object)source != null && (object)target != null)
		{
			if (!HasImplicitSpanConversion(source, target, ref useSiteInfo))
			{
				return HasExplicitSpanConversion(source, target, ref useSiteInfo);
			}
			return true;
		}
		return false;
	}

	public static void AddTypesParticipatingInUserDefinedConversion(ArrayBuilder<(NamedTypeSymbol ParticipatingType, TypeParameterSymbol? ConstrainedToTypeOpt)> result, TypeSymbol type, bool includeBaseTypes, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)type == null)
		{
			return;
		}
		type = type.StrippedType();
		bool flag = result.Count > 0;
		if (type is TypeParameterSymbol typeParameterSymbol)
		{
			NamedTypeSymbol type2 = typeParameterSymbol.EffectiveBaseClass(ref useSiteInfo);
			addFromClassOrStruct(result, flag, type2, includeBaseTypes, ref useSiteInfo);
			foreach (NamedTypeSymbol item in includeBaseTypes ? typeParameterSymbol.AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo) : typeParameterSymbol.EffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteInfo))
			{
				if (!flag || !HasIdentityConversionToAny(item, result))
				{
					result.Add((item, typeParameterSymbol));
				}
			}
		}
		else
		{
			addFromClassOrStruct(result, flag, type, includeBaseTypes, ref useSiteInfo);
		}
		static void addFromClassOrStruct(ArrayBuilder<(NamedTypeSymbol ParticipatingType, TypeParameterSymbol? ConstrainedToTypeOpt)> arrayBuilder, bool excludeExisting, TypeSymbol typeSymbol, bool flag2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			if (typeSymbol.IsClassType() || typeSymbol.IsStructType())
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)typeSymbol;
				if (!excludeExisting || !HasIdentityConversionToAny(namedTypeSymbol, arrayBuilder))
				{
					arrayBuilder.Add((namedTypeSymbol, null));
				}
			}
			if (flag2)
			{
				NamedTypeSymbol namedTypeSymbol2 = typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo2);
				while ((object)namedTypeSymbol2 != null)
				{
					if (!excludeExisting || !HasIdentityConversionToAny(namedTypeSymbol2, arrayBuilder))
					{
						arrayBuilder.Add((namedTypeSymbol2, null));
					}
					namedTypeSymbol2 = namedTypeSymbol2.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteInfo2);
				}
			}
		}
	}

	private UserDefinedConversionResult AnalyzeExplicitUserDefinedConversions(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<(NamedTypeSymbol, TypeParameterSymbol)> instance = ArrayBuilder<(NamedTypeSymbol, TypeParameterSymbol)>.GetInstance();
		ComputeUserDefinedExplicitConversionTypeSet(source, target, instance, ref useSiteInfo);
		ArrayBuilder<UserDefinedConversionAnalysis> instance2 = ArrayBuilder<UserDefinedConversionAnalysis>.GetInstance();
		ComputeApplicableUserDefinedExplicitConversionSet(sourceExpression, source, target, isChecked, instance, instance2, ref useSiteInfo);
		instance.Free();
		ImmutableArray<UserDefinedConversionAnalysis> immutableArray = instance2.ToImmutableAndFree();
		if (immutableArray.Length == 0)
		{
			return UserDefinedConversionResult.NoApplicableOperators(immutableArray);
		}
		TypeSymbol typeSymbol = MostSpecificSourceTypeForExplicitUserDefinedConversion(immutableArray, sourceExpression, source, ref useSiteInfo);
		if ((object)typeSymbol == null)
		{
			return UserDefinedConversionResult.NoBestSourceType(immutableArray);
		}
		TypeSymbol typeSymbol2 = MostSpecificTargetTypeForExplicitUserDefinedConversion(immutableArray, target, ref useSiteInfo);
		if ((object)typeSymbol2 == null)
		{
			return UserDefinedConversionResult.NoBestTargetType(immutableArray);
		}
		int? num = MostSpecificConversionOperator(typeSymbol, typeSymbol2, immutableArray);
		if (!num.HasValue)
		{
			return UserDefinedConversionResult.Ambiguous(immutableArray);
		}
		return UserDefinedConversionResult.Valid(immutableArray, num.Value);
	}

	private static void ComputeUserDefinedExplicitConversionTypeSet(TypeSymbol source, TypeSymbol target, ArrayBuilder<(NamedTypeSymbol ParticipatingType, TypeParameterSymbol ConstrainedToTypeOpt)> d, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		AddTypesParticipatingInUserDefinedConversion(d, source, includeBaseTypes: true, ref useSiteInfo);
		AddTypesParticipatingInUserDefinedConversion(d, target, includeBaseTypes: true, ref useSiteInfo);
	}

	private void ComputeApplicableUserDefinedExplicitConversionSet(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, bool isChecked, ArrayBuilder<(NamedTypeSymbol ParticipatingType, TypeParameterSymbol ConstrainedToTypeOpt)> d, ArrayBuilder<UserDefinedConversionAnalysis> u, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		bool flag = false;
		foreach (var item2 in d)
		{
			NamedTypeSymbol item = item2.ParticipatingType;
			if (item.IsInterface)
			{
				flag = true;
			}
			else
			{
				addCandidatesFromType(null, item, sourceExpression, source, target, isChecked, u, ref useSiteInfo);
			}
		}
		if (!((u.Count == 0) & flag))
		{
			return;
		}
		foreach (var (namedTypeSymbol, constrainedToTypeOpt) in d)
		{
			if (namedTypeSymbol.IsInterface)
			{
				addCandidatesFromType(constrainedToTypeOpt, namedTypeSymbol, sourceExpression, source, target, isChecked, u, ref useSiteInfo);
			}
		}
		void addCandidatesFromType(TypeParameterSymbol constrainedToTypeOpt2, NamedTypeSymbol declaringType, BoundExpression sourceExpression2, TypeSymbol source2, TypeSymbol target2, bool isChecked2, ArrayBuilder<UserDefinedConversionAnalysis> u2, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2)
		{
			AddUserDefinedConversionsToExplicitCandidateSet(sourceExpression2, source2, target2, u2, constrainedToTypeOpt2, declaringType, isExplicit: true, isChecked2, ref useSiteInfo2);
			AddUserDefinedConversionsToExplicitCandidateSet(sourceExpression2, source2, target2, u2, constrainedToTypeOpt2, declaringType, isExplicit: false, isChecked2, ref useSiteInfo2);
		}
	}

	private void AddUserDefinedConversionsToExplicitCandidateSet(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ArrayBuilder<UserDefinedConversionAnalysis> u, TypeParameterSymbol constrainedToTypeOpt, NamedTypeSymbol declaringType, bool isExplicit, bool isChecked, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (((object)source != null && source.IsInterfaceType()) || target.IsInterfaceType() || IgnoreUserDefinedSpanConversions(source, target))
		{
			return;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		declaringType.AddOperators((!isExplicit) ? "op_Implicit" : (isChecked ? "op_CheckedExplicit" : "op_Explicit"), instance);
		if (isExplicit & isChecked)
		{
			if (instance.IsEmpty)
			{
				declaringType.AddOperators("op_Explicit", instance);
			}
			else
			{
				int count = instance.Count;
				ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
				declaringType.AddOperators("op_Explicit", instance2);
				foreach (MethodSymbol item in instance2)
				{
					bool flag = true;
					for (int i = 0; i < count; i++)
					{
						if (SourceMemberContainerTypeSymbol.DoOperatorsPair(instance[i], item))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						instance.Add(item);
					}
				}
				instance2.Free();
			}
		}
		foreach (MethodSymbol item2 in instance)
		{
			if (item2.ReturnsVoid || item2.ParameterCount != 1 || item2.ReturnType.TypeKind == TypeKind.Error)
			{
				continue;
			}
			TypeSymbol typeSymbol = item2.GetParameterType(0);
			TypeSymbol typeSymbol2 = item2.ReturnType;
			Conversion sourceConversion = EncompassingExplicitConversion(sourceExpression, source, typeSymbol, ref useSiteInfo);
			Conversion targetConversion = EncompassingExplicitConversion(typeSymbol2, target, ref useSiteInfo);
			if (!sourceConversion.Exists && (object)source != null && source.IsNullableType() && EncompassingExplicitConversion(source.GetNullableUnderlyingType(), typeSymbol, ref useSiteInfo).Exists)
			{
				sourceConversion = ClassifyBuiltInConversion(source, typeSymbol, isChecked, ref useSiteInfo);
			}
			if (!targetConversion.Exists && (object)target != null && target.IsNullableType() && EncompassingExplicitConversion(typeSymbol2, target.GetNullableUnderlyingType(), ref useSiteInfo).Exists)
			{
				targetConversion = ClassifyBuiltInConversion(typeSymbol2, target, isChecked, ref useSiteInfo);
			}
			if (!sourceConversion.Exists || !targetConversion.Exists)
			{
				continue;
			}
			if ((object)source != null && source.IsNullableType() && typeSymbol.IsValidNullableTypeArgument() && target.CanBeAssignedNull())
			{
				TypeSymbol typeSymbol3 = MakeNullableType(typeSymbol);
				TypeSymbol typeSymbol4 = (typeSymbol2.IsValidNullableTypeArgument() ? MakeNullableType(typeSymbol2) : typeSymbol2);
				Conversion sourceConversion2 = EncompassingExplicitConversion(sourceExpression, source, typeSymbol3, ref useSiteInfo);
				Conversion targetConversion2 = EncompassingExplicitConversion(typeSymbol4, target, ref useSiteInfo);
				u.Add(UserDefinedConversionAnalysis.Lifted(constrainedToTypeOpt, item2, sourceConversion2, targetConversion2, typeSymbol3, typeSymbol4));
				continue;
			}
			if (target.IsNullableType() && typeSymbol2.IsValidNullableTypeArgument())
			{
				typeSymbol2 = MakeNullableType(typeSymbol2);
				targetConversion = EncompassingExplicitConversion(typeSymbol2, target, ref useSiteInfo);
			}
			if ((object)source != null && source.IsNullableType() && typeSymbol.IsValidNullableTypeArgument())
			{
				typeSymbol = MakeNullableType(typeSymbol);
				sourceConversion = EncompassingExplicitConversion(typeSymbol, source, ref useSiteInfo);
			}
			u.Add(UserDefinedConversionAnalysis.Normal(constrainedToTypeOpt, item2, sourceConversion, targetConversion, typeSymbol, typeSymbol2));
		}
		instance.Free();
	}

	private TypeSymbol MostSpecificSourceTypeForExplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, BoundExpression sourceExpression, TypeSymbol source, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)source != null)
		{
			if (u.Any((UserDefinedConversionAnalysis conv, TypeSymbol right) => TypeSymbol.Equals(conv.FromType, right, TypeCompareKind.ConsiderEverything), source))
			{
				return source;
			}
			CompoundUseSiteInfo<AssemblySymbol> inLambdaUseSiteInfo = useSiteInfo;
			Func<UserDefinedConversionAnalysis, bool> func = (UserDefinedConversionAnalysis conv) => IsEncompassedBy(sourceExpression, source, conv.FromType, ref inLambdaUseSiteInfo);
			if (u.Any(func))
			{
				TypeSymbol result = MostEncompassedType(u, func, (UserDefinedConversionAnalysis conv) => conv.FromType, ref inLambdaUseSiteInfo);
				useSiteInfo = inLambdaUseSiteInfo;
				return result;
			}
			useSiteInfo = inLambdaUseSiteInfo;
		}
		return MostEncompassingType(u, (UserDefinedConversionAnalysis conv) => conv.FromType, ref useSiteInfo);
	}

	private TypeSymbol MostSpecificTargetTypeForExplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, TypeSymbol target, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (u.Any((UserDefinedConversionAnalysis conv, TypeSymbol right) => TypeSymbol.Equals(conv.ToType, right, TypeCompareKind.ConsiderEverything), target))
		{
			return target;
		}
		CompoundUseSiteInfo<AssemblySymbol> inLambdaUseSiteInfo = useSiteInfo;
		Func<UserDefinedConversionAnalysis, bool> func = (UserDefinedConversionAnalysis conv) => IsEncompassedBy(conv.ToType, target, ref inLambdaUseSiteInfo);
		if (u.Any(func))
		{
			TypeSymbol result = MostEncompassingType(u, func, (UserDefinedConversionAnalysis conv) => conv.ToType, ref inLambdaUseSiteInfo);
			useSiteInfo = inLambdaUseSiteInfo;
			return result;
		}
		useSiteInfo = inLambdaUseSiteInfo;
		return MostEncompassedType(u, (UserDefinedConversionAnalysis conv) => conv.ToType, ref useSiteInfo);
	}

	private Conversion EncompassingExplicitConversion(BoundExpression expr, TypeSymbol a, TypeSymbol b, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion result = ClassifyStandardConversion(expr, a, b, ref useSiteInfo);
		if (!result.IsEnumeration)
		{
			return result;
		}
		return Conversion.NoConversion;
	}

	private Conversion EncompassingExplicitConversion(TypeSymbol a, TypeSymbol b, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return EncompassingExplicitConversion(null, a, b, ref useSiteInfo);
	}

	private UserDefinedConversionResult AnalyzeImplicitUserDefinedConversions(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<(NamedTypeSymbol, TypeParameterSymbol)> instance = ArrayBuilder<(NamedTypeSymbol, TypeParameterSymbol)>.GetInstance();
		ComputeUserDefinedImplicitConversionTypeSet(source, target, instance, ref useSiteInfo);
		ArrayBuilder<UserDefinedConversionAnalysis> instance2 = ArrayBuilder<UserDefinedConversionAnalysis>.GetInstance();
		ComputeApplicableUserDefinedImplicitConversionSet(sourceExpression, source, target, instance, instance2, ref useSiteInfo);
		instance.Free();
		ImmutableArray<UserDefinedConversionAnalysis> immutableArray = instance2.ToImmutableAndFree();
		if (immutableArray.Length == 0)
		{
			return UserDefinedConversionResult.NoApplicableOperators(immutableArray);
		}
		TypeSymbol typeSymbol = MostSpecificSourceTypeForImplicitUserDefinedConversion(immutableArray, source, ref useSiteInfo);
		if ((object)typeSymbol == null)
		{
			return UserDefinedConversionResult.NoBestSourceType(immutableArray);
		}
		TypeSymbol typeSymbol2 = MostSpecificTargetTypeForImplicitUserDefinedConversion(immutableArray, target, ref useSiteInfo);
		if ((object)typeSymbol2 == null)
		{
			return UserDefinedConversionResult.NoBestTargetType(immutableArray);
		}
		int? num = MostSpecificConversionOperator(typeSymbol, typeSymbol2, immutableArray);
		if (!num.HasValue)
		{
			return UserDefinedConversionResult.Ambiguous(immutableArray);
		}
		return UserDefinedConversionResult.Valid(immutableArray, num.Value);
	}

	private static void ComputeUserDefinedImplicitConversionTypeSet(TypeSymbol s, TypeSymbol t, ArrayBuilder<(NamedTypeSymbol ParticipatingType, TypeParameterSymbol ConstrainedToTypeOpt)> d, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		AddTypesParticipatingInUserDefinedConversion(d, s, includeBaseTypes: true, ref useSiteInfo);
		AddTypesParticipatingInUserDefinedConversion(d, t, includeBaseTypes: false, ref useSiteInfo);
	}

	private void ComputeApplicableUserDefinedImplicitConversionSet(BoundExpression sourceExpression, TypeSymbol source, TypeSymbol target, ArrayBuilder<(NamedTypeSymbol ParticipatingType, TypeParameterSymbol ConstrainedToTypeOpt)> d, ArrayBuilder<UserDefinedConversionAnalysis> u, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo, bool allowAnyTarget = false)
	{
		if (((object)source != null && source.IsInterfaceType()) || ((object)target != null && target.IsInterfaceType()) || IgnoreUserDefinedSpanConversions(source, target))
		{
			return;
		}
		bool flag = false;
		foreach (var item2 in d)
		{
			NamedTypeSymbol item = item2.ParticipatingType;
			if (item.IsInterface)
			{
				flag = true;
			}
			else
			{
				addCandidatesFromType(null, item, sourceExpression, source, target, u, ref useSiteInfo, allowAnyTarget);
			}
		}
		if (!((u.Count == 0) & flag))
		{
			return;
		}
		foreach (var (namedTypeSymbol, constrainedToTypeOpt) in d)
		{
			if (namedTypeSymbol.IsInterface)
			{
				addCandidatesFromType(constrainedToTypeOpt, namedTypeSymbol, sourceExpression, source, target, u, ref useSiteInfo, allowAnyTarget);
			}
		}
		void addCandidatesFromType(TypeParameterSymbol constrainedToTypeOpt2, NamedTypeSymbol declaringType, BoundExpression aExpr, TypeSymbol typeSymbol2, TypeSymbol typeSymbol3, ArrayBuilder<UserDefinedConversionAnalysis> arrayBuilder, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo2, bool flag2)
		{
			ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
			declaringType.AddOperators("op_Implicit", instance);
			foreach (MethodSymbol item3 in instance)
			{
				if (!item3.ReturnsVoid && item3.ParameterCount == 1)
				{
					TypeSymbol parameterType = item3.GetParameterType(0);
					TypeSymbol typeSymbol = item3.ReturnType;
					Conversion sourceConversion = EncompassingImplicitConversion(aExpr, typeSymbol2, parameterType, ref useSiteInfo2);
					Conversion targetConversion = (flag2 ? Conversion.Identity : EncompassingImplicitConversion(typeSymbol, typeSymbol3, ref useSiteInfo2));
					if (sourceConversion.Exists && targetConversion.Exists)
					{
						if ((object)typeSymbol3 != null && typeSymbol3.IsNullableType() && typeSymbol.IsValidNullableTypeArgument())
						{
							typeSymbol = MakeNullableType(typeSymbol);
							targetConversion = (flag2 ? Conversion.Identity : EncompassingImplicitConversion(typeSymbol, typeSymbol3, ref useSiteInfo2));
						}
						arrayBuilder.Add(UserDefinedConversionAnalysis.Normal(constrainedToTypeOpt2, item3, sourceConversion, targetConversion, parameterType, typeSymbol));
					}
					else if ((object)typeSymbol2 != null && typeSymbol2.IsNullableType() && parameterType.IsValidNullableTypeArgument() && (flag2 || typeSymbol3.CanBeAssignedNull()))
					{
						TypeSymbol typeSymbol4 = MakeNullableType(parameterType);
						TypeSymbol typeSymbol5 = (typeSymbol.IsValidNullableTypeArgument() ? MakeNullableType(typeSymbol) : typeSymbol);
						Conversion sourceConversion2 = EncompassingImplicitConversion(aExpr, typeSymbol2, typeSymbol4, ref useSiteInfo2);
						Conversion targetConversion2 = ((!flag2) ? EncompassingImplicitConversion(typeSymbol5, typeSymbol3, ref useSiteInfo2) : Conversion.Identity);
						if (sourceConversion2.Exists && targetConversion2.Exists)
						{
							arrayBuilder.Add(UserDefinedConversionAnalysis.Lifted(constrainedToTypeOpt2, item3, sourceConversion2, targetConversion2, typeSymbol4, typeSymbol5));
						}
					}
				}
			}
			instance.Free();
		}
	}

	private TypeSymbol MostSpecificSourceTypeForImplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, TypeSymbol source, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if ((object)source != null && u.Any((UserDefinedConversionAnalysis conv, TypeSymbol right) => TypeSymbol.Equals(conv.FromType, right, TypeCompareKind.ConsiderEverything), source))
		{
			return source;
		}
		return MostEncompassedType(u, (UserDefinedConversionAnalysis conv) => conv.FromType, ref useSiteInfo);
	}

	private TypeSymbol MostSpecificTargetTypeForImplicitUserDefinedConversion(ImmutableArray<UserDefinedConversionAnalysis> u, TypeSymbol target, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		if (u.Any((UserDefinedConversionAnalysis conv, TypeSymbol right) => TypeSymbol.Equals(conv.ToType, right, TypeCompareKind.ConsiderEverything), target))
		{
			return target;
		}
		return MostEncompassingType(u, (UserDefinedConversionAnalysis conv) => conv.ToType, ref useSiteInfo);
	}

	private static int LiftingCount(UserDefinedConversionAnalysis conv)
	{
		int num = 0;
		if (!TypeSymbol.Equals(conv.FromType, conv.Operator.GetParameterType(0), TypeCompareKind.ConsiderEverything))
		{
			num++;
		}
		if (!TypeSymbol.Equals(conv.ToType, conv.Operator.ReturnType, TypeCompareKind.ConsiderEverything))
		{
			num++;
		}
		return num;
	}

	private static int? MostSpecificConversionOperator(TypeSymbol sx, TypeSymbol tx, ImmutableArray<UserDefinedConversionAnalysis> u)
	{
		return MostSpecificConversionOperator((UserDefinedConversionAnalysis conv) => TypeSymbol.Equals(conv.FromType, sx, TypeCompareKind.ConsiderEverything) && TypeSymbol.Equals(conv.ToType, tx, TypeCompareKind.ConsiderEverything), u);
	}

	private static int? MostSpecificConversionOperator(Func<UserDefinedConversionAnalysis, bool> constraint, ImmutableArray<UserDefinedConversionAnalysis> u)
	{
		BestIndex bestIndex = UniqueIndex(u, (UserDefinedConversionAnalysis conv) => constraint(conv) && LiftingCount(conv) == 0);
		if (bestIndex.Kind == BestIndexKind.Best)
		{
			return bestIndex.Best;
		}
		if (bestIndex.Kind == BestIndexKind.Ambiguous)
		{
			return null;
		}
		BestIndex bestIndex2 = UniqueIndex(u, (UserDefinedConversionAnalysis conv) => constraint(conv) && LiftingCount(conv) == 1);
		if (bestIndex2.Kind == BestIndexKind.Best)
		{
			return bestIndex2.Best;
		}
		if (bestIndex2.Kind == BestIndexKind.Ambiguous)
		{
			return null;
		}
		BestIndex bestIndex3 = UniqueIndex(u, (UserDefinedConversionAnalysis conv) => constraint(conv) && LiftingCount(conv) == 2);
		if (bestIndex3.Kind == BestIndexKind.Best)
		{
			return bestIndex3.Best;
		}
		_ = bestIndex3.Kind;
		_ = 2;
		return null;
	}

	private static BestIndex UniqueIndex<T>(ImmutableArray<T> items, Func<T, bool> predicate)
	{
		if (items.IsEmpty)
		{
			return BestIndex.None();
		}
		int? num = null;
		for (int i = 0; i < items.Length; i++)
		{
			if (predicate(items[i]))
			{
				if (num.HasValue)
				{
					return BestIndex.IsAmbiguous(num.Value, i);
				}
				num = i;
			}
		}
		if (num.HasValue)
		{
			return BestIndex.HasBest(num.Value);
		}
		return BestIndex.None();
	}

	private bool IsEncompassedBy(BoundExpression aExpr, TypeSymbol a, TypeSymbol b, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return EncompassingImplicitConversion(aExpr, a, b, ref useSiteInfo).Exists;
	}

	private bool IsEncompassedBy(TypeSymbol a, TypeSymbol b, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return IsEncompassedBy(null, a, b, ref useSiteInfo);
	}

	private Conversion EncompassingImplicitConversion(BoundExpression aExpr, TypeSymbol a, TypeSymbol b, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		Conversion result = ClassifyStandardImplicitConversion(aExpr, a, b, ref useSiteInfo);
		if (!IsEncompassingImplicitConversionKind(result.Kind))
		{
			return Conversion.NoConversion;
		}
		return result;
	}

	private Conversion EncompassingImplicitConversion(TypeSymbol a, TypeSymbol b, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return EncompassingImplicitConversion(null, a, b, ref useSiteInfo);
	}

	private static bool IsEncompassingImplicitConversionKind(ConversionKind kind)
	{
		switch (kind)
		{
		case ConversionKind.NoConversion:
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ExplicitTupleLiteral:
		case ConversionKind.ExplicitTuple:
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ExplicitDynamic:
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.AnonymousFunction:
		case ConversionKind.MethodGroup:
		case ConversionKind.FunctionType:
		case ConversionKind.ExplicitNumeric:
		case ConversionKind.ExplicitEnumeration:
		case ConversionKind.ExplicitNullable:
		case ConversionKind.ExplicitReference:
		case ConversionKind.Unboxing:
		case ConversionKind.ExplicitUserDefined:
		case ConversionKind.ExplicitPointerToPointer:
		case ConversionKind.ExplicitIntegerToPointer:
		case ConversionKind.ExplicitPointerToInteger:
		case ConversionKind.IntPtr:
		case ConversionKind.InterpolatedString:
		case ConversionKind.SwitchExpression:
		case ConversionKind.ConditionalExpression:
		case ConversionKind.StackAllocToPointerType:
		case ConversionKind.StackAllocToSpanType:
		case ConversionKind.InterpolatedStringHandler:
		case ConversionKind.ExplicitSpan:
			return false;
		case ConversionKind.Identity:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitThrow:
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ImplicitNullable:
		case ConversionKind.NullLiteral:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
		case ConversionKind.ImplicitPointerToVoid:
		case ConversionKind.ImplicitNullToPointer:
		case ConversionKind.ImplicitPointer:
		case ConversionKind.ImplicitConstant:
		case ConversionKind.DefaultLiteral:
		case ConversionKind.InlineArray:
		case ConversionKind.ImplicitSpan:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private TypeSymbol MostEncompassedType<T>(ImmutableArray<T> items, Func<T, TypeSymbol> extract, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return MostEncompassedType(items, (T x) => true, extract, ref useSiteInfo);
	}

	private TypeSymbol MostEncompassedType<T>(ImmutableArray<T> items, Func<T, bool> valid, Func<T, TypeSymbol> extract, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		CompoundUseSiteInfo<AssemblySymbol> inLambdaUseSiteInfo = useSiteInfo;
		int? num = UniqueBestValidIndex(items, valid, delegate(T left, T right)
		{
			TypeSymbol typeSymbol = extract(left);
			TypeSymbol typeSymbol2 = extract(right);
			if (TypeSymbol.Equals(typeSymbol, typeSymbol2, TypeCompareKind.ConsiderEverything))
			{
				return BetterResult.Equal;
			}
			bool flag = IsEncompassedBy(typeSymbol, typeSymbol2, ref inLambdaUseSiteInfo);
			bool flag2 = IsEncompassedBy(typeSymbol2, typeSymbol, ref inLambdaUseSiteInfo);
			if (flag == flag2)
			{
				return BetterResult.Neither;
			}
			return (!flag) ? BetterResult.Right : BetterResult.Left;
		});
		useSiteInfo = inLambdaUseSiteInfo;
		if (num.HasValue)
		{
			return extract(items[num.Value]);
		}
		return null;
	}

	private TypeSymbol MostEncompassingType<T>(ImmutableArray<T> items, Func<T, TypeSymbol> extract, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		return MostEncompassingType(items, (T x) => true, extract, ref useSiteInfo);
	}

	private TypeSymbol MostEncompassingType<T>(ImmutableArray<T> items, Func<T, bool> valid, Func<T, TypeSymbol> extract, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		CompoundUseSiteInfo<AssemblySymbol> inLambdaUseSiteInfo = useSiteInfo;
		int? num = UniqueBestValidIndex(items, valid, delegate(T left, T right)
		{
			TypeSymbol typeSymbol = extract(left);
			TypeSymbol typeSymbol2 = extract(right);
			if (TypeSymbol.Equals(typeSymbol, typeSymbol2, TypeCompareKind.ConsiderEverything))
			{
				return BetterResult.Equal;
			}
			bool flag = IsEncompassedBy(typeSymbol2, typeSymbol, ref inLambdaUseSiteInfo);
			bool flag2 = IsEncompassedBy(typeSymbol, typeSymbol2, ref inLambdaUseSiteInfo);
			if (flag == flag2)
			{
				return BetterResult.Neither;
			}
			return (!flag) ? BetterResult.Right : BetterResult.Left;
		});
		useSiteInfo = inLambdaUseSiteInfo;
		if (num.HasValue)
		{
			return extract(items[num.Value]);
		}
		return null;
	}

	private static int? UniqueBestValidIndex<T>(ImmutableArray<T> items, Func<T, bool> valid, Func<T, T, BetterResult> better)
	{
		if (items.IsEmpty)
		{
			return null;
		}
		int? result = null;
		T arg = default(T);
		for (int i = 0; i < items.Length; i++)
		{
			T val = items[i];
			if (!valid(val))
			{
				continue;
			}
			if (!result.HasValue)
			{
				result = i;
				arg = val;
				continue;
			}
			switch (better(arg, val))
			{
			case BetterResult.Neither:
				result = null;
				arg = default(T);
				break;
			case BetterResult.Right:
				result = i;
				arg = val;
				break;
			}
		}
		if (!result.HasValue)
		{
			return null;
		}
		for (int j = 0; j < result.Value; j++)
		{
			T val2 = items[j];
			if (valid(val2))
			{
				BetterResult betterResult = better(arg, val2);
				if (betterResult != BetterResult.Left && betterResult != BetterResult.Equal)
				{
					return null;
				}
			}
		}
		return result;
	}

	private NamedTypeSymbol MakeNullableType(TypeSymbol type)
	{
		return corLibrary.GetDeclaredSpecialType(SpecialType.System_Nullable_T).Construct(type);
	}

	protected UserDefinedConversionResult AnalyzeImplicitUserDefinedConversionForV6SwitchGoverningType(TypeSymbol source, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		ArrayBuilder<(NamedTypeSymbol, TypeParameterSymbol)> instance = ArrayBuilder<(NamedTypeSymbol, TypeParameterSymbol)>.GetInstance();
		ComputeUserDefinedImplicitConversionTypeSet(source, null, instance, ref useSiteInfo);
		ArrayBuilder<UserDefinedConversionAnalysis> instance2 = ArrayBuilder<UserDefinedConversionAnalysis>.GetInstance();
		ComputeApplicableUserDefinedImplicitConversionSet(null, source, null, instance, instance2, ref useSiteInfo, allowAnyTarget: true);
		instance.Free();
		ImmutableArray<UserDefinedConversionAnalysis> immutableArray = instance2.ToImmutableAndFree();
		int? num = MostSpecificConversionOperator((UserDefinedConversionAnalysis conv) => conv.ToType.IsValidV6SwitchGoverningType(isTargetTypeOfUserDefinedOp: true), immutableArray);
		if (num.HasValue)
		{
			return UserDefinedConversionResult.Valid(immutableArray, num.Value);
		}
		return UserDefinedConversionResult.NoApplicableOperators(immutableArray);
	}
}
