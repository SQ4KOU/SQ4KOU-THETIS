using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConversion : BoundExpression
{
	public ConversionKind ConversionKind => Conversion.Kind;

	public bool IsExtensionMethod => Conversion.IsExtensionMethod;

	public MethodSymbol? SymbolOpt => Conversion.Method;

	public override Symbol? ExpressionSymbol => SymbolOpt;

	public override bool SuppressVirtualCalls => IsBaseConversion;

	public new bool IsParamsArrayOrCollection
	{
		get
		{
			return base.IsParamsArrayOrCollection;
		}
		init
		{
			base.IsParamsArrayOrCollection = value;
		}
	}

	public new TypeSymbol Type => base.Type;

	public BoundExpression Operand { get; }

	public Conversion Conversion { get; }

	public bool IsBaseConversion { get; }

	public bool Checked { get; }

	public bool ExplicitCastInCode { get; }

	public override ConstantValue? ConstantValueOpt { get; }

	public ConversionGroup? ConversionGroupOpt { get; }

	public BoundConversion UpdateOperand(BoundExpression operand)
	{
		return Update(operand, Conversion, IsBaseConversion, Checked, ExplicitCastInCode, ConstantValueOpt, ConversionGroupOpt, Type);
	}

	internal bool ConversionHasSideEffects()
	{
		switch (ConversionKind)
		{
		case ConversionKind.Identity:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
			return false;
		case ConversionKind.ExplicitNumeric:
			return Checked;
		default:
			return true;
		}
	}

	public static BoundConversion SynthesizedNonUserDefined(SyntaxNode syntax, BoundExpression operand, Conversion conversion, TypeSymbol type, ConstantValue? constantValueOpt = null)
	{
		return new BoundConversion(syntax, operand, conversion, isBaseConversion: false, @checked: false, explicitCastInCode: false, constantValueOpt, null, type)
		{
			WasCompilerGenerated = true
		};
	}

	public static BoundConversion Synthesized(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, bool explicitCastInCode, ConversionGroup? conversionGroupOpt, ConstantValue? constantValueOpt, TypeSymbol type, bool hasErrors = false)
	{
		return new BoundConversion(syntax, operand, conversion, @checked, explicitCastInCode, conversionGroupOpt, constantValueOpt, type, hasErrors || !conversion.IsValid)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, bool explicitCastInCode, ConversionGroup? conversionGroupOpt, ConstantValue? constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operand, conversion, isBaseConversion: false, @checked, explicitCastInCode, constantValueOpt, conversionGroupOpt, type, hasErrors || !conversion.IsValid)
	{
	}

	public BoundConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool isBaseConversion, bool @checked, bool explicitCastInCode, ConstantValue? constantValueOpt, ConversionGroup? conversionGroupOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Conversion, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
		Conversion = conversion;
		IsBaseConversion = isBaseConversion;
		Checked = @checked;
		ExplicitCastInCode = explicitCastInCode;
		ConstantValueOpt = constantValueOpt;
		ConversionGroupOpt = conversionGroupOpt;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConversion(this);
	}

	public BoundConversion Update(BoundExpression operand, Conversion conversion, bool isBaseConversion, bool @checked, bool explicitCastInCode, ConstantValue? constantValueOpt, ConversionGroup? conversionGroupOpt, TypeSymbol type)
	{
		if (operand != Operand || conversion != Conversion || isBaseConversion != IsBaseConversion || @checked != Checked || explicitCastInCode != ExplicitCastInCode || constantValueOpt != ConstantValueOpt || conversionGroupOpt != ConversionGroupOpt || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundConversion boundConversion = new BoundConversion(Syntax, operand, conversion, isBaseConversion, @checked, explicitCastInCode, constantValueOpt, conversionGroupOpt, type, base.HasErrors);
			boundConversion.CopyAttributes(this);
			return boundConversion;
		}
		return this;
	}
}
