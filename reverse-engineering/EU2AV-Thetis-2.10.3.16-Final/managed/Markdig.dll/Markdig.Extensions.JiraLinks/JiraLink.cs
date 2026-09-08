using System.Diagnostics;
using Markdig.Helpers;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.JiraLinks;

[DebuggerDisplay("{ProjectKey}-{Issue}")]
public class JiraLink : LinkInline
{
	public StringSlice ProjectKey { get; set; }

	public StringSlice Issue { get; set; }

	public JiraLink()
	{
		base.IsClosed = true;
	}
}
