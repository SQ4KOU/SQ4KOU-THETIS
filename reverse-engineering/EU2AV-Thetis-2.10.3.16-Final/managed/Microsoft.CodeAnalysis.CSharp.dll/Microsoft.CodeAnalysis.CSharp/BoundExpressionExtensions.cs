using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BoundExpressionExtensions
{
	public static RefKind GetRefKind(this BoundExpression node)
	{
		switch (node.Kind)
		{
		case BoundKind.Local:
			return ((BoundLocal)node).LocalSymbol.RefKind;
		case BoundKind.Parameter:
			return ((BoundParameter)node).ParameterSymbol.RefKind;
		case BoundKind.FieldAccess:
			return ((BoundFieldAccess)node).FieldSymbol.RefKind;
		case BoundKind.Call:
			return ((BoundCall)node).Method.RefKind;
		case BoundKind.PropertyAccess:
			return ((BoundPropertyAccess)node).PropertySymbol.RefKind;
		case BoundKind.IndexerAccess:
			return ((BoundIndexerAccess)node).Indexer.RefKind;
		case BoundKind.ImplicitIndexerAccess:
			return ((BoundImplicitIndexerAccess)node).IndexerOrSliceAccess.GetRefKind();
		case BoundKind.InlineArrayAccess:
		{
			BoundInlineArrayAccess boundInlineArrayAccess = (BoundInlineArrayAccess)node;
			if (!boundInlineArrayAccess.IsValue)
			{
				switch (boundInlineArrayAccess.GetItemOrSliceHelper)
				{
				case WellKnownMember.System_Span_T__get_Item:
					return RefKind.Ref;
				case WellKnownMember.System_ReadOnlySpan_T__get_Item:
					return RefKind.In;
				}
			}
			return RefKind.None;
		}
		case BoundKind.ObjectInitializerMember:
		{
			BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)node;
			if (boundObjectInitializerMember.HasErrors)
			{
				return RefKind.None;
			}
			Symbol memberSymbol = boundObjectInitializerMember.MemberSymbol;
			if (!(memberSymbol is FieldSymbol { RefKind: var refKind }))
			{
				if (!(memberSymbol is PropertySymbol { RefKind: var refKind2 }))
				{
					if (memberSymbol is EventSymbol)
					{
						return RefKind.None;
					}
					throw ExceptionUtilities.UnexpectedValue(memberSymbol?.Kind);
				}
				return refKind2;
			}
			return refKind;
		}
		default:
			return RefKind.None;
		}
	}

	public static bool IsLiteralNull(this BoundExpression node)
	{
		if (node != null && node.Kind == BoundKind.Literal)
		{
			ConstantValue constantValueOpt = node.ConstantValueOpt;
			if ((object)constantValueOpt != null)
			{
				return constantValueOpt.Discriminator == ConstantValueTypeDiscriminator.Nothing;
			}
		}
		return false;
	}

	public static bool IsLiteralDefault(this BoundExpression node)
	{
		return node.Kind == BoundKind.DefaultLiteral;
	}

	public static bool IsImplicitObjectCreation(this BoundExpression node)
	{
		return node.Kind == BoundKind.UnconvertedObjectCreationExpression;
	}

	public static bool IsLiteralDefaultOrImplicitObjectCreation(this BoundExpression node)
	{
		if (!node.IsLiteralDefault())
		{
			return node.IsImplicitObjectCreation();
		}
		return true;
	}

	public static bool IsDefaultValue(this BoundExpression node)
	{
		if (node.Kind == BoundKind.DefaultExpression || node.Kind == BoundKind.DefaultLiteral)
		{
			return true;
		}
		ConstantValue constantValueOpt = node.ConstantValueOpt;
		if (constantValueOpt != null)
		{
			return constantValueOpt.IsDefaultValue;
		}
		return false;
	}

	public static bool HasExpressionType(this BoundExpression node)
	{
		return (object)node.Type != null;
	}

	public static bool HasDynamicType(this BoundExpression node)
	{
		return node.Type?.IsDynamic() ?? false;
	}

	public static NamedTypeSymbol? GetInferredDelegateType(this BoundExpression expr, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		NamedTypeSymbol namedTypeSymbol = expr.GetFunctionType()?.GetInternalDelegateType();
		namedTypeSymbol?.AddUseSiteInfo(ref useSiteInfo);
		if (!(expr is BoundMethodGroup boundMethodGroup))
		{
			goto IL_0054;
		}
		ImmutableArray<MethodSymbol> methods = boundMethodGroup.Methods;
		if (methods.Length == 1)
		{
			MethodSymbol methodSymbol = methods[0];
			if ((object)methodSymbol != null && methodSymbol.MethodKind == MethodKind.LocalFunction)
			{
				goto IL_0054;
			}
		}
		bool flag = true;
		goto IL_0057;
		IL_0057:
		if (flag && (object)namedTypeSymbol != null)
		{
			MethodSymbol delegateInvokeMethod = namedTypeSymbol.DelegateInvokeMethod;
			if ((object)delegateInvokeMethod != null && delegateInvokeMethod.OriginalDefinition is SynthesizedDelegateInvokeMethod { RefKind: RefKind.In })
			{
				namedTypeSymbol.DeclaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_InAttribute).AddUseSiteInfo(ref useSiteInfo);
			}
		}
		return namedTypeSymbol;
		IL_0054:
		flag = false;
		goto IL_0057;
	}

	public static TypeSymbol? GetTypeOrFunctionType(this BoundExpression expr)
	{
		TypeSymbol type = expr.Type;
		if ((object)type != null)
		{
			return type;
		}
		return expr.GetFunctionType();
	}

	public static FunctionTypeSymbol? GetFunctionType(this BoundExpression expr)
	{
		if (!(expr is BoundMethodGroup boundMethodGroup))
		{
			if (expr is UnboundLambda unboundLambda)
			{
				return unboundLambda.FunctionType;
			}
			return null;
		}
		return boundMethodGroup.FunctionType;
	}

	public static bool MethodGroupReceiverIsDynamic(this BoundMethodGroup node)
	{
		if (node.InstanceOpt != null)
		{
			return node.InstanceOpt.HasDynamicType();
		}
		return false;
	}

	public static void GetExpressionSymbols(this BoundExpression node, ArrayBuilder<Symbol> symbols, BoundNode parent, Binder binder)
	{
		switch (node.Kind)
		{
		case BoundKind.MethodGroup:
			if (parent is BoundDelegateCreationExpression { MethodOpt: not null } boundDelegateCreationExpression)
			{
				symbols.Add(boundDelegateCreationExpression.MethodOpt);
			}
			else
			{
				symbols.AddRange(CSharpSemanticModel.GetReducedAndFilteredMethodGroupSymbols(binder, (BoundMethodGroup)node));
			}
			return;
		case BoundKind.BadExpression:
			foreach (Symbol symbol2 in ((BoundBadExpression)node).Symbols)
			{
				if ((object)symbol2 != null)
				{
					symbols.Add(symbol2);
				}
			}
			return;
		case BoundKind.DelegateCreationExpression:
		{
			Symbol symbol = ((BoundDelegateCreationExpression)node).Type.GetMembers(".ctor").FirstOrDefault();
			if ((object)symbol != null)
			{
				symbols.Add(symbol);
			}
			return;
		}
		case BoundKind.Call:
		{
			ImmutableArray<MethodSymbol> originalMethodsOpt = ((BoundCall)node).OriginalMethodsOpt;
			if (!originalMethodsOpt.IsDefault)
			{
				symbols.AddRange(originalMethodsOpt);
				return;
			}
			break;
		}
		case BoundKind.IndexerAccess:
		{
			ImmutableArray<PropertySymbol> originalIndexersOpt = ((BoundIndexerAccess)node).OriginalIndexersOpt;
			if (!originalIndexersOpt.IsDefault)
			{
				symbols.AddRange(originalIndexersOpt);
				return;
			}
			break;
		}
		}
		Symbol expressionSymbol = node.ExpressionSymbol;
		if ((object)expressionSymbol != null)
		{
			symbols.Add(expressionSymbol);
		}
	}

	public static Conversion GetConversion(this BoundExpression boundNode)
	{
		if (boundNode.Kind == BoundKind.Conversion)
		{
			return ((BoundConversion)boundNode).Conversion;
		}
		return Conversion.Identity;
	}

	internal static bool IsExpressionOfComImportType([NotNullWhen(true)] this BoundExpression? expressionOpt)
	{
		if (expressionOpt == null)
		{
			return false;
		}
		if (expressionOpt.Type is NamedTypeSymbol { Kind: SymbolKind.NamedType } namedTypeSymbol)
		{
			return namedTypeSymbol.IsComImport;
		}
		return false;
	}

	internal static bool IsDiscardExpression(this BoundExpression expr)
	{
		if (!(expr is BoundDiscardExpression))
		{
			if (expr is OutDeconstructVarPendingInference outDeconstructVarPendingInference)
			{
				if (outDeconstructVarPendingInference.IsDiscardExpression)
				{
					return true;
				}
			}
			else if (expr is BoundDeconstructValuePlaceholder { IsDiscardExpression: not false })
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool NullableAlwaysHasValue(this BoundExpression expr)
	{
		if ((object)expr.Type == null)
		{
			return false;
		}
		if (expr.Type.IsDynamic())
		{
			return false;
		}
		if (!expr.Type.IsNullableType())
		{
			return true;
		}
		if (expr.Kind == BoundKind.ObjectCreationExpression)
		{
			return ((BoundObjectCreationExpression)expr).Constructor.ParameterCount != 0;
		}
		if (expr.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			switch (boundConversion.ConversionKind)
			{
			case ConversionKind.ImplicitNullable:
			case ConversionKind.ExplicitNullable:
				return boundConversion.Operand.NullableAlwaysHasValue();
			case ConversionKind.ImplicitEnumeration:
				return boundConversion.Operand.NullableAlwaysHasValue();
			}
		}
		return false;
	}

	public static bool NullableNeverHasValue(this BoundExpression expr)
	{
		if ((object)expr.Type == null && expr.ConstantValueOpt == ConstantValue.Null)
		{
			return true;
		}
		if ((object)expr.Type == null || !expr.Type.IsNullableType())
		{
			return false;
		}
		if (expr is BoundDefaultLiteral || expr is BoundDefaultExpression)
		{
			return true;
		}
		if (expr.Kind == BoundKind.ObjectCreationExpression)
		{
			return ((BoundObjectCreationExpression)expr).Constructor.ParameterCount == 0;
		}
		if (expr.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			switch (boundConversion.ConversionKind)
			{
			case ConversionKind.NullLiteral:
				return true;
			case ConversionKind.DefaultLiteral:
				return true;
			case ConversionKind.ImplicitNullable:
			case ConversionKind.ExplicitNullable:
				return boundConversion.Operand.NullableNeverHasValue();
			}
		}
		return false;
	}

	public static bool IsNullableNonBoolean(this BoundExpression expr)
	{
		if (expr.Type.IsNullableType() && expr.Type.GetNullableUnderlyingType().SpecialType != SpecialType.System_Boolean)
		{
			return true;
		}
		return false;
	}
}
