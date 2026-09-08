using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class CSharpDefinitionMap(IEnumerable<SemanticEdit> edits, MetadataDecoder metadataDecoder, CSharpSymbolMatcher previousSourceToMetadata, CSharpSymbolMatcher sourceToMetadata, CSharpSymbolMatcher? sourceToPreviousSource, EmitBaseline baseline) : DefinitionMap(edits, baseline)
{
	private readonly MetadataDecoder _metadataDecoder = metadataDecoder;

	private readonly CSharpSymbolMatcher _sourceToPrevious = sourceToPreviousSource ?? sourceToMetadata;

	public override SymbolMatcher SourceToMetadataSymbolMatcher { get; } = sourceToMetadata;

	public override SymbolMatcher SourceToPreviousSymbolMatcher => _sourceToPrevious;

	public override SymbolMatcher PreviousSourceToMetadataSymbolMatcher { get; } = previousSourceToMetadata;

	internal override CommonMessageProvider MessageProvider => Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance;

	protected override ISymbolInternal? GetISymbolInternalOrNull(ISymbol symbol)
	{
		return (symbol as Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.Symbol)?.UnderlyingSymbol;
	}

	protected override LambdaSyntaxFacts GetLambdaSyntaxFacts()
	{
		return CSharpLambdaSyntaxFacts.Instance;
	}

	internal bool TryGetAnonymousTypeValue(AnonymousTypeManager.AnonymousTypeOrDelegateTemplateSymbol template, out AnonymousTypeValue typeValue)
	{
		return _sourceToPrevious.TryGetAnonymousTypeValue(template, out typeValue);
	}

	protected override void GetStateMachineFieldMapFromMetadata(ITypeSymbolInternal stateMachineType, ImmutableArray<LocalSlotDebugInfo> localSlotDebugInfo, out IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap, out IReadOnlyDictionary<ITypeReference, int> awaiterMap, out int awaiterSlotCount)
	{
		Dictionary<EncHoistedLocalInfo, int> dictionary = new Dictionary<EncHoistedLocalInfo, int>();
		Dictionary<ITypeReference, int> dictionary2 = new Dictionary<ITypeReference, int>(SymbolEquivalentEqualityComparer.Instance);
		int num = -1;
		foreach (Symbol member in ((Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol)stateMachineType).GetMembers())
		{
			if (member.Kind != SymbolKind.Field)
			{
				continue;
			}
			string name = member.Name;
			int slotIndex;
			switch (GeneratedNameParser.GetKind(name))
			{
			case GeneratedNameKind.AwaiterField:
				if (GeneratedNameParser.TryParseSlotIndex(name, out slotIndex))
				{
					Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol fieldSymbol2 = (Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol)member;
					dictionary2[(ITypeReference)fieldSymbol2.Type.GetCciAdapter()] = slotIndex;
					if (slotIndex > num)
					{
						num = slotIndex;
					}
				}
				break;
			case GeneratedNameKind.HoistedLocalField:
			case GeneratedNameKind.DisplayClassLocalOrField:
			case GeneratedNameKind.HoistedSynthesizedLocalField:
				if (GeneratedNameParser.TryParseSlotIndex(name, out slotIndex))
				{
					Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol fieldSymbol = (Microsoft.CodeAnalysis.CSharp.Symbols.FieldSymbol)member;
					if (slotIndex < localSlotDebugInfo.Length)
					{
						EncHoistedLocalInfo key = new EncHoistedLocalInfo(localSlotDebugInfo[slotIndex], (ITypeReference)fieldSymbol.Type.GetCciAdapter());
						dictionary[key] = slotIndex;
					}
				}
				break;
			}
		}
		hoistedLocalMap = dictionary;
		awaiterMap = dictionary2;
		awaiterSlotCount = num + 1;
	}

	protected override ImmutableArray<EncLocalInfo> GetLocalSlotMapFromMetadata(StandaloneSignatureHandle handle, EditAndContinueMethodDebugInformation debugInfo)
	{
		ImmutableArray<LocalInfo<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol>> localsOrThrow = _metadataDecoder.GetLocalsOrThrow(handle);
		return CreateLocalSlotMap(debugInfo, localsOrThrow);
	}

	protected override ITypeSymbolInternal? TryGetStateMachineType(MethodDefinitionHandle methodHandle)
	{
		if (!_metadataDecoder.Module.HasStateMachineAttribute(methodHandle, out var stateMachineTypeName))
		{
			return null;
		}
		return _metadataDecoder.GetTypeSymbolForSerializedType(stateMachineTypeName);
	}

	protected override IMethodSymbolInternal GetMethodSymbol(MethodDefinitionHandle methodHandle)
	{
		return (IMethodSymbolInternal)_metadataDecoder.GetSymbolForILToken(methodHandle);
	}

	private static ImmutableArray<EncLocalInfo> CreateLocalSlotMap(EditAndContinueMethodDebugInformation methodEncInfo, ImmutableArray<LocalInfo<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol>> slotMetadata)
	{
		EncLocalInfo[] array = new EncLocalInfo[slotMetadata.Length];
		ImmutableArray<LocalSlotDebugInfo> localSlots = methodEncInfo.LocalSlots;
		if (!localSlots.IsDefault)
		{
			int num = Math.Min(localSlots.Length, slotMetadata.Length);
			Dictionary<EncLocalInfo, int> dictionary = new Dictionary<EncLocalInfo, int>();
			for (int i = 0; i < num; i++)
			{
				LocalSlotDebugInfo slotInfo = localSlots[i];
				if (slotInfo.SynthesizedKind.IsLongLived())
				{
					LocalInfo<Microsoft.CodeAnalysis.CSharp.Symbols.TypeSymbol> localInfo = slotMetadata[i];
					if (localInfo.CustomModifiers.IsDefaultOrEmpty)
					{
						EncLocalInfo key = new EncLocalInfo(slotInfo, (ITypeReference)localInfo.Type.GetCciAdapter(), localInfo.Constraints, localInfo.SignatureOpt);
						dictionary.Add(key, i);
					}
				}
			}
			foreach (KeyValuePair<EncLocalInfo, int> item in dictionary)
			{
				array[item.Value] = item.Key;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].IsDefault)
			{
				array[j] = new EncLocalInfo(slotMetadata[j].SignatureOpt);
			}
		}
		return ImmutableArray.Create(array);
	}

	protected override bool TryParseDisplayClassOrLambdaName(string name, out int suffixIndex, out char idSeparator, out bool isDisplayClass, out bool isDisplayClassParentField, out bool hasDebugIds)
	{
		suffixIndex = 0;
		isDisplayClass = false;
		isDisplayClassParentField = false;
		hasDebugIds = false;
		idSeparator = '_';
		if (!GeneratedNameParser.TryParseGeneratedName(name, out var kind, out var _, out var closeBracketOffset))
		{
			return false;
		}
		if ((kind != GeneratedNameKind.DisplayClassLocalOrField && (uint)(kind - 98) > 1u && kind != GeneratedNameKind.LocalFunction) || 1 == 0)
		{
			return false;
		}
		isDisplayClass = kind == GeneratedNameKind.LambdaDisplayClass;
		isDisplayClassParentField = kind == GeneratedNameKind.DisplayClassLocalOrField;
		suffixIndex = closeBracketOffset + 2;
		hasDebugIds = !isDisplayClassParentField && System.MemoryExtensions.AsSpan(name, suffixIndex).StartsWith(System.MemoryExtensions.AsSpan("__"), StringComparison.Ordinal);
		if (hasDebugIds)
		{
			suffixIndex += "__".Length;
		}
		return true;
	}
}
