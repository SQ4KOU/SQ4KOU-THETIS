namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedRecordOrdinaryMethod : SynthesizedSourceOrdinaryMethodSymbol
{
	private readonly int _memberOffset;

	protected SynthesizedRecordOrdinaryMethod(SourceMemberContainerTypeSymbol containingType, string name, int memberOffset, DeclarationModifiers declarationModifiers)
		: base(containingType, name, containingType.GetFirstLocation(), (CSharpSyntaxNode)containingType.SyntaxReferences[0].GetSyntax(), (declarationModifiers: declarationModifiers, flags: SourceMemberMethodSymbol.MakeFlags(MethodKind.Ordinary, RefKind.None, declarationModifiers, returnsVoid: false, returnsVoidIsSet: false, isExpressionBodied: false, isExtensionMethod: false, isNullableAnalysisEnabled: false, isVarArg: false, isExplicitInterfaceImplementation: false, hasThisInitializer: false)))
	{
		_memberOffset = memberOffset;
	}

	internal sealed override LexicalSortKey GetLexicalSortKey()
	{
		return LexicalSortKey.GetSynthesizedMemberKey(_memberOffset);
	}
}
