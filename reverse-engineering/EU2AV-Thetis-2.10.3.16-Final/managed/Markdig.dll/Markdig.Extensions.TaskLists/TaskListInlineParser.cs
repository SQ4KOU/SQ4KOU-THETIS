using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Markdig.Extensions.TaskLists;

public class TaskListInlineParser : InlineParser
{
	public string ListClass { get; set; }

	public string ListItemClass { get; set; }

	public TaskListInlineParser()
	{
		base.OpeningCharacters = new char[1] { '[' };
		ListClass = "contains-task-list";
		ListItemClass = "task-list-item";
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		if (!(processor.Block.Parent is ListItemBlock listItemBlock))
		{
			return false;
		}
		int start = slice.Start;
		char c = slice.NextChar();
		if (!c.IsSpace() && c != 'x' && c != 'X')
		{
			return false;
		}
		if (slice.NextChar() != ']')
		{
			return false;
		}
		slice.SkipChar();
		TaskList taskList = new TaskList
		{
			Span = 
			{
				Start = processor.GetSourcePosition(start, out var lineIndex, out var column)
			},
			Line = lineIndex,
			Column = column,
			Checked = !c.IsSpace()
		};
		taskList.Span.End = taskList.Span.Start + 2;
		processor.Inline = taskList;
		if (!string.IsNullOrEmpty(ListItemClass))
		{
			listItemBlock.GetAttributes().AddClass(ListItemClass);
		}
		ListBlock obj = (ListBlock)listItemBlock.Parent;
		if (!string.IsNullOrEmpty(ListClass))
		{
			obj.GetAttributes().AddClass(ListClass);
		}
		return true;
	}
}
