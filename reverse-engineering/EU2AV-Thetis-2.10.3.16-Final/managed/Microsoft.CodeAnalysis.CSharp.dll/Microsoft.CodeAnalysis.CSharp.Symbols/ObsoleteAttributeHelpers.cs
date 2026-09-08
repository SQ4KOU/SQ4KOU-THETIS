using System;
using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ObsoleteAttributeHelpers
{
	internal static void InitializeObsoleteDataFromMetadata(ref ObsoleteAttributeData data, EntityHandle token, PEModuleSymbol containingModule, bool ignoreByRefLikeMarker, bool ignoreRequiredMemberMarker)
	{
		if (data == ObsoleteAttributeData.Uninitialized)
		{
			ObsoleteAttributeData obsoleteDataFromMetadata = GetObsoleteDataFromMetadata(token, containingModule, ignoreByRefLikeMarker, ignoreRequiredMemberMarker);
			Interlocked.CompareExchange(ref data, obsoleteDataFromMetadata, ObsoleteAttributeData.Uninitialized);
		}
	}

	internal static ObsoleteAttributeData GetObsoleteDataFromMetadata(EntityHandle token, PEModuleSymbol containingModule, bool ignoreByRefLikeMarker, bool ignoreRequiredMemberMarker)
	{
		return containingModule.Module.TryGetDeprecatedOrExperimentalOrObsoleteAttribute(token, new MetadataDecoder(containingModule), ignoreByRefLikeMarker, ignoreRequiredMemberMarker);
	}

	private static ThreeState GetObsoleteContextState(Symbol symbol, bool forceComplete, Func<Symbol, ThreeState> getStateFromSymbol)
	{
		while ((object)symbol != null)
		{
			if (symbol.Kind == SymbolKind.Field)
			{
				Symbol associatedSymbol = ((FieldSymbol)symbol).AssociatedSymbol;
				if ((object)associatedSymbol != null)
				{
					symbol = associatedSymbol;
				}
			}
			if (forceComplete)
			{
				symbol.ForceCompleteObsoleteAttribute();
			}
			ThreeState threeState = getStateFromSymbol(symbol);
			if (threeState != ThreeState.False)
			{
				return threeState;
			}
			symbol = ((!symbol.IsAccessor()) ? symbol.ContainingSymbol : ((MethodSymbol)symbol).AssociatedSymbol);
		}
		return ThreeState.False;
	}

	internal static ObsoleteDiagnosticKind GetObsoleteDiagnosticKind(Symbol symbol, Symbol containingMember, bool forceComplete = false)
	{
		switch (symbol.ObsoleteKind)
		{
		case ObsoleteAttributeKind.None:
			if (symbol.ContainingModule.ObsoleteKind == ObsoleteAttributeKind.Experimental || symbol.ContainingAssembly.ObsoleteKind == ObsoleteAttributeKind.Experimental)
			{
				return getDiagnosticKind(containingMember, forceComplete, (Symbol symbol2) => symbol2.ExperimentalState);
			}
			if (symbol.ContainingModule.ObsoleteKind == ObsoleteAttributeKind.Uninitialized || symbol.ContainingAssembly.ObsoleteKind == ObsoleteAttributeKind.Uninitialized)
			{
				return ObsoleteDiagnosticKind.Lazy;
			}
			return ObsoleteDiagnosticKind.NotObsolete;
		case ObsoleteAttributeKind.WindowsExperimental:
			return ObsoleteDiagnosticKind.Diagnostic;
		case ObsoleteAttributeKind.Experimental:
			return getDiagnosticKind(containingMember, forceComplete, (Symbol symbol2) => symbol2.ExperimentalState);
		case ObsoleteAttributeKind.Uninitialized:
			return ObsoleteDiagnosticKind.Lazy;
		default:
			return getDiagnosticKind(containingMember, forceComplete, (Symbol symbol2) => symbol2.ObsoleteState);
		}
		static ObsoleteDiagnosticKind getDiagnosticKind(Symbol symbol2, bool forceComplete2, Func<Symbol, ThreeState> getStateFromSymbol)
		{
			return GetObsoleteContextState(symbol2, forceComplete2, getStateFromSymbol) switch
			{
				ThreeState.False => ObsoleteDiagnosticKind.Diagnostic, 
				ThreeState.True => ObsoleteDiagnosticKind.Suppressed, 
				_ => ObsoleteDiagnosticKind.LazyPotentiallySuppressed, 
			};
		}
	}

	internal static DiagnosticInfo CreateObsoleteDiagnostic(Symbol symbol, BinderFlags location)
	{
		return createObsoleteDiagnostic(symbol, location);
		static DiagnosticInfo createObsoleteDiagnostic(Symbol symbol2, BinderFlags self)
		{
			ObsoleteAttributeData obsoleteAttributeData = symbol2.ObsoleteAttributeData ?? symbol2.ContainingModule.ObsoleteAttributeData ?? symbol2.ContainingAssembly.ObsoleteAttributeData;
			if (obsoleteAttributeData == null)
			{
				return null;
			}
			if (self.Includes(BinderFlags.SuppressObsoleteChecks))
			{
				return null;
			}
			if (obsoleteAttributeData.Kind == ObsoleteAttributeKind.WindowsExperimental)
			{
				return new CSDiagnosticInfo(ErrorCode.WRN_WindowsExperimental, new FormattedSymbol(symbol2, SymbolDisplayFormat.CSharpErrorMessageFormat));
			}
			if (obsoleteAttributeData.Kind == ObsoleteAttributeKind.Experimental)
			{
				if (string.IsNullOrEmpty(obsoleteAttributeData.Message))
				{
					return new CustomObsoleteDiagnosticInfo(MessageProvider.Instance, 9204, obsoleteAttributeData, new FormattedSymbol(symbol2, SymbolDisplayFormat.CSharpErrorMessageFormat));
				}
				return new CustomObsoleteDiagnosticInfo(MessageProvider.Instance, 9268, obsoleteAttributeData, new FormattedSymbol(symbol2, SymbolDisplayFormat.CSharpErrorMessageFormat), obsoleteAttributeData.Message);
			}
			bool flag = self.Includes(BinderFlags.CollectionInitializerAddMethod);
			string? message = obsoleteAttributeData.Message;
			bool isError = obsoleteAttributeData.IsError;
			ErrorCode errorCode = ((message == null) ? ((!flag) ? ErrorCode.WRN_DeprecatedSymbol : ErrorCode.WRN_DeprecatedCollectionInitAdd) : (isError ? ((!flag) ? ErrorCode.ERR_DeprecatedSymbolStr : ErrorCode.ERR_DeprecatedCollectionInitAddStr) : ((!flag) ? ErrorCode.WRN_DeprecatedSymbolStr : ErrorCode.WRN_DeprecatedCollectionInitAddStr)));
			ErrorCode errorCode2 = errorCode;
			string message2 = obsoleteAttributeData.Message;
			object[] arguments = ((message2 == null) ? new object[1] { symbol2 } : new object[2] { symbol2, message2 });
			return new CustomObsoleteDiagnosticInfo(MessageProvider.Instance, (int)errorCode2, obsoleteAttributeData, arguments);
		}
	}

	internal static bool IsObsoleteDiagnostic(this DiagnosticInfo diagnosticInfo)
	{
		switch ((ErrorCode)diagnosticInfo.Code)
		{
		case ErrorCode.WRN_DeprecatedSymbol:
		case ErrorCode.WRN_DeprecatedSymbolStr:
		case ErrorCode.ERR_DeprecatedSymbolStr:
		case ErrorCode.WRN_DeprecatedCollectionInitAddStr:
		case ErrorCode.ERR_DeprecatedCollectionInitAddStr:
		case ErrorCode.WRN_DeprecatedCollectionInitAdd:
		case ErrorCode.WRN_WindowsExperimental:
		case ErrorCode.WRN_Experimental:
		case ErrorCode.WRN_ExperimentalWithMessage:
			return true;
		default:
			return false;
		}
	}
}
