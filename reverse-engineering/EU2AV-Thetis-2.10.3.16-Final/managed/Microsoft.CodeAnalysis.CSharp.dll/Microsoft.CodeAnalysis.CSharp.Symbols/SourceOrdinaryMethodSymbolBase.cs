using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceOrdinaryMethodSymbolBase : SourceOrdinaryMethodOrUserDefinedOperatorSymbol
{
	private readonly string _name;

	public abstract override ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

	public sealed override string Name => _name;

	protected abstract override SourceMemberMethodSymbol BoundAttributesSource { get; }

	protected SourceOrdinaryMethodSymbolBase(NamedTypeSymbol containingType, string name, Location location, CSharpSyntaxNode syntax, bool isIterator, (DeclarationModifiers declarationModifiers, Flags flags) modifiersAndFlags)
		: base(containingType, syntax.GetReference(), location, isIterator, modifiersAndFlags)
	{
		_name = name;
	}

	protected sealed override void LazyAsyncMethodChecks(CancellationToken cancellationToken)
	{
		if (!IsAsync)
		{
			CompleteAsyncMethodChecks(null, cancellationToken);
			return;
		}
		BindingDiagnosticBag instance = BindingDiagnosticBag.GetInstance();
		AsyncMethodChecks(instance);
		CompleteAsyncMethodChecks(instance, cancellationToken);
		instance.Free();
	}

	private void CompleteAsyncMethodChecks(BindingDiagnosticBag diagnosticsOpt, CancellationToken cancellationToken)
	{
		if (state.NotePartComplete(CompletionPart.Members))
		{
			if (diagnosticsOpt != null)
			{
				AddDeclarationDiagnostics(diagnosticsOpt);
			}
			CompleteAsyncMethodChecksBetweenStartAndFinish();
			state.NotePartComplete(CompletionPart.TypeMembers);
		}
		else
		{
			state.SpinWaitComplete(CompletionPart.TypeMembers, cancellationToken);
		}
	}

	protected abstract void CompleteAsyncMethodChecksBetweenStartAndFinish();

	public abstract override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken));

	internal abstract override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations();
}
