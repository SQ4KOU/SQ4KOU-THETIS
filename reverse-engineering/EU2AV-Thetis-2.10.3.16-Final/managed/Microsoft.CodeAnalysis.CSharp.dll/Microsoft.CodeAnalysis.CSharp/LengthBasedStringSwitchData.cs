using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LengthBasedStringSwitchData
{
	internal struct LengthJumpTable(LabelSymbol? nullCaseLabel, ImmutableArray<(int value, LabelSymbol label)> lengthCaseLabels)
	{
		public readonly LabelSymbol? NullCaseLabel = nullCaseLabel;

		public readonly ImmutableArray<(int value, LabelSymbol label)> LengthCaseLabels = lengthCaseLabels;
	}

	internal struct CharJumpTable
	{
		public readonly LabelSymbol Label;

		public readonly int SelectedCharPosition;

		public readonly ImmutableArray<(char value, LabelSymbol label)> CharCaseLabels;

		internal CharJumpTable(LabelSymbol label, int selectedCharPosition, ImmutableArray<(char value, LabelSymbol label)> charCaseLabels)
		{
			Label = label;
			SelectedCharPosition = selectedCharPosition;
			CharCaseLabels = charCaseLabels;
		}
	}

	internal struct StringJumpTable
	{
		public readonly LabelSymbol Label;

		public readonly ImmutableArray<(string value, LabelSymbol label)> StringCaseLabels;

		internal StringJumpTable(LabelSymbol label, ImmutableArray<(string value, LabelSymbol label)> stringCaseLabels)
		{
			Label = label;
			StringCaseLabels = stringCaseLabels;
		}
	}

	internal readonly LengthJumpTable LengthBasedJumpTable;

	internal readonly ImmutableArray<CharJumpTable> CharBasedJumpTables;

	internal readonly ImmutableArray<StringJumpTable> StringBasedJumpTables;

	internal LengthBasedStringSwitchData(LengthJumpTable lengthJumpTable, ImmutableArray<CharJumpTable> charJumpTables, ImmutableArray<StringJumpTable> stringJumpTables)
	{
		LengthBasedJumpTable = lengthJumpTable;
		CharBasedJumpTables = charJumpTables;
		StringBasedJumpTables = stringJumpTables;
	}

	internal bool ShouldGenerateLengthBasedSwitch(int labelsCount)
	{
		if (SwitchStringJumpTableEmitter.ShouldGenerateHashTableSwitch(labelsCount))
		{
			return StringBasedJumpTables.All((StringJumpTable t) => t.StringCaseLabels.Length <= 5);
		}
		return false;
	}

	internal static LengthBasedStringSwitchData Create(ImmutableArray<(ConstantValue value, LabelSymbol label)> inputCases)
	{
		LabelSymbol nullCaseLabel = null;
		foreach (var item2 in inputCases)
		{
			if (item2.value.IsNull)
			{
				nullCaseLabel = item2.label;
			}
		}
		ArrayBuilder<(int, LabelSymbol)> instance = ArrayBuilder<(int, LabelSymbol)>.GetInstance();
		ArrayBuilder<CharJumpTable> instance2 = ArrayBuilder<CharJumpTable>.GetInstance();
		ArrayBuilder<StringJumpTable> instance3 = ArrayBuilder<StringJumpTable>.GetInstance();
		foreach (IGrouping<int, (ConstantValue, LabelSymbol)> item3 in from c in inputCases
			where !c.value.IsNull
			group c by c.value.StringValue.Length)
		{
			int key = item3.Key;
			LabelSymbol item = CreateAndRegisterCharJumpTables(key, item3.SelectAsArray<(ConstantValue, LabelSymbol), (string, LabelSymbol)>(((ConstantValue value, LabelSymbol label) c) => (c.value.StringValue, label: c.label)), instance2, instance3);
			instance.Add((key, item));
		}
		return new LengthBasedStringSwitchData(new LengthJumpTable(nullCaseLabel, instance.ToImmutableAndFree()), instance2.ToImmutableAndFree(), instance3.ToImmutableAndFree());
	}

	private static LabelSymbol CreateAndRegisterCharJumpTables(int stringLength, ImmutableArray<(string value, LabelSymbol label)> casesWithGivenLength, ArrayBuilder<CharJumpTable> charJumpTables, ArrayBuilder<StringJumpTable> stringJumpTables)
	{
		if (stringLength == 0)
		{
			return casesWithGivenLength.Single().label;
		}
		if (casesWithGivenLength.Length == 1)
		{
			return CreateAndRegisterStringJumpTable(casesWithGivenLength, stringJumpTables);
		}
		int bestCharacterPosition = selectBestCharacterIndex(stringLength, casesWithGivenLength);
		ArrayBuilder<(char, LabelSymbol)> instance = ArrayBuilder<(char, LabelSymbol)>.GetInstance();
		foreach (IGrouping<char, (string, LabelSymbol)> item5 in from c in casesWithGivenLength
			group c by c.value[bestCharacterPosition])
		{
			LabelSymbol item = ((stringLength == 1) ? item5.Single().Item2 : CreateAndRegisterStringJumpTable(item5.ToImmutableArray(), stringJumpTables));
			char key = item5.Key;
			instance.Add((key, item));
		}
		CharJumpTable item2 = new CharJumpTable(new GeneratedLabelSymbol("char-dispatch"), bestCharacterPosition, instance.ToImmutableAndFree());
		charJumpTables.Add(item2);
		return item2.Label;
		static (int singleEntryCount, int largestBucket) positionScore(int position, ImmutableArray<(string value, LabelSymbol label)> caseLabels)
		{
			PooledDictionary<char, int> instance2 = PooledDictionary<char, int>.GetInstance();
			foreach (var item6 in caseLabels)
			{
				char key2 = item6.value[position];
				if (instance2.TryGetValue(key2, out var value))
				{
					instance2[key2] = value + 1;
				}
				else
				{
					instance2[key2] = 1;
				}
			}
			int item3 = instance2.Values.Count((int c) => c == 1);
			int item4 = instance2.Values.Max();
			instance2.Free();
			return (singleEntryCount: item3, largestBucket: item4);
		}
		static int selectBestCharacterIndex(int num3, ImmutableArray<(string value, LabelSymbol label)> caseLabels)
		{
			int result = -1;
			int num = -1;
			int num2 = int.MaxValue;
			for (int i = 0; i < num3; i++)
			{
				var (num4, num5) = positionScore(i, caseLabels);
				if (num4 > num || (num4 == num && num5 < num2))
				{
					num = num4;
					num2 = num5;
					result = i;
				}
			}
			return result;
		}
	}

	private static LabelSymbol CreateAndRegisterStringJumpTable(ImmutableArray<(string value, LabelSymbol label)> cases, ArrayBuilder<StringJumpTable> stringJumpTables)
	{
		StringJumpTable item = new StringJumpTable(new GeneratedLabelSymbol("string-dispatch"), cases.SelectAsArray<(string, LabelSymbol), (string, LabelSymbol)>(((string value, LabelSymbol label) c) => (value: c.value, label: c.label)));
		stringJumpTables.Add(item);
		return item.Label;
	}
}
