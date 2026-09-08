namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal static class MemberDisplayFormatExtensions
{
	internal static bool IsValid(this MemberDisplayFormat value)
	{
		if (value >= MemberDisplayFormat.SingleLine)
		{
			return value <= MemberDisplayFormat.Hidden;
		}
		return false;
	}
}
