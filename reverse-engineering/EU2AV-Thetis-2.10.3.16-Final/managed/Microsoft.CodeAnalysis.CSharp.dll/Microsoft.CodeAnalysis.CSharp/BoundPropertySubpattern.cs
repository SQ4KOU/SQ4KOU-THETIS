using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPropertySubpattern : BoundSubpattern
{
	public BoundPropertySubpatternMember? Member { get; }

	public bool IsLengthOrCount { get; }

	internal BoundPropertySubpattern WithPattern(BoundPattern pattern)
	{
		return Update(Member, IsLengthOrCount, pattern);
	}

	public BoundPropertySubpattern(SyntaxNode syntax, BoundPropertySubpatternMember? member, bool isLengthOrCount, BoundPattern pattern, bool hasErrors = false)
		: base(BoundKind.PropertySubpattern, syntax, pattern, hasErrors || member.HasErrors() || pattern.HasErrors())
	{
		Member = member;
		IsLengthOrCount = isLengthOrCount;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPropertySubpattern(this);
	}

	public BoundPropertySubpattern Update(BoundPropertySubpatternMember? member, bool isLengthOrCount, BoundPattern pattern)
	{
		if (member != Member || isLengthOrCount != IsLengthOrCount || pattern != base.Pattern)
		{
			BoundPropertySubpattern boundPropertySubpattern = new BoundPropertySubpattern(Syntax, member, isLengthOrCount, pattern, base.HasErrors);
			boundPropertySubpattern.CopyAttributes(this);
			return boundPropertySubpattern;
		}
		return this;
	}
}
