using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

public delegate Task ExecuteCallback(IInteractionContext context, object[] args, IServiceProvider serviceProvider, ICommandInfo commandInfo);
