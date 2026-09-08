using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Diagnostics.CSharp;

[DiagnosticAnalyzer("C#", new string[] { })]
internal sealed class CSharpCompilerDiagnosticAnalyzer : CompilerDiagnosticAnalyzer
{
	protected override CommonMessageProvider MessageProvider => Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance;

	internal override ImmutableArray<int> GetSupportedErrorCodes()
	{
		Array values = Enum.GetValues(typeof(ErrorCode));
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(values.Length);
		foreach (ErrorCode item in values)
		{
			bool flag = !ErrorFacts.IsBuildOnlyDiagnostic(item);
			if (flag)
			{
				bool flag2 = (uint)(item - -2) <= 1u;
				flag = !flag2;
			}
			if (flag)
			{
				instance.Add((int)item);
			}
		}
		return instance.ToImmutableAndFree();
	}
}
