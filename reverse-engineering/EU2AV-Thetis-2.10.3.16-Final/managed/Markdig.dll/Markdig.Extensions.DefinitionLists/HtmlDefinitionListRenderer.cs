using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Markdig.Extensions.DefinitionLists;

public class HtmlDefinitionListRenderer : HtmlObjectRenderer<DefinitionList>
{
	protected override void Write(HtmlRenderer renderer, DefinitionList list)
	{
		renderer.EnsureLine();
		renderer.Write("<dl").WriteAttributes(list).WriteLine('>');
		foreach (Block item in list)
		{
			bool flag = false;
			DefinitionItem definitionItem = (DefinitionItem)item;
			int num = 0;
			bool flag2 = false;
			for (int i = 0; i < definitionItem.Count; i++)
			{
				Block block = definitionItem[i];
				if (block is DefinitionTerm definitionTerm)
				{
					if (flag)
					{
						if (!flag2)
						{
							renderer.EnsureLine();
						}
						renderer.WriteLine("</dd>");
						flag2 = false;
						flag = false;
						num = 0;
					}
					renderer.Write("<dt").WriteAttributes(definitionTerm).Write('>');
					renderer.WriteLeafInline(definitionTerm);
					renderer.WriteLine("</dt>");
					continue;
				}
				if (!flag)
				{
					renderer.Write("<dd").WriteAttributes(definitionItem).Write('>');
					num = 0;
					flag = true;
				}
				Block block2 = ((i + 1 < definitionItem.Count) ? definitionItem[i + 1] : null);
				bool num2 = (block2 == null || block2 is DefinitionItem) && num == 0 && block is ParagraphBlock;
				bool implicitParagraph = renderer.ImplicitParagraph;
				if (num2)
				{
					renderer.ImplicitParagraph = true;
					flag2 = true;
				}
				renderer.Write(block);
				renderer.ImplicitParagraph = implicitParagraph;
				num++;
			}
			if (flag)
			{
				if (!flag2)
				{
					renderer.EnsureLine();
				}
				renderer.WriteLine("</dd>");
			}
		}
		renderer.EnsureLine();
		renderer.WriteLine("</dl>");
	}
}
