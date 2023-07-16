using System;
using System.Collections.Generic;
using System.Reflection;

namespace HatTrick.CommandLine
{
    public class MapOf<T> : Map where T : new()
    {
        #region constructors
        public MapOf(CommandDefinition commandDef) : this(commandDef, null)
        {
        }

        public MapOf(CommandDefinition commandDef, (string optionKey, string propertyName)[] correlations) : base(commandDef, correlations)
        {
            this.RegisterValidator();
        }
        #endregion

        #region register validator
        protected override void RegisterValidator()
        {
            base.CommandDefinition.RegisterMappedHandlerValidator(this.Validate);
        }
        #endregion

        #region validate
        public override void Validate()
        {
            base.Validate();

            List<CommandOptionDefinition> options = base.CommandDefinition.Options;

            Type t = typeof(T);
            PropertyInfo[] props = t.GetProperties();

            foreach (var op in options)
            {
                bool isCorrelated = base.CorrelationExistsForOptionKey(op.Key, out (string opKey, string property) correlation);
                string propName = isCorrelated ? correlation.property : op.Key;

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
                    string message = $"Type mismatch mapping option value to target...Option key: {op.Key}...Option value type: {valType.Name}...Property type: {prop.PropertyType}";
                    throw new CommandMappingException(message);
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
                bool isCorrelated = base.CorrelationExistsForOptionKey(op.Key, out (string opKey, string property) correlation);
                string propName = isCorrelated ? correlation.property : op.Key;

                var prop = Array.Find(props, (p) => p.Name == propName);

                var val = op.Value;

                if (val is not null)
                    prop.SetValue(to, val);
            }
        }
        #endregion
    }
}
