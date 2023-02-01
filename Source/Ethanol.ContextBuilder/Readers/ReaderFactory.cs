using Ethanol.ContextBuilder.Attributes;
using Ethanol.ContextBuilder.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ethanol.ContextBuilder.Readers
{
    /// <summary>
    /// Factory class supporting to instantiating flow readers.
    /// </summary>
    public static class ReaderFactory
    {
        /// <summary>
        /// Creates flow reader according the given <paramref name="moduleSpecification"/>.
        /// </summary>
        /// <param name="moduleSpecification">The reader modules specification.</param>
        /// <returns>Reader module or null if a reader of the given name does not exist.</returns>
        public static ReaderModule<IpfixObject> GetReader(ModuleSpecification moduleSpecification)
        {
            if (__readers.Value.TryGetValue(moduleSpecification.Name, out var readerType))
            {
                return CreateObject(readerType, moduleSpecification.Parameters);
            }
            else
                return null;
        }

        private static ReaderModule<IpfixObject> CreateObject(Type readerType, IReadOnlyDictionary<string, string> parameters)
        {
            //call Create method:
            var createMethod = readerType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(m=>m.Name == "Create");
            var newObject = createMethod?.Invoke(null, new object[] { parameters });
            return (ReaderModule<IpfixObject>)newObject;
        }

        static Lazy<Dictionary<string, Type>> __readers = new Lazy<Dictionary<string, Type>>(GetReaderModules);
        private static Dictionary<string, Type> GetReaderModules()
        {
            var readers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes().Where(t => t.GetCustomAttribute<ModuleAttribute>()?.ModuleType == ModuleType.Reader));
            return new Dictionary<string, Type>(readers.Select(x => new KeyValuePair<string, Type>(x.GetCustomAttribute<ModuleAttribute>().Name, x)));
        }
    }
}
