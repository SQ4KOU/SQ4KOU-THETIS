using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CSDiagnosticInfo : DiagnosticInfoWithSymbols
{
	public static readonly DiagnosticInfo EmptyErrorInfo = new CSDiagnosticInfo((ErrorCode)0);

	public static readonly DiagnosticInfo VoidDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.Void);

	private readonly IReadOnlyList<Location> _additionalLocations;

	public override IReadOnlyList<Location> AdditionalLocations => _additionalLocations;

	internal new ErrorCode Code => (ErrorCode)base.Code;

	internal CSDiagnosticInfo(ErrorCode code)
		: this(code, Array.Empty<object>(), ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty)
	{
	}

	internal CSDiagnosticInfo(ErrorCode code, params object[] args)
		: this(code, args, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty)
	{
	}

	internal CSDiagnosticInfo(ErrorCode code, ImmutableArray<Symbol> symbols, object[] args)
		: this(code, args, symbols, ImmutableArray<Location>.Empty)
	{
	}

	internal CSDiagnosticInfo(ErrorCode code, object[] args, ImmutableArray<Symbol> symbols, ImmutableArray<Location> additionalLocations)
		: base(code, args, symbols)
	{
		IReadOnlyList<Location> additionalLocations2;
		if (!additionalLocations.IsDefaultOrEmpty)
		{
			IReadOnlyList<Location> readOnlyList = additionalLocations;
			additionalLocations2 = readOnlyList;
		}
		else
		{
			additionalLocations2 = SpecializedCollections.EmptyReadOnlyList<Location>();
		}
		_additionalLocations = additionalLocations2;
	}

	private CSDiagnosticInfo(CSDiagnosticInfo original, DiagnosticSeverity severity)
		: base(original, severity)
	{
		_additionalLocations = original._additionalLocations;
	}

	protected override DiagnosticInfo GetInstanceWithSeverityCore(DiagnosticSeverity severity)
	{
		return new CSDiagnosticInfo(this, severity);
	}

	internal static bool IsEmpty(DiagnosticInfo info)
	{
		return info == EmptyErrorInfo;
	}
}
