using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DisfigureModApi.Util
{
    public class ClassReader
    {
        public static string CLassFieldToString(object _input)
        {
            if (_input == null)
            {
                ModApi.Log.LogWarning("Input is null");
                return "No Object Present";
            }

            Type type = _input.GetType();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Class: " + type.Name);

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                sb.AppendLine("Field: " + field.Name + " Value: " + field.GetValue(_input));
            }

            return sb.ToString();
        }
    }
}
