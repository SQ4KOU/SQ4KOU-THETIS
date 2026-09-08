using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBinaryOperator : BoundBinaryOperatorBase
{
	internal class UncommonData
	{
		public readonly ConstantValue? ConstantValue;

		public readonly MethodSymbol? Method;

		public readonly TypeSymbol? ConstrainedToType;

		public readonly bool IsUnconvertedInterpolatedStringAddition;

		public readonly InterpolatedStringHandlerData? InterpolatedStringHandlerData;

		public readonly ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt;

		public static UncommonData UnconvertedInterpolatedStringAddition(ConstantValue? constantValue)
		{
			return new UncommonData(constantValue, null, null, default(ImmutableArray<MethodSymbol>), isUnconvertedInterpolatedStringAddition: true, null);
		}

		public static UncommonData InterpolatedStringHandlerAddition(InterpolatedStringHandlerData data)
		{
			return new UncommonData(null, null, null, default(ImmutableArray<MethodSymbol>), isUnconvertedInterpolatedStringAddition: false, data);
		}

		public static UncommonData? CreateIfNeeded(ConstantValue? constantValue, MethodSymbol? method, TypeSymbol? constrainedToType, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt)
		{
			if (constantValue != null || (object)method != null || (object)constrainedToType != null || !originalUserDefinedOperatorsOpt.IsDefault)
			{
				return new UncommonData(constantValue, method, constrainedToType, originalUserDefinedOperatorsOpt, isUnconvertedInterpolatedStringAddition: false, null);
			}
			return null;
		}

		private UncommonData(ConstantValue? constantValue, MethodSymbol? method, TypeSymbol? constrainedToType, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, bool isUnconvertedInterpolatedStringAddition, InterpolatedStringHandlerData? interpolatedStringHandlerData)
		{
			ConstantValue = constantValue;
			Method = method;
			ConstrainedToType = constrainedToType;
			OriginalUserDefinedOperatorsOpt = originalUserDefinedOperatorsOpt;
			IsUnconvertedInterpolatedStringAddition = isUnconvertedInterpolatedStringAddition;
			InterpolatedStringHandlerData = interpolatedStringHandlerData;
		}

		public UncommonData WithUpdatedMethod(MethodSymbol? method)
		{
			if ((object)method == Method)
			{
				return this;
			}
			return new UncommonData(ConstantValue, method, ConstrainedToType, OriginalUserDefinedOperatorsOpt, IsUnconvertedInterpolatedStringAddition, InterpolatedStringHandlerData);
		}
	}

	public override ConstantValue? ConstantValueOpt => Data?.ConstantValue;

	public override Symbol? ExpressionSymbol => BinaryOperatorMethod;

	public MethodSymbol? BinaryOperatorMethod
	{
		get
		{
			if (!OperatorKind.IsDynamic())
			{
				return Data?.Method;
			}
			return null;
		}
	}

	public MethodSymbol? LeftTruthOperatorMethod
	{
		get
		{
			if (!OperatorKind.IsDynamic() || !OperatorKind.IsLogical())
			{
				return null;
			}
			return Data?.Method;
		}
	}

	internal TypeSymbol? ConstrainedToType => Data?.ConstrainedToType;

	internal bool IsUnconvertedInterpolatedStringAddition => Data?.IsUnconvertedInterpolatedStringAddition ?? false;

	internal InterpolatedStringHandlerData? InterpolatedStringHandlerData => Data?.InterpolatedStringHandlerData;

	internal ImmutableArray<MethodSymbol> OriginalUserDefinedOperatorsOpt => Data?.OriginalUserDefinedOperatorsOpt ?? default(ImmutableArray<MethodSymbol>);

	public BinaryOperatorKind OperatorKind { get; }

	public UncommonData? Data { get; }

	public override LookupResultKind ResultKind { get; }

	public BoundBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, ConstantValue? constantValueOpt, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, ImmutableArray<MethodSymbol> originalUserDefinedOperatorsOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, UncommonData.CreateIfNeeded(constantValueOpt, methodOpt, constrainedToTypeOpt, originalUserDefinedOperatorsOpt), resultKind, left, right, type, hasErrors)
	{
	}

	public BoundBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, ConstantValue? constantValueOpt, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, BoundExpression left, BoundExpression right, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operatorKind, UncommonData.CreateIfNeeded(constantValueOpt, methodOpt, constrainedToTypeOpt, default(ImmutableArray<MethodSymbol>)), resultKind, left, right, type, hasErrors)
	{
	}

	public BoundBinaryOperator Update(BinaryOperatorKind operatorKind, ConstantValue? constantValueOpt, MethodSymbol? methodOpt, TypeSymbol? constrainedToTypeOpt, LookupResultKind resultKind, BoundExpression left, BoundExpression right, TypeSymbol type)
	{
		UncommonData data = UncommonData.CreateIfNeeded(constantValueOpt, methodOpt, constrainedToTypeOpt, OriginalUserDefinedOperatorsOpt);
		return Update(operatorKind, data, resultKind, left, right, type);
	}

	public BoundBinaryOperator Update(UncommonData uncommonData)
	{
		return Update(OperatorKind, uncommonData, ResultKind, base.Left, base.Right, base.Type);
	}

	public BoundBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, UncommonData? data, LookupResultKind resultKind, BoundExpression left, BoundExpression right, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.BinaryOperator, syntax, left, right, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		OperatorKind = operatorKind;
		Data = data;
		ResultKind = resultKind;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
		UncommonData data = Data;
		if (data != null && (object)data.Method != null)
		{
			OperatorKind.IsDynamic();
		}
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBinaryOperator(this);
	}

	public BoundBinaryOperator Update(BinaryOperatorKind operatorKind, UncommonData? data, LookupResultKind resultKind, BoundExpression left, BoundExpression right, TypeSymbol type)
	{
		if (operatorKind != OperatorKind || data != Data || resultKind != ResultKind || left != base.Left || right != base.Right || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundBinaryOperator boundBinaryOperator = new BoundBinaryOperator(Syntax, operatorKind, data, resultKind, left, right, type, base.HasErrors);
			boundBinaryOperator.CopyAttributes(this);
			return boundBinaryOperator;
		}
		return this;
	}
}
