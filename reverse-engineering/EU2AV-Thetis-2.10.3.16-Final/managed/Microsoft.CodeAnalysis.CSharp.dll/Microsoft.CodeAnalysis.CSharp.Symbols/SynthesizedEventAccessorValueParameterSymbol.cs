using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEventAccessorValueParameterSymbol : SynthesizedAccessorValueParameterSymbol
{
	private SingleInitNullable<TypeWithAnnotations> _lazyParameterType;

	public override TypeWithAnnotations TypeWithAnnotations => _lazyParameterType.Initialize(delegate(SourceEventAccessorSymbol accessor)
	{
		SourceEventSymbol associatedEvent = accessor.AssociatedEvent;
		if (accessor.MethodKind == MethodKind.EventAdd)
		{
			return associatedEvent.TypeWithAnnotations;
		}
		return associatedEvent.IsWindowsRuntimeEvent ? TypeWithAnnotations.Create(associatedEvent.DeclaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationToken)) : associatedEvent.TypeWithAnnotations;
	}, (SourceEventAccessorSymbol)ContainingSymbol);

	public SynthesizedEventAccessorValueParameterSymbol(SourceEventAccessorSymbol accessor, int ordinal)
		: base(accessor, ordinal)
	{
	}
}
