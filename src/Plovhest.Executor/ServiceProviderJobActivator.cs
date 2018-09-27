namespace Plovhest.Executor
{
    using System;
    using Hangfire;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Hangfire job activator for Microsoft.Extensions.DependencyInjection
    /// </summary>
    public class ServiceProviderJobActivator : JobActivator
    {
        private readonly IServiceProvider _sp;
        
        public ServiceProviderJobActivator(IServiceProvider serviceProvider) => _sp = serviceProvider;
        public override object ActivateJob(Type type) => _sp.GetService(type);
        public override JobActivatorScope BeginScope(JobActivatorContext context) => new ServiceProviderScope(_sp);

        private class ServiceProviderScope : JobActivatorScope
        {
            private readonly IServiceScope _scope;
        
            public ServiceProviderScope(IServiceProvider serviceProvider) => _scope = serviceProvider.CreateScope();
            public override object Resolve(Type type) => _scope.ServiceProvider.GetService(type);
            public override void DisposeScope() => _scope?.Dispose();
        }
    }
}
