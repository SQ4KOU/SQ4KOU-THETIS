using System.Diagnostics;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal class MemberFilter
{
	public virtual bool Include(StackFrame frame)
	{
		return Include(frame.GetMethod());
	}

	public virtual bool Include(MemberInfo member)
	{
		return true;
	}
}
