using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisfigureTestMod.Util
{
    public static class Extentions
    {
        public static List<T> ToList<T>(this Il2CppSystem.Collections.Generic.List<T> values)
        {
            List<T> list = new();
            foreach (var item in values)
            {
                list.Add(item);
            }
            return list;
        }   
    }
}
