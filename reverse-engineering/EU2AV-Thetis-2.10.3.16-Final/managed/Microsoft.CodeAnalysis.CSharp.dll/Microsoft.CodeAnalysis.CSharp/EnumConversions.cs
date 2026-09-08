namespace Microsoft.CodeAnalysis.CSharp;

internal static class EnumConversions
{
	internal static DeclarationKind ToDeclarationKind(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.ClassDeclaration:
			return DeclarationKind.Class;
		case SyntaxKind.InterfaceDeclaration:
			return DeclarationKind.Interface;
		case SyntaxKind.StructDeclaration:
			return DeclarationKind.Struct;
		case SyntaxKind.NamespaceDeclaration:
		case SyntaxKind.FileScopedNamespaceDeclaration:
			return DeclarationKind.Namespace;
		case SyntaxKind.EnumDeclaration:
			return DeclarationKind.Enum;
		case SyntaxKind.DelegateDeclaration:
			return DeclarationKind.Delegate;
		case SyntaxKind.RecordDeclaration:
			return DeclarationKind.Record;
		case SyntaxKind.RecordStructDeclaration:
			return DeclarationKind.RecordStruct;
		case SyntaxKind.ExtensionBlockDeclaration:
			return DeclarationKind.Extension;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}
}
