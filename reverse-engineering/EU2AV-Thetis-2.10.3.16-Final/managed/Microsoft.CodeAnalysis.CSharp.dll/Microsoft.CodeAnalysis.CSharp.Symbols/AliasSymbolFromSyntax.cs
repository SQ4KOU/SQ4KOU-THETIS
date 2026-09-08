using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class AliasSymbolFromSyntax : AliasSymbol
{
	private readonly SyntaxReference _directive;

	private SymbolCompletionState _state;

	private NamespaceOrTypeSymbol? _aliasTarget;

	private BindingDiagnosticBag? _aliasTargetDiagnostics;

	public override NamespaceOrTypeSymbol Target => GetAliasTarget(null);

	internal BindingDiagnosticBag AliasTargetDiagnostics
	{
		get
		{
			GetAliasTarget(null);
			return _aliasTargetDiagnostics;
		}
	}

	internal override bool RequiresCompletion => true;

	internal AliasSymbolFromSyntax(SourceNamespaceSymbol containingSymbol, UsingDirectiveSyntax syntax)
		: base(syntax.Alias.Name.Identifier.ValueText, containingSymbol, ImmutableArray.Create(syntax.Alias.Name.Identifier.GetLocation()), isExtern: false)
	{
		_directive = syntax.GetReference();
	}

	internal AliasSymbolFromSyntax(SourceNamespaceSymbol containingSymbol, ExternAliasDirectiveSyntax syntax)
		: base(syntax.Identifier.ValueText, containingSymbol, ImmutableArray.Create(syntax.Identifier.GetLocation()), isExtern: true)
	{
		_directive = syntax.GetReference();
	}

	internal override NamespaceOrTypeSymbol GetAliasTarget(ConsList<TypeSymbol>? basesBeingResolved)
	{
		if (!_state.HasComplete(CompletionPart.StartBaseType))
		{
			BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
			NamespaceOrTypeSymbol value = (IsExtern ? ResolveExternAliasTarget(instance) : ResolveAliasTarget((UsingDirectiveSyntax)_directive.GetSyntax(), instance, basesBeingResolved));
			if ((object)Interlocked.CompareExchange(ref _aliasTarget, value, null) == null)
			{
				Interlocked.Exchange(ref _aliasTargetDiagnostics, instance);
				_state.NotePartComplete(CompletionPart.StartBaseType);
			}
			else
			{
				instance.Free();
				_state.SpinWaitComplete(CompletionPart.StartBaseType, default(CancellationToken));
			}
		}
		return _aliasTarget;
	}

	private NamespaceSymbol ResolveExternAliasTarget(BindingDiagnosticBag diagnostics)
	{
		if (!ContainingSymbol.DeclaringCompilation.GetExternAliasTarget(Name, out NamespaceSymbol @namespace))
		{
			diagnostics.Add(ErrorCode.ERR_BadExternAlias, GetFirstLocation(), Name);
		}
		return @namespace;
	}

	private NamespaceOrTypeSymbol ResolveAliasTarget(UsingDirectiveSyntax usingDirective, BindingDiagnosticBag diagnostics, ConsList<TypeSymbol>? basesBeingResolved)
	{
		if (usingDirective.UnsafeKeyword != default(SyntaxToken))
		{
			MessageID.IDS_FeatureUsingTypeAlias.CheckFeatureAvailability(diagnostics, usingDirective.UnsafeKeyword);
		}
		else if (!(usingDirective.NamespaceOrType is NameSyntax))
		{
			MessageID.IDS_FeatureUsingTypeAlias.CheckFeatureAvailability(diagnostics, usingDirective.NamespaceOrType);
		}
		TypeSyntax namespaceOrType = usingDirective.NamespaceOrType;
		BinderFlags binderFlags = BinderFlags.SuppressConstraintChecks | BinderFlags.SuppressObsoleteChecks;
		if (usingDirective.UnsafeKeyword != default(SyntaxToken))
		{
			this.CheckUnsafeModifier(DeclarationModifiers.Unsafe, usingDirective.UnsafeKeyword.GetLocation(), diagnostics);
			binderFlags |= BinderFlags.UnsafeRegion;
		}
		else if (!DeclaringCompilation.IsFeatureEnabled(MessageID.IDS_FeatureUsingTypeAlias))
		{
			binderFlags |= BinderFlags.UnsafeRegion;
		}
		Binder.NamespaceOrTypeOrAliasSymbolWithAnnotations namespaceOrTypeOrAliasSymbolWithAnnotations = ContainingSymbol.DeclaringCompilation.GetBinderFactory(namespaceOrType.SyntaxTree).GetBinder(namespaceOrType).WithAdditionalFlags(binderFlags)
			.BindNamespaceOrTypeSymbol(namespaceOrType, diagnostics, basesBeingResolved);
		if (usingDirective.NamespaceOrType is NullableTypeSyntax nullableTypeSyntax && namespaceOrTypeOrAliasSymbolWithAnnotations.TypeWithAnnotations.NullableAnnotation == NullableAnnotation.Annotated && (namespaceOrTypeOrAliasSymbolWithAnnotations.TypeWithAnnotations.Type?.IsReferenceType ?? false))
		{
			diagnostics.Add(ErrorCode.ERR_BadNullableReferenceTypeInUsingAlias, nullableTypeSyntax.QuestionToken.GetLocation());
		}
		NamespaceOrTypeSymbol namespaceOrTypeSymbol = namespaceOrTypeOrAliasSymbolWithAnnotations.NamespaceOrTypeSymbol;
		if (namespaceOrTypeSymbol is TypeSymbol { IsNativeIntegerWrapperType: not false } && (usingDirective.NamespaceOrType.IsNint || usingDirective.NamespaceOrType.IsNuint))
		{
			MessageID.IDS_FeatureUsingTypeAlias.CheckFeatureAvailability(diagnostics, usingDirective.NamespaceOrType);
		}
		return namespaceOrTypeSymbol;
	}
}
