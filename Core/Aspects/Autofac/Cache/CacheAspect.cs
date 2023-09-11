using Castle.DynamicProxy;
using Core.CrossCuttingConcerns.Caching;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Core.Utilities.Results;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Aspects.Autofac.Cache
{
    public class CacheAspect : MethodInterception
    {
        private int _duration;
        private ICacheManager _cacheManager;

        public CacheAspect(int duration = 1)
        {
            _duration = duration;
            _cacheManager = ServiceTool.ServiceProvider.GetService<ICacheManager>();
        }

        public void OnSuccess(IInvocation invocation)
        {
            var methodName = string.Format($"{invocation.Method.ReflectedType.FullName}.{invocation.Method.Name}"); // cache'de unique olması adına key şu şekilde tutulur -> ILocationService.GetLocationsByIpAddress
            var arguments = invocation.Arguments.ToList(); // method parametreleri çekilir
            var key = $"{methodName}({string.Join(",", arguments.Select(x => x?.ToString() ?? "<Null>"))})"; // eğer parametre varsa ILocationService.GetLocationsByIpAddress(parameter) yoksa ILocationService.GetLocationsByIpAddress şeklinde tutulur
            
            if (_cacheManager.IsExists(key))
            {
                var cachedData = _cacheManager.Get(key);
                // Do the correct deserialization here
                var returnType = invocation.Method.ReturnType;
                var genericType = returnType.GetGenericArguments()[0];

                if (cachedData != null && cachedData.GetType() == typeof(SuccessDataResult<>).MakeGenericType(genericType))
                {
                    // JSON verisi SuccessDataResult içeriyorsa, bu türü kullanarak deserialization yapın
                    var successResultType = typeof(SuccessDataResult<>).MakeGenericType(genericType);
                    var deserializedData = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(cachedData), successResultType);
                    invocation.ReturnValue = deserializedData;
                }
                else
                {
                    invocation.Proceed();
                }

                return;
            }

            invocation.Proceed();
            _cacheManager.Set(key, invocation.ReturnValue, _duration);

        }
    }
}


