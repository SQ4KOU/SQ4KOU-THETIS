namespace Microsoft.CodeAnalysis;

public enum SarifVersion
{
	Sarif1 = 1,
	Sarif2 = 2,
	Default = Sarif1,
	Latest = int.MaxValue
}
