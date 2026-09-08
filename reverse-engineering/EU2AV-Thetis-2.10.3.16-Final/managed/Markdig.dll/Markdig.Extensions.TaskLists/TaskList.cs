using System.Diagnostics;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.TaskLists;

[DebuggerDisplay("TaskList {Checked}")]
public class TaskList : LeafInline
{
	public bool Checked { get; set; }
}
