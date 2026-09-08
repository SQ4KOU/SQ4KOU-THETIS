namespace ExCSS;

internal sealed class ScriptingMediaFeature : MediaFeature
{
	private static readonly IValueConverter TheConverter = Map.ScriptingStates.ToConverter();

	internal override IValueConverter Converter => TheConverter;

	public ScriptingMediaFeature()
		: base(FeatureNames.Scripting)
	{
	}
}
