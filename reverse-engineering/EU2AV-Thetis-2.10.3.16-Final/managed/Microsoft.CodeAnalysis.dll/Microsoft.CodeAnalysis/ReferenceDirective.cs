namespace Microsoft.CodeAnalysis;

internal readonly struct ReferenceDirective(string file, Location location)
{
	public readonly string? File = file;

	public readonly Location? Location = location;
}
