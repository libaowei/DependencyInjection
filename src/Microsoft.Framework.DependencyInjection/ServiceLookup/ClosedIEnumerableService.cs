// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Framework.DependencyInjection.ServiceLookup
{
    internal class ClosedIEnumerableService : IService
    {
        private readonly Type _itemType;
        private readonly ServiceEntry _serviceEntry;

        public ClosedIEnumerableService(Type itemType, ServiceEntry entry)
        {
            _itemType = itemType;
            _serviceEntry = entry;
        }

        public IService Next { get; set; }

        public LifecycleKind Lifecycle
        {
            get { return LifecycleKind.Transient; }
        }

        public object Create(ServiceProvider provider)
        {
            return CreateCallSite(provider).Invoke(provider);
        }

        public IServiceCallSite CreateCallSite(ServiceProvider provider)
        {
            var list = new List<IServiceCallSite>();
            for (var service = _serviceEntry.First; service != null; service = service.Next)
            {
                list.Add(service.CreateCallSite(provider));
            }
            return new CallSite(_itemType, list.ToArray());
        }

        private class CallSite : IServiceCallSite
        {
            private readonly Type _itemType;
            private readonly IServiceCallSite[] _serviceCallSites;

            public CallSite(Type itemType, IServiceCallSite[] serviceCallSites)
            {
                _itemType = itemType;
                _serviceCallSites = serviceCallSites;
            }

            public object Invoke(ServiceProvider provider)
            {
                var array = Array.CreateInstance(_itemType, _serviceCallSites.Length);
                for (var index = 0; index != _serviceCallSites.Length; ++index)
                {
                    array.SetValue(_serviceCallSites[index].Invoke(provider), index);
                }
                return array;
            }

            public Expression Build(Expression provider)
            {
                throw new NotImplementedException();
            }
        }
    }
}
