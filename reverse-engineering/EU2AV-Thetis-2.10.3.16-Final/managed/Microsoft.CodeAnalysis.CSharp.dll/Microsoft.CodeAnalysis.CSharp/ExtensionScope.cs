namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct ExtensionScope(Binder binder)
{
	public readonly Binder Binder = binder;
}
