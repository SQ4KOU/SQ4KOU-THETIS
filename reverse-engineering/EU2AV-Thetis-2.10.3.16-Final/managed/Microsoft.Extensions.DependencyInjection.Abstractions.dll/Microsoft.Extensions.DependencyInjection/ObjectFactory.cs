using System;

namespace Microsoft.Extensions.DependencyInjection;

public delegate object ObjectFactory(IServiceProvider serviceProvider, object?[]? arguments);
public delegate T ObjectFactory<out T>(IServiceProvider serviceProvider, object?[]? arguments);
