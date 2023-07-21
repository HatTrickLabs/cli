using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Reflection.Metadata;

namespace HatTrick.CommandLine
{
    public class SignatureMapOf<T> : Map where T : Delegate
    {
        #region const
        public const string IgnoreMapToken = "~";
        #endregion

        #region internals
        private T _target;
        #endregion

        #region constructors
        public SignatureMapOf(CommandDefinition commandDef) : this(commandDef, null)
        {
        }

        public SignatureMapOf(CommandDefinition commandDef, (string optionKey, string parameterName)[] correlations) : base(commandDef, correlations)
        {
            RegisterValidator();
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
            CommandDefinition.RegisterMappedHandlerValidator(Validate);
        }
        #endregion

        #region validate
        //Yikes, this whole thing def works, but it's getting wild.
        public override void Validate()
        {
            base.Validate();

            List<CommandOptionDefinition> options = base.CommandDefinition.Options;
            bool[] utilized = new bool[options.Count];

            ParameterInfo[] pInfos = _target.Method.GetParameters();

            for (int i = 0; i < pInfos.Length; i++)
            {
                var parameter = pInfos[i];
                bool isCorrelated = base.CorrelationExistsForMapTarget(parameter.Name, out (string opKey, string to) correlation);
                string opKey = isCorrelated ? correlation.opKey : parameter.Name;

                int idx = options.FindIndex(o => o.Key == opKey);
                if (idx == -1)
                {
                    string msg   = $"No option definition found for signature parameter '{parameter.Name}'";

                    if (isCorrelated)
                        msg += " via correlation " + correlation;

                    throw new CommandMappingException(msg);
                }

                var op = options[idx];

                utilized[idx] = true;

                Type valType = op.GenericType;

                if (!parameter.ParameterType.IsAssignableFrom(valType))
                {
                    var opTypeName = TypeMap.GetAliasOrName(op.GenericType);
                    var paramTypeName = TypeMap.GetAliasOrName(parameter.ParameterType);
                    var msg = $"Type mismatch while mapping option value to parameter...Option key: {opKey}...Option type: {opTypeName}...Parameter type: {paramTypeName}";
                    throw new CommandMappingException(msg);
                }
            }


            for (int i = 0; i < utilized.Length; i++)
            {
                if (utilized[i] == false)
                {
                    var op = options[i];
                    bool isCorrelated = base.CorrelationExistsForOptionKey(op.Key, out (string opKey, string to) correlation);

                    if (isCorrelated && correlation.to == SignatureMapOf<T>.IgnoreMapToken)
                        continue;

                    string msg = $"No parameter found on signature for option '{op.Key}'";

                    if (isCorrelated)
                        msg += " via correlation " + correlation;

                    throw new CommandMappingException(msg);
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
            //we also need not be concerned about Ignored ops as we are mapping in the direction param -> op
            for (int i = 0; i < pInfos.Length; i++)
            {
                var parameter = pInfos[i];

                bool isCorrelated = CorrelationExistsForMapTarget(parameter.Name, out (string opKey, string parameterName) correlation);
                string opKey = isCorrelated ? correlation.opKey : parameter.Name;

                parameters[i] = command[opKey].Value;
            }
        }
        #endregion
    }
}
