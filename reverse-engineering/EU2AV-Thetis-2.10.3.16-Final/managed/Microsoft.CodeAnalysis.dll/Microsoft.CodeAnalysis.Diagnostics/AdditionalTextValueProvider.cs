using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class AdditionalTextValueProvider<TValue>
{
	internal readonly AnalysisValueProvider<AdditionalText, TValue> CoreValueProvider;

	public AdditionalTextValueProvider(Func<AdditionalText, TValue> computeValue, IEqualityComparer<AdditionalText>? additionalTextComparer = null)
	{
		CoreValueProvider = new AnalysisValueProvider<AdditionalText, TValue>(computeValue, additionalTextComparer ?? EqualityComparer<AdditionalText>.Default);
	}
}
