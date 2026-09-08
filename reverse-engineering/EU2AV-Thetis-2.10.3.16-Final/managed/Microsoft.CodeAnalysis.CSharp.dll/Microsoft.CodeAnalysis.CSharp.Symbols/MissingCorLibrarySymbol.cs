using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class MissingCorLibrarySymbol : MissingAssemblySymbol
{
	internal static readonly MissingCorLibrarySymbol Instance = new MissingCorLibrarySymbol();

	private NamedTypeSymbol[] _lazySpecialTypes;

	private TypeConversions _lazyTypeConversions;

	internal override TypeConversions TypeConversions
	{
		get
		{
			if (_lazyTypeConversions == null)
			{
				Interlocked.CompareExchange(ref _lazyTypeConversions, new TypeConversions(this), null);
			}
			return _lazyTypeConversions;
		}
	}

	private MissingCorLibrarySymbol()
		: base(new AssemblyIdentity("<Missing Core Assembly>"))
	{
		SetCorLibrary(this);
	}

	internal override NamedTypeSymbol GetDeclaredSpecialType(ExtendedSpecialType type)
	{
		if (_lazySpecialTypes == null)
		{
			Interlocked.CompareExchange(ref _lazySpecialTypes, new NamedTypeSymbol[58], null);
		}
		if ((object)_lazySpecialTypes[(int)type] == null)
		{
			MetadataTypeName fullName = MetadataTypeName.FromFullName(type.GetMetadataName(), useCLSCompliantNameArityEncoding: true);
			NamedTypeSymbol value = new MissingMetadataTypeSymbol.TopLevel(moduleSymbol, ref fullName, type);
			Interlocked.CompareExchange(ref _lazySpecialTypes[(int)type], value, null);
		}
		return _lazySpecialTypes[(int)type];
	}
}
