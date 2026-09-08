using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ConstantValueUtils
{
	private sealed class CheckConstantInterpolatedStringValidity : BoundTreeWalkerWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		internal readonly BindingDiagnosticBag diagnostics;

		public CheckConstantInterpolatedStringValidity(BindingDiagnosticBag diagnostics)
		{
			this.diagnostics = diagnostics;
		}

		public override BoundNode VisitInterpolatedString(BoundInterpolatedString node)
		{
			Binder.CheckFeatureAvailability(node.Syntax, MessageID.IDS_FeatureConstantInterpolatedStrings, diagnostics);
			return null;
		}
	}

	public static ConstantValue EvaluateFieldConstant(SourceFieldSymbol symbol, EqualsValueClauseSyntax equalsValueNode, HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, BindingDiagnosticBag diagnostics)
	{
		Binder binder = symbol.DeclaringCompilation.GetBinderFactory(equalsValueNode.SyntaxTree).GetBinder(equalsValueNode);
		binder = new WithPrimaryConstructorParametersBinder(symbol.ContainingType, binder);
		if (earlyDecodingWellKnownAttributes)
		{
			binder = new EarlyWellKnownAttributeBinder(binder);
		}
		return GetAndValidateConstantValue(BindFieldOrEnumInitializer(new ConstantFieldsInProgressBinder(new ConstantFieldsInProgress(symbol, dependencies), binder), symbol, equalsValueNode, diagnostics).Value, symbol, symbol.Type, equalsValueNode.Value, diagnostics);
	}

	private static BoundFieldEqualsValue BindFieldOrEnumInitializer(Binder binder, FieldSymbol fieldSymbol, EqualsValueClauseSyntax initializer, BindingDiagnosticBag diagnostics)
	{
		SourceEnumConstantSymbol sourceEnumConstantSymbol = fieldSymbol as SourceEnumConstantSymbol;
		Binder next = new LocalScopeBinder(binder);
		next = new ExecutableCodeBinder(initializer, fieldSymbol, next);
		if ((object)sourceEnumConstantSymbol != null)
		{
			return next.BindEnumConstantInitializer(sourceEnumConstantSymbol, initializer, diagnostics);
		}
		return next.BindFieldInitializer(fieldSymbol, initializer, diagnostics);
	}

	internal static ConstantValue GetAndValidateConstantValue(BoundExpression boundValue, Symbol thisSymbol, TypeSymbol typeSymbol, SyntaxNode initValueNode, BindingDiagnosticBag diagnostics)
	{
		ConstantValue result = ConstantValue.Bad;
		CheckLangVersionForConstantValue(boundValue, diagnostics);
		if (!boundValue.HasAnyErrors)
		{
			if (typeSymbol.TypeKind == TypeKind.TypeParameter)
			{
				diagnostics.Add(ErrorCode.ERR_InvalidConstantDeclarationType, initValueNode.Location, thisSymbol, typeSymbol);
			}
			else
			{
				bool flag = false;
				BoundExpression boundExpression = boundValue;
				while (boundExpression.Kind == BoundKind.Conversion)
				{
					BoundConversion boundConversion = (BoundConversion)boundExpression;
					flag = flag || boundConversion.ConversionKind.IsDynamic();
					boundExpression = boundConversion.Operand;
				}
				ConstantValue constantValue = boundValue.ConstantValueOpt;
				ConstantValue constantValueOpt = boundExpression.ConstantValueOpt;
				if (constantValueOpt != null && !constantValueOpt.IsNull && typeSymbol.IsReferenceType && typeSymbol.SpecialType != SpecialType.System_String)
				{
					diagnostics.Add(ErrorCode.ERR_NotNullConstRefField, initValueNode.Location, thisSymbol, typeSymbol);
					constantValue = constantValue ?? constantValueOpt;
				}
				if (constantValue != null && !flag)
				{
					result = constantValue;
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_NotConstantExpression, initValueNode.Location, thisSymbol);
				}
			}
		}
		return result;
	}

	internal static void CheckLangVersionForConstantValue(BoundExpression expression, BindingDiagnosticBag diagnostics)
	{
		if ((object)expression.Type != null && expression.Type.IsStringType())
		{
			new CheckConstantInterpolatedStringValidity(diagnostics).Visit(expression);
		}
	}
}
