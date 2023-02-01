using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Attributes
{
    public enum ModuleType { Reader, Writer, Builder }
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleAttribute : Attribute
    {
        public ModuleAttribute(ModuleType moduleType, string Name)
        {
            ModuleType = moduleType;
            this.Name = Name;
        }

        public ModuleType ModuleType { get; }
        public string Name { get; }
    }
}
