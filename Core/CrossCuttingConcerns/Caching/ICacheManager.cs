using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Caching
{
    public interface ICacheManager
    {
        object Get(string key);
        bool IsExists(string key);
        void Remove(string key);
        void Set(string key, object returnValue, int duration);
    }
}
