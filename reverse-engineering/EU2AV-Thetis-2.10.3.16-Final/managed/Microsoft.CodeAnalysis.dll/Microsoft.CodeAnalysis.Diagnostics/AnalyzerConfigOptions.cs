using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class AnalyzerConfigOptions
{
	public static StringComparer KeyComparer { get; } = AnalyzerConfig.Section.PropertiesKeyComparer;

	public virtual IEnumerable<string> Keys
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public abstract bool TryGetValue(string key, [NotNullWhen(true)] out string? value);
}
