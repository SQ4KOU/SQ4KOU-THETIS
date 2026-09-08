using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceEnumConstantSymbol : SourceFieldSymbolWithSyntaxReference
{
	private sealed class ZeroValuedEnumConstantSymbol : SourceEnumConstantSymbol
	{
		public ZeroValuedEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
			: base(containingEnum, syntax, diagnostics)
		{
		}

		protected override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, BindingDiagnosticBag diagnostics)
		{
			return Microsoft.CodeAnalysis.ConstantValue.Default(ContainingType.EnumUnderlyingType.SpecialType);
		}
	}

	private sealed class ExplicitValuedEnumConstantSymbol : SourceEnumConstantSymbol
	{
		public ExplicitValuedEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
			: base(containingEnum, syntax, diagnostics)
		{
		}

		protected override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, BindingDiagnosticBag diagnostics)
		{
			EnumMemberDeclarationSyntax syntaxNode = base.SyntaxNode;
			return ConstantValueUtils.EvaluateFieldConstant(this, syntaxNode.EqualsValue, dependencies, earlyDecodingWellKnownAttributes, diagnostics);
		}
	}

	private sealed class ImplicitValuedEnumConstantSymbol : SourceEnumConstantSymbol
	{
		private readonly SourceEnumConstantSymbol _otherConstant;

		private readonly uint _otherConstantOffset;

		public ImplicitValuedEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, SourceEnumConstantSymbol otherConstant, uint otherConstantOffset, BindingDiagnosticBag diagnostics)
			: base(containingEnum, syntax, diagnostics)
		{
			_otherConstant = otherConstant;
			_otherConstantOffset = otherConstantOffset;
		}

		protected override ConstantValue MakeConstantValue(HashSet<SourceFieldSymbolWithSyntaxReference> dependencies, bool earlyDecodingWellKnownAttributes, BindingDiagnosticBag diagnostics)
		{
			ConstantValue constantValue = _otherConstant.GetConstantValue(new ConstantFieldsInProgress(this, dependencies), earlyDecodingWellKnownAttributes);
			if (constantValue == Microsoft.CodeAnalysis.ConstantValue.Unset)
			{
				return Microsoft.CodeAnalysis.ConstantValue.Unset;
			}
			if (constantValue.IsBad)
			{
				return Microsoft.CodeAnalysis.ConstantValue.Bad;
			}
			if (EnumConstantHelper.OffsetValue(constantValue, _otherConstantOffset, out ConstantValue offsetValue) == EnumOverflowKind.OverflowReport)
			{
				diagnostics.Add(ErrorCode.ERR_EnumeratorOverflow, GetFirstLocation(), this);
			}
			return offsetValue;
		}
	}

	public sealed override RefKind RefKind => RefKind.None;

	public override Symbol AssociatedSymbol => null;

	protected sealed override DeclarationModifiers Modifiers => DeclarationModifiers.Static | DeclarationModifiers.Public | DeclarationModifiers.Const;

	public new EnumMemberDeclarationSyntax SyntaxNode => (EnumMemberDeclarationSyntax)base.SyntaxNode;

	public static SourceEnumConstantSymbol CreateExplicitValuedConstant(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
	{
		return new ExplicitValuedEnumConstantSymbol(containingEnum, syntax, diagnostics);
	}

	public static SourceEnumConstantSymbol CreateImplicitValuedConstant(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, SourceEnumConstantSymbol otherConstant, int otherConstantOffset, BindingDiagnosticBag diagnostics)
	{
		if ((object)otherConstant == null)
		{
			return new ZeroValuedEnumConstantSymbol(containingEnum, syntax, diagnostics);
		}
		return new ImplicitValuedEnumConstantSymbol(containingEnum, syntax, otherConstant, (uint)otherConstantOffset, diagnostics);
	}

	protected SourceEnumConstantSymbol(SourceMemberContainerTypeSymbol containingEnum, EnumMemberDeclarationSyntax syntax, BindingDiagnosticBag diagnostics)
		: base(containingEnum, syntax.Identifier.ValueText, syntax.GetReference(), syntax.Identifier.Span)
	{
		if (Name == "value__")
		{
			diagnostics.Add(ErrorCode.ERR_ReservedEnumerator, ErrorLocation, "value__");
		}
	}

	internal override TypeWithAnnotations GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return TypeWithAnnotations.Create(ContainingType);
	}

	protected override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		if (containingType.AnyMemberHasAttributes)
		{
			return OneOrMany.Create(SyntaxNode.AttributeLists);
		}
		return OneOrMany<SyntaxList<AttributeListSyntax>>.Empty;
	}

	internal sealed override void ForceComplete(SourceLocation? locationOpt, Predicate<Symbol>? filter, CancellationToken cancellationToken)
	{
		if (filter != null && !filter(this))
		{
			return;
		}
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.Type:
				state.NotePartComplete(CompletionPart.Type);
				break;
			case CompletionPart.Members:
				state.NotePartComplete(CompletionPart.Members);
				break;
			case CompletionPart.TypeMembers:
				GetConstantValue(ConstantFieldsInProgress.Empty, earlyDecodingWellKnownAttributes: false);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete(CompletionPart.ImportsAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.TypeParameters | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompletedChecksStarted | CompletionPart.MembersCompleted);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}
}
