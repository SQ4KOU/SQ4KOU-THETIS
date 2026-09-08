using System.Threading.Tasks;

namespace Discord;

public interface IUpdateable
{
	Task UpdateAsync(RequestOptions options = null);
}
