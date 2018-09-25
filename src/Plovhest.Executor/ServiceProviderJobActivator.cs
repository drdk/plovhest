using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Hangfire;

namespace Plovhest.Executor
{
    public class ServiceProviderJobActivator : JobActivator
    {
        private IServiceProvider _serviceProvider;

        public ServiceProviderJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type type)
        {
            return _serviceProvider.GetService(type);
        }
    }
}
