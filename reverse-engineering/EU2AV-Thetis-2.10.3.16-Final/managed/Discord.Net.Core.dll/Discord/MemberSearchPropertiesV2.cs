namespace Discord;

public class MemberSearchPropertiesV2
{
	public MemberSearchPaginationFilter? After { get; set; }

	public MemberSearchPaginationFilter? Before { get; set; }

	public MemberSearchV2SortType? Sort { get; set; }

	public MemberSearchFilter? AndQuery { get; set; }

	public MemberSearchFilter? OrQuery { get; set; }
}
