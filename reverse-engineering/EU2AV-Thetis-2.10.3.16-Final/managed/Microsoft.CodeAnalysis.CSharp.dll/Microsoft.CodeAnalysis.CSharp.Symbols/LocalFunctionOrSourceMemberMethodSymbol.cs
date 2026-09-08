using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class LocalFunctionOrSourceMemberMethodSymbol : SourceMethodSymbol
{
	private TypeWithAnnotations.Boxed? _lazyIteratorElementType;

	internal sealed override TypeWithAnnotations IteratorElementTypeWithAnnotations
	{
		get
		{
			if (_lazyIteratorElementType == TypeWithAnnotations.Boxed.Sentinel)
			{
				TypeWithAnnotations value = InMethodBinder.GetIteratorElementTypeFromReturnType(DeclaringCompilation, RefKind, base.ReturnType, null, null);
				if (value.IsDefault)
				{
					value = TypeWithAnnotations.Create(new ExtendedErrorTypeSymbol(DeclaringCompilation, "", 0, null));
				}
				Interlocked.CompareExchange(ref _lazyIteratorElementType, new TypeWithAnnotations.Boxed(value), TypeWithAnnotations.Boxed.Sentinel);
			}
			return _lazyIteratorElementType?.Value ?? default(TypeWithAnnotations);
		}
	}

	internal sealed override bool IsIterator => _lazyIteratorElementType != null;

	protected LocalFunctionOrSourceMemberMethodSymbol(SyntaxReference? syntaxReferenceOpt, bool isIterator)
		: base(syntaxReferenceOpt)
	{
		if (isIterator)
		{
			_lazyIteratorElementType = TypeWithAnnotations.Boxed.Sentinel;
		}
	}
}
