namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FeatureSwitchDefinitionAttribute : Attribute
{
	public string SwitchName { get; }

	public FeatureSwitchDefinitionAttribute(string switchName)
	{
		SwitchName = switchName;
	}
}
