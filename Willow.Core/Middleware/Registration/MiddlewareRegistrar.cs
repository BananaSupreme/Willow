﻿using Microsoft.Extensions.DependencyInjection;

using Willow.Core.Middleware.Abstractions;
using Willow.Core.Registration.Abstractions;

namespace Willow.Core.Middleware.Registration;

internal sealed class MiddlewareRegistrar : IServiceRegistrar
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(IMiddlewareBuilderFactory<>), typeof(MiddlewareBuilderFactory<>));
    }
}