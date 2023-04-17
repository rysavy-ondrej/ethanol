using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

public class IndirectStringConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(string);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            if (parser.Current is Scalar scalar)
            {
                var textValue = scalar.Value;
                if (textValue.StartsWith("${"))
                {
                    var variableName = textValue.Trim('$','{','}'); 
                    return Environment.GetEnvironmentVariable(variableName) ?? String.Empty;
                }
                else
                {
                    return textValue;
                }
            }
            return String.Empty;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            emitter.Emit(new Scalar(null, null, value.ToString(), ScalarStyle.Any, true, false));
        }
    }
