using System.Threading.Tasks;

namespace Discord.Interactions;

public delegate Task RestResponseCallback(IInteractionContext context, string responseBody);
