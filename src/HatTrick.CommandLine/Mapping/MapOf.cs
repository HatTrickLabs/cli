using System;
using System.Collections.Generic;
using System.Reflection;

namespace HatTrick.CommandLine
{
    public class MapOf<T> : Map where T : new()
    {
        #region const
        public const string IgnoreMapToken = "~";
        #endregion

        #region constructors
        public MapOf(CommandDefinition commandDef) : this(commandDef, null)
        {
        }

        public MapOf(CommandDefinition commandDef, (string optionKey, string propertyName)[] correlations) : base(commandDef, correlations)
        {
            RegisterValidator();
        }
        #endregion

        #region register validator
        protected override void RegisterValidator()
        {
            CommandDefinition.RegisterMappedHandlerValidator(Validate);
        }
        #endregion

        #region validate
        public override void Validate()
        {
            base.Validate();

            List<CommandOptionDefinition> options = CommandDefinition.Options;

            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties();

            foreach (var op in options)
            {
                bool isCorrelated = CorrelationExistsForOptionKey(op.Key, out (string opKey, string property) correlation);
                string propName = isCorrelated ? correlation.property : op.Key;

                //if correlation map specifies the 'ignore map' token for the property, it means DO NOT attempt to map.
                if (propName == MapOf<T>.IgnoreMapToken)
                    continue;

                var prop = Array.Find(props, (p) => p.Name == propName);

                if (prop is null)
                {
                    string message = $"No property found on '{t.Name}' for option '{op.Key}'";

                    if (isCorrelated)
                        message += " via correlation " + correlation;

                    throw new CommandMappingException(message);
                }

                Type valType = op.GenericType;

                if (!prop.PropertyType.IsAssignableFrom(valType))
                {
                    var opTypeName = TypeMap.GetAliasOrName(op.GenericType);
                    var propTypeName = TypeMap.GetAliasOrName(prop.PropertyType);
                    var msg = $"Type mismatch while mapping option value to target...Option key: {op.Key}...Option type: {opTypeName}...Property type: {propTypeName}";
                    throw new CommandMappingException(msg);
                }
            }
        }
        #endregion

        #region map
        public void Map(Command command, out T to)
        {
            to = new();

            IList<CommandOption> options = command.GetOptions();

            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties();

            foreach (var op in options)
            {
                bool isCorrelated = CorrelationExistsForOptionKey(op.Key, out (string opKey, string property) correlation);
                string propName = isCorrelated ? correlation.property : op.Key;

                //if correlation map specifies the 'ignore map' token for the property, it means DO NOT attempt to map.
                if (propName == MapOf<T>.IgnoreMapToken)
                    continue;

                var prop = Array.Find(props, (p) => p.Name == propName);

                var val = op.GetValue<object>();

                if (val is not null)
                    prop.SetValue(to, val);
            }
        }
        #endregion
    }
}
