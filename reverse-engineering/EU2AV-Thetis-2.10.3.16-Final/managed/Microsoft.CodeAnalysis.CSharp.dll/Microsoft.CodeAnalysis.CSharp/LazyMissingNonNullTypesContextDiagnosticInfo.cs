using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LazyMissingNonNullTypesContextDiagnosticInfo : LazyDiagnosticInfo
{
	private readonly TypeWithAnnotations _type;

	private readonly DiagnosticInfo _info;

	private LazyMissingNonNullTypesContextDiagnosticInfo(TypeWithAnnotations type, DiagnosticInfo info)
	{
		_type = type;
		_info = info;
	}

	private LazyMissingNonNullTypesContextDiagnosticInfo(LazyMissingNonNullTypesContextDiagnosticInfo original, DiagnosticSeverity severity)
		: base(original, severity)
	{
		_type = original._type;
		_info = original._info;
	}

	protected override DiagnosticInfo GetInstanceWithSeverityCore(DiagnosticSeverity severity)
	{
		return new LazyMissingNonNullTypesContextDiagnosticInfo(this, severity);
	}

	public static void AddAll(Binder binder, SyntaxToken questionToken, TypeWithAnnotations? type, DiagnosticBag diagnostics)
	{
		Location location = questionToken.GetLocation();
		ArrayBuilder<DiagnosticInfo> instance = ArrayBuilder<DiagnosticInfo>.GetInstance();
		GetRawDiagnosticInfos(binder, questionToken, instance);
		foreach (DiagnosticInfo item in instance)
		{
			DiagnosticInfo info = (type.HasValue ? new LazyMissingNonNullTypesContextDiagnosticInfo(type.Value, item) : item);
			diagnostics.Add(info, location);
		}
		instance.Free();
	}

	private static void GetRawDiagnosticInfos(Binder binder, SyntaxToken questionToken, ArrayBuilder<DiagnosticInfo> infos)
	{
		CSharpSyntaxTree cSharpSyntaxTree = (CSharpSyntaxTree)questionToken.SyntaxTree;
		CSDiagnosticInfo featureAvailabilityDiagnosticInfo = MessageID.IDS_FeatureNullableReferenceTypes.GetFeatureAvailabilityDiagnosticInfo(cSharpSyntaxTree.Options);
		if (featureAvailabilityDiagnosticInfo != null)
		{
			infos.Add(featureAvailabilityDiagnosticInfo);
		}
		if ((featureAvailabilityDiagnosticInfo == null || featureAvailabilityDiagnosticInfo.Severity != DiagnosticSeverity.Error) && !binder.AreNullableAnnotationsEnabled(questionToken))
		{
			ErrorCode code = (cSharpSyntaxTree.IsGeneratedCode(binder.Compilation.Options.SyntaxTreeOptionsProvider, CancellationToken.None) ? ErrorCode.WRN_MissingNonNullTypesContextForAnnotationInGeneratedCode : ErrorCode.WRN_MissingNonNullTypesContextForAnnotation);
			infos.Add(new CSDiagnosticInfo(code));
		}
	}

	internal static bool IsNullableReference(TypeSymbol type)
	{
		if ((object)type != null)
		{
			if (!type.IsValueType)
			{
				return !type.IsErrorType();
			}
			return false;
		}
		return true;
	}

	protected override DiagnosticInfo ResolveInfo()
	{
		if (!IsNullableReference(_type.Type))
		{
			return null;
		}
		return _info;
	}
}
