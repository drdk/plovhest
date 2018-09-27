using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Plovhest.Executor
{
    public class ServiceProviderJobActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new ServiceProviderScope(_serviceProvider);
        }

        private class ServiceProviderScope : JobActivatorScope
        {
            private readonly IServiceScope _scope;

            public ServiceProviderScope(IServiceProvider serviceProvider)
            {
                _scope = serviceProvider.CreateScope();
            }

            public override object Resolve(Type type)
            {
                return _scope.ServiceProvider.GetService(type);
            }

            public override void DisposeScope()
            {
                _scope?.Dispose();
            }
        }
    }
}
