using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Caching;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Aspects.Autofac.Cache
{
    public class CacheRemoveAspect : MethodInterception
    {
        private string _key;
        private ICacheManager _cacheManager;

        public CacheRemoveAspect(string key)
        {
            _key = key;
            _cacheManager = ServiceTool.ServiceProvider.GetService<ICacheManager>();
        }

        protected override void OnSuccess(IInvocation invocation) // method başarıyla tamamlandıysa cache'den silme işlemi gerçekleşir.
        {
            _cacheManager.Remove(_key);
        }
    }
}
