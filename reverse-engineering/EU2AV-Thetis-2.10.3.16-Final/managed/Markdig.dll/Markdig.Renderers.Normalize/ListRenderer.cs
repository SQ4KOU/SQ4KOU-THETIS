using System.Globalization;
using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class ListRenderer : NormalizeObjectRenderer<ListBlock>
{
	protected override void Write(NormalizeRenderer renderer, ListBlock listBlock)
	{
		renderer.EnsureLine();
		bool compactParagraph = renderer.CompactParagraph;
		renderer.CompactParagraph = !listBlock.IsLoose;
		if (listBlock.IsOrdered)
		{
			int result = 0;
			if (listBlock.OrderedStart != null && listBlock.BulletType == '1')
			{
				int.TryParse(listBlock.OrderedStart, out result);
			}
			for (int i = 0; i < listBlock.Count; i++)
			{
				ListItemBlock containerBlock = (ListItemBlock)listBlock[i];
				renderer.EnsureLine();
				renderer.Write(result.ToString(CultureInfo.InvariantCulture));
				renderer.Write(listBlock.OrderedDelimiter);
				renderer.Write(' ');
				renderer.PushIndent(new string(' ', IntLog10Fast(result) + 3));
				renderer.WriteChildren(containerBlock);
				renderer.PopIndent();
				if (listBlock.BulletType == '1')
				{
					result++;
				}
				if (i + 1 < listBlock.Count && listBlock.IsLoose)
				{
					renderer.EnsureLine();
					renderer.WriteLine();
				}
			}
		}
		else
		{
			for (int j = 0; j < listBlock.Count; j++)
			{
				ListItemBlock containerBlock2 = (ListItemBlock)listBlock[j];
				renderer.EnsureLine();
				renderer.Write(renderer.Options.ListItemCharacter ?? listBlock.BulletType);
				renderer.Write(' ');
				renderer.PushIndent("  ");
				renderer.WriteChildren(containerBlock2);
				renderer.PopIndent();
				if (j + 1 < listBlock.Count && listBlock.IsLoose)
				{
					renderer.EnsureLine();
					renderer.WriteLine();
				}
			}
		}
		renderer.CompactParagraph = compactParagraph;
		renderer.FinishBlock(emptyLine: true);
	}

	private static int IntLog10Fast(int input)
	{
		if (input >= 10)
		{
			if (input >= 100)
			{
				if (input >= 1000)
				{
					if (input >= 10000)
					{
						if (input >= 100000)
						{
							if (input >= 1000000)
							{
								if (input >= 10000000)
								{
									if (input >= 100000000)
									{
										if (input >= 1000000000)
										{
											return 9;
										}
										return 8;
									}
									return 7;
								}
								return 6;
							}
							return 5;
						}
						return 4;
					}
					return 3;
				}
				return 2;
			}
			return 1;
		}
		return 0;
	}
}
