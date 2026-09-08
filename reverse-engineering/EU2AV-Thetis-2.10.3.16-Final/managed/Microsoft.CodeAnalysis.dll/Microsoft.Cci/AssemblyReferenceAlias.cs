namespace Microsoft.Cci;

internal readonly struct AssemblyReferenceAlias
{
	public readonly string Name;

	public readonly IAssemblyReference Assembly;

	internal AssemblyReferenceAlias(string name, IAssemblyReference assembly)
	{
		Name = name;
		Assembly = assembly;
	}
}
