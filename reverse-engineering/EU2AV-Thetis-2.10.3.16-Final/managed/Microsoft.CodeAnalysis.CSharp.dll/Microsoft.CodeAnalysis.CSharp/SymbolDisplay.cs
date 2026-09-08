using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

public static class SymbolDisplay
{
	public static string ToDisplayString(ISymbol symbol, SymbolDisplayFormat? format = null)
	{
		format = format ?? SymbolDisplayFormat.CSharpErrorMessageFormat;
		return ToDisplayString(symbol, null, -1, format, minimal: false);
	}

	public static string ToDisplayString(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableFlowState nullableFlowState, SymbolDisplayFormat? format = null)
	{
		return ToDisplayString(symbol, nullableFlowState.ToAnnotation(), format);
	}

	public static string ToDisplayString(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation, SymbolDisplayFormat? format = null)
	{
		format = format ?? SymbolDisplayFormat.CSharpErrorMessageFormat;
		symbol = symbol.WithNullableAnnotation(nullableAnnotation);
		return ToDisplayString(symbol, null, -1, format, minimal: false);
	}

	private static string ToDisplayString(ISymbol symbol, SemanticModel? semanticModelOpt, int positionOpt, SymbolDisplayFormat format, bool minimal)
	{
		ArrayBuilder<SymbolDisplayPart> instance = ArrayBuilder<SymbolDisplayPart>.GetInstance();
		PopulateDisplayParts(instance, symbol, semanticModelOpt, positionOpt, format, minimal);
		string result = instance.ToDisplayString();
		instance.Free();
		return result;
	}

	public static string ToMinimalDisplayString(ISymbol symbol, SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
	{
		if (format == null)
		{
			format = SymbolDisplayFormat.MinimallyQualifiedFormat;
		}
		return ToDisplayString(symbol, semanticModel, position, format, minimal: true);
	}

	public static string ToMinimalDisplayString(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableFlowState nullableFlowState, SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
	{
		return ToMinimalDisplayString(symbol, nullableFlowState.ToAnnotation(), semanticModel, position, format);
	}

	public static string ToMinimalDisplayString(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation, SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
	{
		if (format == null)
		{
			format = SymbolDisplayFormat.MinimallyQualifiedFormat;
		}
		symbol = symbol.WithNullableAnnotation(nullableAnnotation);
		return ToDisplayString(symbol, semanticModel, position, format, minimal: true);
	}

	public static ImmutableArray<SymbolDisplayPart> ToDisplayParts(ISymbol symbol, SymbolDisplayFormat? format = null)
	{
		format = format ?? SymbolDisplayFormat.CSharpErrorMessageFormat;
		return ToDisplayParts(symbol, null, -1, format, minimal: false);
	}

	public static ImmutableArray<SymbolDisplayPart> ToDisplayParts(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableFlowState nullableFlowState, SymbolDisplayFormat? format = null)
	{
		format = format ?? SymbolDisplayFormat.CSharpErrorMessageFormat;
		return ToDisplayParts(symbol, nullableFlowState, null, -1, format, minimal: false);
	}

	public static ImmutableArray<SymbolDisplayPart> ToDisplayParts(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation, SymbolDisplayFormat? format = null)
	{
		if (format == null)
		{
			format = SymbolDisplayFormat.CSharpErrorMessageFormat;
		}
		return ToDisplayParts(symbol.WithNullableAnnotation(nullableAnnotation), null, -1, format, minimal: false);
	}

	public static ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(ISymbol symbol, SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
	{
		if (format == null)
		{
			format = SymbolDisplayFormat.MinimallyQualifiedFormat;
		}
		return ToDisplayParts(symbol, semanticModel, position, format, minimal: true);
	}

	public static ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableFlowState nullableFlowState, SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
	{
		if (format == null)
		{
			format = SymbolDisplayFormat.MinimallyQualifiedFormat;
		}
		return ToDisplayParts(symbol, nullableFlowState, semanticModel, position, format, minimal: true);
	}

	public static ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableAnnotation nullableAnnotation, SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
	{
		if (format == null)
		{
			format = SymbolDisplayFormat.MinimallyQualifiedFormat;
		}
		return ToDisplayParts(symbol.WithNullableAnnotation(nullableAnnotation), semanticModel, position, format, minimal: true);
	}

	private static ImmutableArray<SymbolDisplayPart> ToDisplayParts(ITypeSymbol symbol, Microsoft.CodeAnalysis.NullableFlowState nullableFlowState, SemanticModel? semanticModelOpt, int positionOpt, SymbolDisplayFormat format, bool minimal)
	{
		return ToDisplayParts(symbol.WithNullableAnnotation(nullableFlowState.ToAnnotation()), semanticModelOpt, positionOpt, format, minimal);
	}

	private static ImmutableArray<SymbolDisplayPart> ToDisplayParts(ISymbol symbol, SemanticModel? semanticModelOpt, int positionOpt, SymbolDisplayFormat format, bool minimal)
	{
		ArrayBuilder<SymbolDisplayPart> instance = ArrayBuilder<SymbolDisplayPart>.GetInstance();
		PopulateDisplayParts(instance, symbol, semanticModelOpt, positionOpt, format, minimal);
		return instance.ToImmutableAndFree();
	}

	private static ArrayBuilder<SymbolDisplayPart> PopulateDisplayParts(ArrayBuilder<SymbolDisplayPart> builder, ISymbol symbol, SemanticModel? semanticModelOpt, int positionOpt, SymbolDisplayFormat format, bool minimal)
	{
		if (symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		if (minimal)
		{
			if (semanticModelOpt == null)
			{
				throw new ArgumentException(CSharpResources.SyntaxTreeSemanticModelMust);
			}
			if (positionOpt < 0 || positionOpt > semanticModelOpt.SyntaxTree.Length)
			{
				throw new ArgumentOutOfRangeException(CSharpResources.PositionNotWithinTree);
			}
		}
		if ((symbol as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.MethodSymbol)?.UnderlyingMethodSymbol is SynthesizedSimpleProgramEntryPointSymbol)
		{
			builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.MethodName, symbol, "<top-level-statements-entry-point>"));
		}
		else
		{
			SymbolDisplayVisitor instance = SymbolDisplayVisitor.GetInstance(builder, format, semanticModelOpt, positionOpt);
			symbol.Accept(instance);
			if (symbol is INamedTypeSymbol { IsExtension: not false } namedTypeSymbol && format.CompilerInternalOptions.HasFlag(SymbolDisplayCompilerInternalOptions.UseMetadataMemberNames))
			{
				instance.AddExtensionMarkerName(namedTypeSymbol);
			}
			instance.Free();
		}
		return builder;
	}

	public static string? FormatPrimitive(object? obj, bool quoteStrings, bool useHexadecimalNumbers)
	{
		ObjectDisplayOptions objectDisplayOptions = ObjectDisplayOptions.EscapeNonPrintableCharacters;
		if (quoteStrings)
		{
			objectDisplayOptions |= ObjectDisplayOptions.UseQuotes;
		}
		if (useHexadecimalNumbers)
		{
			objectDisplayOptions |= ObjectDisplayOptions.UseHexadecimalNumbers;
		}
		return ObjectDisplay.FormatPrimitive(obj, objectDisplayOptions);
	}

	public static string FormatLiteral(string value, bool quote)
	{
		ObjectDisplayOptions options = (ObjectDisplayOptions)(0x10 | (quote ? 8 : 0));
		return ObjectDisplay.FormatLiteral(value, options);
	}

	public static string FormatLiteral(char c, bool quote)
	{
		ObjectDisplayOptions options = (ObjectDisplayOptions)(0x10 | (quote ? 8 : 0));
		return ObjectDisplay.FormatLiteral(c, options);
	}
}
