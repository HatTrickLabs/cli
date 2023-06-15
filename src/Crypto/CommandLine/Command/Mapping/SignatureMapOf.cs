using System;
using System.Collections.Generic;
using System.Reflection;

namespace Crypto.CommandLine
{
    public class SignatureMapOf<T> : Map where T : Delegate
    {
        #region internals
        private T _target;
        #endregion

        #region constructors
        public SignatureMapOf(CommandDefinition commandDef) : this(commandDef, null)
        {
        }

        public SignatureMapOf(CommandDefinition commandDef, (string optionKey, string parameterName)[] correlations) : base(commandDef, correlations)
        {
            this.RegisterValidator();
        }
        #endregion

        #region set target
        internal void SetTarget(T target)
        {
            _target = target;
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

            IList<CommandOptionDefinition> options = base.CommandDefinition.Options;

            ParameterInfo[] pInfos = _target.Method.GetParameters();

            for (int i = 0; i < options.Count; i++)
            {
                var op = options[i];

                bool isCorrelated = base.CorrelationExistsForOptionKey(op.Key, out (string opKey, string parameterName) correlation);
                string paramName = isCorrelated ? correlation.parameterName : op.Key;

                var parameter = Array.Find(pInfos, (p) => p.Name == paramName);

                if (parameter is null)
                {
                    string message = $"No parameter found on signature for option '{op.Key}'";

                    if (isCorrelated)
                        message += " via correlation " + correlation;

                    throw new CommandMappingException(message);
                }

                Type valType = op.GetType().GetGenericArguments()[0];

                if (!parameter.ParameterType.IsAssignableFrom(valType))
                {
                    string message = $"Type mismatch mapping option value to parameter...Option key: {op.Key}...Option value type: {valType.Name}...Parameter type: {parameter.ParameterType}";
                    throw new CommandMappingException(message);
                }
            }
        }
        #endregion

        #region map
        public void Map(Command command, out object[] parameters)
        {
            IList<CommandOption> options = command.GetOptions();

            ParameterInfo[] pInfos = _target.Method.GetParameters();

            parameters = new object[pInfos.Length];

            //NOTE: parameters must be in the EXACT order as signature def... 
            //we must work on the set from the order of the parameter infos
            for (int i = 0; i < pInfos.Length; i++)
            {
                var parameter = pInfos[i];

                bool isCorrelated = base.CorrelationExistsForMapTarget(parameter.Name, out (string opKey, string parameterName) correlation);
                string opKey = isCorrelated ? correlation.opKey: parameter.Name;

                parameters[i] = command[opKey].Value;
            }
        }
        #endregion
    }
}
