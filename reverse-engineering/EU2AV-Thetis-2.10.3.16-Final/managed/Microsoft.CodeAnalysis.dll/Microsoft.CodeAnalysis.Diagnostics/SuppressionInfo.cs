using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class SuppressionInfo
{
	public string Id { get; }

	public AttributeData? Attribute { get; }

	public ImmutableArray<Suppression> ProgrammaticSuppressions { get; }

	internal SuppressionInfo(string id, AttributeData? attribute, ImmutableArray<Suppression> programmaticSuppressions)
	{
		Id = id;
		Attribute = attribute;
		ProgrammaticSuppressions = programmaticSuppressions;
	}
}
