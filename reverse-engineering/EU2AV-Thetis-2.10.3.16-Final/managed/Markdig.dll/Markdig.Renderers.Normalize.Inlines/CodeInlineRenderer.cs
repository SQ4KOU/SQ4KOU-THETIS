using Markdig.Syntax.Inlines;

namespace Markdig.Renderers.Normalize.Inlines;

public class CodeInlineRenderer : NormalizeObjectRenderer<CodeInline>
{
	protected override void Write(NormalizeRenderer renderer, CodeInline obj)
	{
		int num = 0;
		string content = obj.Content;
		int num2;
		for (num2 = 0; num2 < content.Length; num2++)
		{
			int num3 = content.IndexOf(obj.Delimiter, num2);
			if (num3 == -1)
			{
				break;
			}
			int num4 = 1;
			for (num2 = num3 + 1; num2 < content.Length && content[num2] == obj.Delimiter; num2++)
			{
				num4++;
			}
			if (num < num4)
			{
				num = num4;
			}
		}
		renderer.Write(obj.Delimiter, num + 1);
		if (content.Length != 0)
		{
			if (content[0] == obj.Delimiter)
			{
				renderer.Write(' ');
			}
			renderer.Write(content);
			if (content[content.Length - 1] == obj.Delimiter)
			{
				renderer.Write(' ');
			}
		}
		else
		{
			renderer.Write(' ');
		}
		renderer.Write(obj.Delimiter, num + 1);
	}
}
