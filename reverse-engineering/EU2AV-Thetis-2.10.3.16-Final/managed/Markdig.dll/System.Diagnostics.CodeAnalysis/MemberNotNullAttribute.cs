namespace System.Diagnostics.CodeAnalysis;

internal sealed class MemberNotNullAttribute : Attribute
{
	public string[] Members { get; }

	public MemberNotNullAttribute(string member)
	{
		Members = new string[1] { member };
	}

	public MemberNotNullAttribute(params string[] members)
	{
		Members = members;
	}
}
