using System.Threading.Tasks;

namespace Discord;

public interface IDeletable
{
	Task DeleteAsync(RequestOptions options = null);
}
